// See https://github.com/SolidEdgeCommunity/SolidEdge.Community for documentation.
// Useful Package Manager Console Commands: https://github.com/SolidEdgeCommunity/SolidEdge.Community/wiki/Package-Manager-Console-Powershell-Reference
// Register-SolidEdgeAddIn
// Unregister-SolidEdgeAddIn
// Set-DebugSolidEdge
// Install-SolidEdgeAddInRibbonSchema

using SolidEdgeCommunity.Extensions; // https://github.com/SolidEdgeCommunity/SolidEdge.Community/wiki/Using-Extension-Methods
using System;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Windows.Forms;
using SolidEdgeFramework;
using System.Windows.Input;
using System.Net.Http;
using SolidEdgePart;
using SolidEdgeGeometry;

namespace DemoAddIn
{
    [ComVisible(true)]
    [Guid("BF1C1BB8-75EE-444A-8DCE-0F1521D0764B")] // Must be unique!
    [ProgId("SolidEdgeCommunity.Samples.DemoAddIn")] // Must be unique!
    public class DemoAddIn :
        SolidEdgeCommunity.AddIn.SolidEdgeAddIn, // Solid Edge Community provided base class.
        SolidEdgeFramework.ISEApplicationEvents,  // Solid Edge Application Events
        SolidEdgeFramework.ISEApplicationWindowEvents, // Solid Edge Window Events
        SolidEdgeFramework.ISEFeatureLibraryEvents, // Solid Edge Feature Library Events
        SolidEdgeFramework.ISEFileUIEvents, // Solid Egde File UI Events
        SolidEdgeFramework.ISENewFileUIEvents, // Solid Egde New File UI Events
        SolidEdgeFramework.ISEECEvents, // Solid Edge EC Events
        SolidEdgeFramework.ISEShortCutMenuEvents, // Solid Edge Shortcut Menu Events
        SolidEdgeFramework.ISEMouseEvents // Solid Edge Mouse Events
    {
        private SolidEdgeCommunity.ConnectionPointController _connectionPointController;
        private static readonly HttpClient _client = new HttpClient();
        private static bool _getting_suggestions = false;
        private static SolidEdgeFramework.Command _cmd = null;
        private static SolidEdgeFramework.Mouse _mouse = null;
        private static SolidEdgeFramework.Application _application = null;
        #region SolidEdgeCommunity.AddIn.SolidEdgeAddIn overrides

        /// <summary>
        /// Called when the addin is first loaded by Solid Edge.
        /// </summary>
        public override void OnConnection(SolidEdgeFramework.Application application, SolidEdgeFramework.SeConnectMode ConnectMode, SolidEdgeFramework.AddIn AddInInstance)
        {
            _application = application;
            
            // If you makes changes to your ribbon, be sure to increment the GuiVersion or your ribbon
            // will not initialize properly.
            AddInEx.GuiVersion = 1;

            // Create an instance of the default connection point controller. It helps manage connections to COM events.
            _connectionPointController = new SolidEdgeCommunity.ConnectionPointController(this);

            // Uncomment the following line to attach to the Solid Edge Application Events.
            _connectionPointController.AdviseSink<SolidEdgeFramework.ISEApplicationEvents>(this.Application);

            // Not necessary unless you absolutely need to see low level windows messages.
            // Uncomment the following line to attach to the Solid Edge Application Window Events.
            //_connectionPointController.AdviseSink<SolidEdgeFramework.ISEApplicationWindowEvents>(this.Application);

            // Uncomment the following line to attach to the Solid Edge Feature Library Events.
            _connectionPointController.AdviseSink<SolidEdgeFramework.ISEFeatureLibraryEvents>(this.Application);

            // Uncomment the following line to attach to the Solid Edge File UI Events.
            //_connectionPointController.AdviseSink<SolidEdgeFramework.ISEFileUIEvents>(this.Application);

            // Uncomment the following line to attach to the Solid Edge File New UI Events.
            //_connectionPointController.AdviseSink<SolidEdgeFramework.ISENewFileUIEvents>(this.Application);

            // Uncomment the following line to attach to the Solid Edge EC Events.
            //_connectionPointController.AdviseSink<SolidEdgeFramework.ISEECEvents>(this.Application);

            // Uncomment the following line to attach to the Solid Edge Shortcut Menu Events.
            //_connectionPointController.AdviseSink<SolidEdgeFramework.ISEShortCutMenuEvents>(this.Application);

        }

        /// <summary>
        /// Called when the addin first connects to a new Solid Edge environment.
        /// </summary>
        public override void OnConnectToEnvironment(SolidEdgeFramework.Environment environment, bool firstTime)
        {
            if (environment.GetCategoryId().Equals(SolidEdgeSDK.EnvironmentCategories.Part))
            {// Uncomment the following line to attach to the Solid Edge Mouse Events.
                _cmd = (SolidEdgeFramework.Command)_application.CreateCommand((int)SolidEdgeConstants.seCmdFlag.seNoDeactivate);
                _cmd.Start();
                ConnectMouse();
            }
        }

        /// <summary>
        /// Called when the addin is about to be unloaded by Solid Edge.
        /// </summary>
        public override void OnDisconnection(SolidEdgeFramework.SeDisconnectMode DisconnectMode)
        {
            // Disconnect from all COM events.
            _connectionPointController.UnadviseAllSinks();
        }

        /// <summary>
        /// Called when Solid Edge raises the SolidEdgeFramework.ISEAddInEdgeBarEvents[Ex].AddPage() event.
        /// </summary>
        public override void OnCreateEdgeBarPage(SolidEdgeCommunity.AddIn.EdgeBarController controller, SolidEdgeFramework.SolidEdgeDocument document)
        {
            // Note: Confirmed with Solid Edge development, OnCreateEdgeBarPage does not get called when Solid Edge is first open and the first document is open.
            // i.e. Under the hood, SolidEdgeFramework.ISEAddInEdgeBarEvents[Ex].AddPage() is not getting called.
            // As an alternative, you can call DemoAddIn.Instance.EdgeBarController.Add() in some other event if you need.

            // Get the document type of the passed in document.
            var documentType = document.Type;
            var imageId = 1;

            // Depending on the document type, you may have different edgebar controls.
            switch (documentType)
            {
                case SolidEdgeFramework.DocumentTypeConstants.igAssemblyDocument:
                case SolidEdgeFramework.DocumentTypeConstants.igDraftDocument:
                case SolidEdgeFramework.DocumentTypeConstants.igPartDocument:
                case SolidEdgeFramework.DocumentTypeConstants.igSheetMetalDocument:
                    controller.Add<MyEdgeBarControl>(document, imageId);
                    break;
            }
        }

        /// <summary>
        /// Called directly after OnConnectToEnvironment() to give you an opportunity to configure a ribbon for a specific environment.
        /// </summary>
        public override void OnCreateRibbon(SolidEdgeCommunity.AddIn.RibbonController controller, Guid environmentCategory, bool firstTime)
        {
            // Depending on environment, you may or may not want to load different ribbons.
            if (environmentCategory.Equals(SolidEdgeSDK.EnvironmentCategories.Assembly))
            {
                // Assembly Environment
                controller.Add<Ribbon3d>(environmentCategory, firstTime);
            }
            else if (environmentCategory.Equals(SolidEdgeSDK.EnvironmentCategories.Draft))
            {
                // Draft Environment
                controller.Add<Ribbon2d>(environmentCategory, firstTime);
            }
            else if (environmentCategory.Equals(SolidEdgeSDK.EnvironmentCategories.Part))
            {
                // Traditional Part Environment
                controller.Add<Ribbon3d>(environmentCategory, firstTime);
            }
            else if (environmentCategory.Equals(SolidEdgeSDK.EnvironmentCategories.DMPart))
            {
                // Synchronous Part Environment
                controller.Add<Ribbon3d>(environmentCategory, firstTime);
            }
            else if (environmentCategory.Equals(SolidEdgeSDK.EnvironmentCategories.SheetMetal))
            {
                // Traditional SheetMetal Environment
                controller.Add<Ribbon3d>(environmentCategory, firstTime);
            }
            else if (environmentCategory.Equals(SolidEdgeSDK.EnvironmentCategories.DMSheetMetal))
            {
                // Synchronous SheetMetal Environment
                controller.Add<Ribbon3d>(environmentCategory, firstTime);
            }
        }

        #endregion

        #region SolidEdgeFramework.ISEApplicationEvents

        /// <summary>
        /// Occurs after the active document changes.
        /// </summary>
        public void AfterActiveDocumentChange(object theDocument)
        {
            var document = theDocument as SolidEdgeFramework.SolidEdgeDocument;
        }

        /// <summary>
        /// Occurs after a specified command is run.
        /// </summary>
        public void AfterCommandRun(int theCommandID)
        {
        }

        /// <summary>
        /// Occurs after a specified document is opened.
        /// </summary>
        public void AfterDocumentOpen(object theDocument)
        {
            var document = theDocument as SolidEdgeFramework.SolidEdgeDocument;
        }

        /// <summary>
        /// Occurs after a specified document is printed.
        /// </summary>
        public void AfterDocumentPrint(object theDocument, int hDC, ref double ModelToDC, ref int Rect)
        {
            var document = theDocument as SolidEdgeFramework.SolidEdgeDocument;
        }

        /// <summary>
        /// Occurs when a specified document is saved.
        /// </summary>
        public void AfterDocumentSave(object theDocument)
        {
            var document = theDocument as SolidEdgeFramework.SolidEdgeDocument;
        }

        /// <summary>
        /// Occurs when a specified environment is activated.
        /// </summary>
        public void AfterEnvironmentActivate(object theEnvironment)
        {
            var environment = theEnvironment as SolidEdgeFramework.Environment;
        }

        /// <summary>
        /// Occurs after a new document is opened.
        /// </summary>
        public void AfterNewDocumentOpen(object theDocument)
        {
            var document = theDocument as SolidEdgeFramework.SolidEdgeDocument;
        }

        /// <summary>
        /// Occurs after a new window is created.
        /// </summary>
        public void AfterNewWindow(object theWindow)
        {
            // Could be either a SolidEdgeFramework.Window (3D) or SolidEdgeDraft.SheetWindow (2D).
            var window = theWindow as SolidEdgeFramework.Window;
            var sheetWindow = theWindow as SolidEdgeDraft.SheetWindow;
        }

        /// <summary>
        /// Occurs after a specified window is activated.
        /// </summary>
        public void AfterWindowActivate(object theWindow)
        {
            // Could be either a SolidEdgeFramework.Window (3D) or SolidEdgeDraft.SheetWindow (2D).
            var window = theWindow as SolidEdgeFramework.Window;
            var sheetWindow = theWindow as SolidEdgeDraft.SheetWindow;
        }

        /// <summary>
        /// Occurs before a specified command is run.
        /// </summary>
        async public void BeforeCommandRun(int theCommandID)
        {
            if (theCommandID == 10307)
            {
                ConnectMouse();
            }
        }

        /// <summary>
        /// Occurs before a specified document is closed.
        /// </summary>
        public void BeforeDocumentClose(object theDocument)
        {
            var document = theDocument as SolidEdgeFramework.SolidEdgeDocument;
        }

        /// <summary>
        /// Occurs before a specified document is printed.
        /// </summary>
        public void BeforeDocumentPrint(object theDocument, int hDC, ref double ModelToDC, ref int Rect)
        {
            var document = theDocument as SolidEdgeFramework.SolidEdgeDocument;
        }

        /// <summary>
        /// Occurs before a specified document is saved.
        /// </summary>
        public void BeforeDocumentSave(object theDocument)
        {
            var document = theDocument as SolidEdgeFramework.SolidEdgeDocument;
        }

        /// <summary>
        /// Occurs before a specified environment is deactivated.
        /// </summary>
        public void BeforeEnvironmentDeactivate(object theEnvironment)
        {
            var environment = theEnvironment as SolidEdgeFramework.Environment;
        }

        /// <summary>
        /// Occurs before the associated application is closed.
        /// </summary>
        public void BeforeQuit()
        {
        }

        /// <summary>
        /// Occurs before a specified window is deactivated.
        /// </summary>
        public void BeforeWindowDeactivate(object theWindow)
        {
            // Could be either a SolidEdgeFramework.Window (3D) or SolidEdgeDraft.SheetWindow (2D).
            var window = theWindow as SolidEdgeFramework.Window;
            var sheetWindow = theWindow as SolidEdgeDraft.SheetWindow;
        }

        #endregion

        #region SolidEdgeFramework.ISEApplicationWindowEvents

        /// <summary>
        /// Occurs when the application receives a window event.
        /// </summary>
        public void WindowProc(int hWnd, int nMsg, int wParam, int lParam)
        {
        }

        #endregion

        #region SolidEdgeFramework.ISEFeatureLibraryEvents

        /// <summary>
        /// Occurs when a new feature library file is created.
        /// </summary>
        public void AfterFeatureLibraryDocumentCreated(string Name)
        {
        }

        /// <summary>
        /// Occurs when a new feature library file is deleted.
        /// </summary>
        public void AfterFeatureLibraryDocumentDeleted(string Name)
        {
        }

        /// <summary>
        /// Occurs when a user executes the Rename command on a feature library file.
        /// </summary>
        public void AfterFeatureLibraryDocumentRenamed(string NewName, string OldName)
        {
        }

        #endregion

        # region SolidEdgeFramework.ISEFileUIEvents

        /// <summary>
        /// Occurs before the user interface is displayed for a part created in place by Solid Edge Assembly.
        /// </summary>
        public void OnCreateInPlacePartUI(out string Filename, out string AppendToTitle, out string Template)
        {
            bool overrideDefaultBehavior = false;

            if (overrideDefaultBehavior)
            {
                Filename = null;
                AppendToTitle = null;
                Template = null;
            }
            else
            {
                // Visual Studio will stop on this exception in debug mode. Simply F5 through it when testing.
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Occurs before the user interface is created for a new Solid Edge file.
        /// </summary>
        public void OnFileNewUI(out string Filename, out string AppendToTitle)
        {
            bool overrideDefaultBehavior = false;

            if (overrideDefaultBehavior)
            {
                // If you get here, you override the default Solid Edge UI.
                Filename = null;
                AppendToTitle = null;
            }
            else
            {
                // Visual Studio will stop on this exception in debug mode. Simply F5 through it when testing.
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Occurs before the creation of the user interface for the file opened by Solid Edge.
        /// </summary>
        public void OnFileOpenUI(out string Filename, out string AppendToTitle)
        {
            bool overrideDefaultBehavior = false;

            if (overrideDefaultBehavior)
            {
                // If you get here, you override the default Solid Edge UI.
                Filename = null;
                AppendToTitle = null;
            }
            else
            {
                // Visual Studio will stop on this exception in debug mode. Simply F5 through it when testing.
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Occurs before the user interface is created for the file saved as image by Solid Edge.
        /// </summary>
        public void OnFileSaveAsImageUI(out string Filename, out string AppendToTitle, ref int Width, ref int Height, ref SolidEdgeFramework.SeImageQualityType ImageQuality)
        {
            bool overrideDefaultBehavior = false;

            if (overrideDefaultBehavior)
            {
                // If you get here, you override the default Solid Edge UI.
                Filename = null;
                AppendToTitle = null;
                Width = 100;
                Height = 100;
                ImageQuality = SolidEdgeFramework.SeImageQualityType.seImageQualityHigh;
            }
            else
            {
                // Visual Studio will stop on this exception in debug mode. Simply F5 through it when testing.
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Occurs before the user interface is created by the file saved by Solid Edge as another file.
        /// </summary>
        public void OnFileSaveAsUI(out string Filename, out string AppendToTitle)
        {
            bool overrideDefaultBehavior = false;

            if (overrideDefaultBehavior)
            {
                // If you get here, you override the default Solid Edge UI.
                Filename = null;
                AppendToTitle = null;
            }
            else
            {
                // Visual Studio will stop on this exception in debug mode. Simply F5 through it when testing.
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Occurs before the user interface is created for a part placed by Solid Edge Assembly.
        /// </summary>
        public void OnPlacePartUI(out string Filename, out string AppendToTitle)
        {
            bool overrideDefaultBehavior = false;

            if (overrideDefaultBehavior)
            {
                // If you get here, you override the default Solid Edge UI.
                Filename = null;
                AppendToTitle = null;
            }
            else
            {
                // Visual Studio will stop on this exception in debug mode. Simply F5 through it when testing.
                throw new NotImplementedException();
            }
        }

        #endregion

        #region SolidEdgeFramework.ISENewFileUIEvents

        public void OnNewFileUI(SolidEdgeFramework.DocumentTypeConstants DocumentType, out string Filename, out string AppendToTitle)
        {
            bool overrideDefaultBehavior = false;

            if (overrideDefaultBehavior)
            {
                // If you get here, you override the default Solid Edge UI.
                Filename = null;
                AppendToTitle = null;
            }
            else
            {
                // Visual Studio will stop on this exception in debug mode. Simply F5 through it when testing.
                throw new NotImplementedException();
            }
        }

        #endregion

        #region SolidEdgeFramework.ISEECEvents

        /// <summary>
        /// Fires File Open event for PDM workflow.
        /// </summary>
        public void PDM_OnFileOpenUI(out string bstrFilename)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Fires event before CPD Display.
        /// </summary>
        public void SEEC_BeforeCPDDisplay(object pCPDInitializer, SolidEdgeFramework.eCPDMode eCPDMode)
        {
        }

        /// <summary>
        /// Fires PreCPD event.
        /// </summary>
        public void SEEC_IsPreCPDEventSupported(out bool pvbPreCPDEventSupported)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region SolidEdgeFramework.ISEShortCutMenuEvents

        /// <summary>
        /// Notification of short-cut menu creation.
        /// </summary>
        public void BuildMenu(string EnvCatID, SolidEdgeFramework.ShortCutMenuContextConstants Context, object pGraphicDispatch, out Array MenuStrings, out Array CommandIDs)
        {
            MenuStrings = Array.CreateInstance(typeof(string), 0);
            CommandIDs = Array.CreateInstance(typeof(int), 0);

            switch (Context)
            {
                case SolidEdgeFramework.ShortCutMenuContextConstants.seShortCutForFeaturePathFinder:
                    break;
                case SolidEdgeFramework.ShortCutMenuContextConstants.seShortCutForFeaturePathFinderDocument:
                    break;
                case SolidEdgeFramework.ShortCutMenuContextConstants.seShortCutForGraphicLocate:
                    break;
                case SolidEdgeFramework.ShortCutMenuContextConstants.seShortCutForView:
                    break;
            }
        }

        #endregion

        #region Mouse Events
        async void ISEMouseEvents.MouseDown(short sButton, short sShift, double dX, double dY, double dZ, object pWindowDispatch, int lKeyPointType, object pGraphicDispatch)
        {
            //if (Keyboard.IsKeyDown(Key.LeftShift) && Keyboard.IsKeyDown(Key.S) && !_getting_suggestions)
            //{
                _getting_suggestions = true;

                var _application = SolidEdgeCommunity.SolidEdgeUtils.Connect();

                PartDocument _doc = _application.ActiveDocument as PartDocument;
                Model _model = _doc.Models.Item(1);
                Holes _holes = _model.Holes;

                

                List<HoleInfo> _holeInfos = new List<HoleInfo>();

                foreach (Hole hole in _holes)
                {
                    HoleInfo _holeInfo = default(HoleInfo);
                    SolidEdgePart.HoleData _holedata = hole.HoleData as SolidEdgePart.HoleData;
                    _holeInfo.diameter = 1000 * _holedata.HoleDiameter;
                    Profile profile = hole.Profile as Profile;
                    Holes2d holes2d = profile.Holes2d as Holes2d;
                    Hole2d hole2d = holes2d.Item(1);

                    double x_2d, y_2d, x_3d, y_3d, z_3d;
                    hole2d.GetCenterPoint(out x_2d, out y_2d);
                    profile.Convert2DCoordinate(x_2d, y_2d, out x_3d, out y_3d, out z_3d);

                    _holeInfo.x = x_3d;
                    _holeInfo.y = y_3d;
                    _holeInfo.z = z_3d;


                    RefPlane plane = profile.Plane as RefPlane;
                    Array normals = new double[3] as Array;
                    plane.GetNormal(ref normals);

                    double[] ns = normals as double[];
                    _holeInfo.nx = ns[0];
                    _holeInfo.ny = ns[1];
                    _holeInfo.nz = ns[2];

                    _holeInfos.Add(_holeInfo);
                    MessageBox.Show(string.Format("diam: {0:0.000} x: {1:0.000}, y: {2:0.000}, z: {3:0.000}, nx: {3:0.000}, ny: {3:0.000}, nz: {3:0.000}", _holeInfo.diameter,
                                                                                                                                                           _holeInfo.x,
                                                                                                                                                           _holeInfo.y,
                                                                                                                                                           _holeInfo.z,
                                                                                                                                                           _holeInfo.nx,
                                                                                                                                                           _holeInfo.ny,
                                                                                                                                                           _holeInfo.nz));
                }

                _holeInfos = _holeInfos.OrderBy(p => p.diameter).ToList();

                string query = "http://trapezohedron.shapespace.com:9985/v1/suggestions?query={\"status\": {\"v\": [";
                bool first = true;
                foreach (HoleInfo hi in _holeInfos)
                {
                    if (!first)
                    {
                        query += ", ";
                    }
                    first = false;
                    string add_v = String.Format("\"{0:0.0}\"", hi.diameter);
                    query += add_v;
                }
                query += "], \"e\": [";

                int v_source = 0;
                first = true;
                foreach (HoleInfo hi_source in _holeInfos)
                {
                    int v_dest = 0;
                    string bucket_dir_source = string.Format("{0:0.0000}{1:0.0000}{2:0.0000}", hi_source.nx, hi_source.ny, hi_source.nz);
                    foreach (HoleInfo hi_dest in _holeInfos)
                    {
                        if (v_dest > v_source)
                        {
                            if (!first)
                            {
                                query += ", ";
                            }
                            first = false;

                            double dist_bucket_size = 50;
                            string bucket_dir_dest = string.Format("{0:0.0000}{1:0.0000}{2:0.0000}", hi_dest.nx, hi_dest.ny, hi_dest.nz);
                            double e_dist = Math.Sqrt(Math.Pow(hi_source.x - hi_dest.x, 2) + Math.Pow(hi_source.y - hi_dest.y, 2) + Math.Pow(hi_source.z - hi_dest.z, 2));
                            double e_dist_bucket = Math.Ceiling(e_dist / dist_bucket_size);
                            string add_e = string.Format("[[\"{0:0.0}\", \"{1:0.0}\"], \"{2:0}\"]", hi_source.diameter, hi_dest.diameter, e_dist_bucket);
                            if (bucket_dir_source == bucket_dir_dest)
                            {
                                add_e += string.Format(",[[\"{0:0.0}\", \"{1:0.0}\"], \"co_dir\"]", hi_source.diameter, hi_dest.diameter);
                                //add_e += string.Format("[[\"{0:0.0}\", \"{1:0.0}\"], \"co_dir\"]", hi_source.diameter, hi_dest.diameter);
                            }
                            query += add_e;
                        }
                        v_dest += 1;
                    }
                    v_source += 1;
                }
                query += "]}, \"location\": [[[\"32.0\", \"*\"], \"co_dir\"]]}";

                int PointOnGraphicFlag;
                double PointOnGraphic_X;
                double PointOnGraphic_Y;
                double PointOnGraphic_Z;
                _mouse.PointOnGraphic(out PointOnGraphicFlag, out PointOnGraphic_X, out PointOnGraphic_Y, out PointOnGraphic_Z);

                //string query = "http://trapezohedron.shapespace.com:9985/v1/suggestions?query={\"status\": {\"v\": [\"32.0\", \"57.0\"], \"e\": [[[\"32.0\", \"57.0\"], \"co_dir\"]]}, \"location\": [[[\"32.0\", \"*\"], \"co_dir\"]]}";
                var values = new Dictionary<string, string> { };

                var content = new FormUrlEncodedContent(values);
                var response = await _client.GetAsync(query);
                var responseString = await response.Content.ReadAsStringAsync();
                MessageBox.Show(responseString);
                _getting_suggestions = false;
            //}
        }

        void ISEMouseEvents.MouseUp(short sButton, short sShift, double dX, double dY, double dZ, object pWindowDispatch, int lKeyPointType, object pGraphicDispatch)
        {
        }

        async void ISEMouseEvents.MouseMove(short sButton, short sShift, double dX, double dY, double dZ, object pWindowDispatch, int lKeyPointType, object pGraphicDispatch)
        {
            
        }

        void ISEMouseEvents.MouseClick(short sButton, short sShift, double dX, double dY, double dZ, object pWindowDispatch, int lKeyPointType, object pGraphicDispatch)
        {
        }

        void ISEMouseEvents.MouseDblClick(short sButton, short sShift, double dX, double dY, double dZ, object pWindowDispatch, int lKeyPointType, object pGraphicDispatch)
        {
        }

        void ISEMouseEvents.MouseDrag(short sButton, short sShift, double dX, double dY, double dZ, object pWindowDispatch, short DragState, int lKeyPointType, object pGraphicDispatch)
        {
        }

        #endregion

        #region RegAsm.exe callbacks

        /// <summary>
        /// Called when regasm.exe is executed against the assembly.
        /// </summary>
        [ComRegisterFunction]
        static void OnRegister(Type t)
        {
            // See http://www.codeproject.com/Articles/839585/Solid-Edge-ST-AddIn-Architecture-Overview#Registration for registration details.
            // The following code helps write registry entries that Solid Edge needs to identify an addin. You can omit this code and
            // user installer logic if you'd like. This is simply here to help.

            try
            {
                var settings = new SolidEdgeCommunity.AddIn.RegistrationSettings(t);

                settings.Enabled = true;
                settings.Environments.Add(SolidEdgeSDK.EnvironmentCategories.Application);
                settings.Environments.Add(SolidEdgeSDK.EnvironmentCategories.AllDocumentEnvrionments);

                // See http://msdn.microsoft.com/en-us/goglobal/bb964664.aspx for LCID details.
                var englishCulture = CultureInfo.GetCultureInfo(1033);

                // Title & Summary are Locale specific. 
                settings.Titles.Add(englishCulture, "SolidEdgeCommunity.Samples.DemoAddIn");
                settings.Summaries.Add(englishCulture, "Solid Edge Addin in .NET 4.0.");

                // Optionally, you can add additional locales.
                //var spanishCultere = CultureInfo.GetCultureInfo(3082);
                //settings.Titles.Add(spanishCultere, "SolidEdge.Samples.DemoAddIn");
                //settings.Summaries.Add(spanishCultere, "Solid Edge Addin in .NET 4.0.");

                //var germanCultere = CultureInfo.GetCultureInfo(1031);
                //settings.Titles.Add(germanCultere, "SolidEdge.Samples.DemoAddIn");
                //settings.Summaries.Add(germanCultere, "Solid Edge Addin in .NET 4.0.");

                DemoAddIn.Register(settings);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.StackTrace, ex.Message);
            }
        }

        /// <summary>
        /// Called when regasm.exe /u is executed against the assembly.
        /// </summary>
        [ComUnregisterFunction]
        static void OnUnregister(Type t)
        {
            DemoAddIn.Unregister(t);
        }
        #endregion

        private void ConnectMouse()
        {
            _cmd = (SolidEdgeFramework.Command)_application.CreateCommand((int)SolidEdgeConstants.seCmdFlag.seNoDeactivate);
            _mouse = (SolidEdgeFramework.Mouse)_cmd.Mouse;
            _cmd.Start();
            _mouse.EnabledMove = true;
            _mouse.LocateMode = (int)SolidEdgeConstants.seLocateModes.seSmartLocate;
            _mouse.ScaleMode = 1;   // Design model coordinates.
            _mouse.WindowTypes = 1; // Graphic window's only.
            _connectionPointController.AdviseSink<SolidEdgeFramework.ISEMouseEvents>(_mouse);
        }
    }
    
}
