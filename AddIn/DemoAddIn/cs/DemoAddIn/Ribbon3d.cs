using SolidEdgeCommunity.AddIn;
using SolidEdgeCommunity.Extensions; // https://github.com/SolidEdgeCommunity/SolidEdge.Community/wiki/Using-Extension-Methods
using SolidEdgePart;
using SolidEdgeFramework;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Windows.Forms;

namespace DemoAddIn
{
    class Ribbon3d : SolidEdgeCommunity.AddIn.Ribbon,
         SolidEdgeFramework.ISEMouseEvents // Solid Edge Mouse Events
    {
        const string _embeddedResourceName = "DemoAddIn.Ribbon3d.xml";
        private RibbonButton _buttonBoundingBox;
        private RibbonButton _buttonOpenGlBoxes;
        private RibbonButton _buttonGdiPlus;
        private RibbonButton _buttonHole;

        private SolidEdgeCommunity.ConnectionPointController _connectionPointController;
        private static readonly HttpClient _client = new HttpClient();
        private static bool _getting_suggestions = false;
        private static SolidEdgeFramework.Command _cmd = null;
        private static SolidEdgeFramework.Mouse _mouse = null;
        private static SolidEdgeFramework.Application _application = null;
        private static SolidEdgeGeometry.Plane _plane = null;


        public Ribbon3d()
        {
            // Get a reference to the current assembly. This is where the ribbon XML is embedded.
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();

            // In this example, XML file must have a build action of "Embedded Resource".
            this.LoadXml(assembly, _embeddedResourceName);

            // Example of how to bind a local variable to a particular ribbon control.
            _buttonBoundingBox = GetButton(20);
            _buttonOpenGlBoxes = GetButton(21);
            _buttonGdiPlus = GetButton(22);
            _buttonHole = GetButton(5);

            // Example of how to bind a particular ribbon control click event.
            _buttonBoundingBox.Click += _buttonBoundingBox_Click;
            _buttonOpenGlBoxes.Click += _buttonOpenGlBoxes_Click;
            _buttonGdiPlus.Click += _buttonGdiPlus_Click;
            _buttonHole.Click += _buttonHole_Click;

            // Get the Solid Edge version.
            var version = DemoAddIn.Instance.SolidEdgeVersion;
            _application = DemoAddIn.Instance.Application;
            // Create an instance of the default connection point controller. It helps manage connections to COM events.
            _connectionPointController = new SolidEdgeCommunity.ConnectionPointController(this);

            // View.GetModelRange() is only available in ST6 or greater.
            if (version.Major < 106)
            {
                _buttonBoundingBox.Enabled = false;
            }
        }

        public override void OnControlClick(RibbonControl control)
        {
            // Demonstrate how to handle commands without binding to a local variable.
            switch (control.CommandId)
            {
                case 0:
                    using (var dialog = new SaveFileDialog())
                    {
                        // The ShowDialog() extension method is exposed by:
                        // using SolidEdgeFramework.Extensions (SolidEdge.Community.dll)
                        if (_application.ShowDialog(dialog) == DialogResult.OK)
                        {

                        }
                    }
                    break;
                case 1:
                    using (var dialog = new FolderBrowserDialog())
                    {
                        // The ShowDialog() extension method is exposed by:
                        // using SolidEdgeFramework.Extensions (SolidEdge.Community.dll)
                        if (_application.ShowDialog(dialog) == DialogResult.OK)
                        {
                        }
                    }
                    break;
                case 2:
                    using (var dialog = new MyCustomDialog())
                    {
                        // The ShowDialog() extension method is exposed by:
                        // using SolidEdgeFramework.Extensions (SolidEdge.Community.dll)
                        if (_application.ShowDialog(dialog) == DialogResult.OK)
                        {
                        }
                    }
                    break;
                case 8:
                    _application.StartCommand(SolidEdgeConstants.PartCommandConstants.PartToolsOptions);
                    break;
                case 11:
                    _application.StartCommand(SolidEdgeConstants.PartCommandConstants.PartHelpSolidEdgeontheWeb);
                    break;
            }
        }

        void _buttonGdiPlus_Click(RibbonControl control)
        {
            var overlay = GetActiveOverlay();

            // Toggle the button check state.
            _buttonGdiPlus.Checked = !_buttonGdiPlus.Checked;
            overlay.ShowGDIPlus = _buttonGdiPlus.Checked;
        }

        void _buttonOpenGlBoxes_Click(RibbonControl control)
        {
            var overlay = GetActiveOverlay();

            // Toggle the button check state.
            _buttonOpenGlBoxes.Checked = !_buttonOpenGlBoxes.Checked;
            overlay.ShowOpenGlBoxes = _buttonOpenGlBoxes.Checked;
        }

        void _buttonBoundingBox_Click(RibbonControl control)
        {
            var overlay = GetActiveOverlay();

            // Toggle the button check state.
            _buttonBoundingBox.Checked = !_buttonBoundingBox.Checked;
            overlay.ShowBoundingBox = _buttonBoundingBox.Checked;
        }
        void _buttonHole_Click(RibbonControl control)
        {
            var overlay = GetActiveOverlay();

            _buttonHole.Checked = !_buttonHole.Checked;
           
            ConnectMouse();

            overlay.ShowOpenHole = _buttonHole.Checked;
        }
        private void ConnectMouse()
        {
            _cmd = (SolidEdgeFramework.Command)_application.CreateCommand((int)SolidEdgeConstants.seCmdFlag.seNoDeactivate);
            _mouse = (SolidEdgeFramework.Mouse)_cmd.Mouse;
            _cmd.Start();
            _mouse.EnabledMove = true;
            _mouse.LocateMode = (int)SolidEdgeConstants.seLocateModes.seLocateSimple;
            _mouse.ScaleMode = 1;   // Design model coordinates.
            _mouse.WindowTypes = 1; // Graphic window's only.
            _mouse.AddToLocateFilter(32);
            _connectionPointController.AdviseSink<SolidEdgeFramework.ISEMouseEvents>(_mouse);
            
        }


        private MyViewOverlay GetActiveOverlay()
        {
            var controlller = DemoAddIn.Instance.ViewOverlayController;
            var window = (SolidEdgeFramework.Window)DemoAddIn.Instance.Application.ActiveWindow;
            var overlay = (MyViewOverlay)controlller.GetOverlay(window);

            if (overlay == null)
            {
                // If the overlay has not been created yet, add a new one.
                overlay = controlller.Add<MyViewOverlay>(window);
            }

            return overlay;
        }

        async void ISEMouseEvents.MouseDown(short sButton, short sShift, double dX, double dY, double dZ, object pWindowDispatch, int lKeyPointType, object pGraphicDispatch)
        {
             
        }

        async void ISEMouseEvents.MouseUp(short sButton, short sShift, double dX, double dY, double dZ, object pWindowDispatch, int lKeyPointType, object pGraphicDispatch)
        {

        }

        void ISEMouseEvents.MouseMove(short sButton, short sShift, double dX, double dY, double dZ, object pWindowDispatch, int lKeyPointType, object pGraphicDispatch)
        {
            
        }

        async void ISEMouseEvents.MouseClick(short sButton, short sShift, double dX, double dY, double dZ, object pWindowDispatch, int lKeyPointType, object pGraphicDispatch)
        {
            //if (Keyboard.IsKeyDown(Key.LeftShift) && Keyboard.IsKeyDown(Key.S) && !_getting_suggestions)
            //MessageBox.Show($"dx{1000 * dX}, dy{1000 * dY}, dz{1000 * dZ}");
            _getting_suggestions = true;

             _application = SolidEdgeCommunity.SolidEdgeUtils.Connect();

            PartDocument _doc = _application.ActiveDocument as PartDocument;
            Model _model = _doc.Models.Item(1);
            Holes _holes = _model.Holes;

            
            
            var selected_face = pGraphicDispatch as SolidEdgeGeometry.Face;

            Array minparams = new double[2] as Array;
            Array maxparams = new double[2] as Array;
            selected_face.GetParamRange(ref minparams, ref maxparams);
            var mins = minparams as double[];
            var maxs = maxparams as double[];

            Array u = new double[2] { mins[0] + 0.5*(maxs[0]-mins[0]),
                                      mins[1] + 0.5*(maxs[1]-mins[1])};

            Array n = new double[3] as Array;
            selected_face.GetNormal(1, ref u, ref n);
            var norm = n as double[];
            int x = (int)Math.Round(norm[0]);
            int y = (int)Math.Round(norm[1]);
            int z = (int)Math.Round(norm[2]);

            string Face_normal_vector = string.Format("{0:0}{1:0}{2:0}", x, y, z); 

            
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

                _holeInfo.x = x_3d * 1000;
                _holeInfo.y = y_3d * 1000;
                _holeInfo.z = z_3d * 1000;


                RefPlane plane = profile.Plane as RefPlane;
                Array normals = new double[3] as Array;
                plane.GetNormal(ref normals);

                double[] ns = normals as double[];
                _holeInfo.nx = ns[0];
                _holeInfo.ny = ns[1];
                _holeInfo.nz = ns[2];

                _holeInfos.Add(_holeInfo);
                MessageBox.Show(string.Format("diam: {0:0.000} x: {1:0.000}, y: {2:0.000}, z: {3:0.000}, nx: {3:0.000}, ny: {3:0.000}, nz: {3:0.000}",
                                             _holeInfo.diameter, _holeInfo.x, _holeInfo.y, _holeInfo.z, _holeInfo.nx, _holeInfo.ny, _holeInfo.nz));


            }


            _holeInfos = _holeInfos.OrderBy(p => p.diameter).ToList();

            string query = "http://trapezohedron.shapespace.com:9985/v1/suggestions?query={\"status\": {\"v\": [";
            bool first = true;

            //adding the hole diameters to query
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


            double dist_bucket_size = 50;
            int v_source = 0;
            first = true;
            foreach (HoleInfo hi_source in _holeInfos)
            {
                int v_dest = 0;
                string bucket_dir_source = string.Format("{0:0.0000}{1:0.0000}{2:0.0000}", hi_source.nx, hi_source.ny, hi_source.nz);
                // MessageBox.Show($"Source {hi_source.x}, {hi_source.y}, {hi_source.z} --- {hi_source.nx}, {hi_source.ny}, {hi_source.nz} ");
                // MessageBox.Show($"{bucket_dir_source}");
                foreach (HoleInfo hi_dest in _holeInfos)
                {

                    if (v_dest > v_source)
                    {
                        //MessageBox.Show($"destination {hi_dest.x}, {hi_dest.y}, {hi_dest.z} --- {hi_dest.nx}, {hi_dest.ny}, {hi_dest.nz}");
                        if (!first)
                        {
                            query += ", ";
                        }
                        first = false;


                        string bucket_dir_dest = string.Format("{0:0.0000}{1:0.0000}{2:0.0000}", hi_dest.nx, hi_dest.ny, hi_dest.nz);
                        double e_dist = Math.Sqrt(Math.Pow(hi_source.x - hi_dest.x, 2) + Math.Pow(hi_source.y - hi_dest.y, 2) + Math.Pow(hi_source.z - hi_dest.z, 2));
                        //MessageBox.Show($"Bucket_dir_dest {bucket_dir_dest}, e_dist {e_dist}");
                        double e_dist_bucket = Math.Ceiling(e_dist / dist_bucket_size);
                        //MessageBox.Show($"e_dist_bucket {e_dist_bucket}");
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

           // query += "]}, \"location\":[[[\"32.0\", \"*\"], \"co_dir\"],";
            query += "]}, \"location\": [";

            //Accessing 3D mouse coordinates 
            _mouse.PointOnGraphic(out int PointOnGraphicFlag, out double PointOnGraphic_X, out double PointOnGraphic_Y, out double PointOnGraphic_Z);
            MessageBox.Show($"PointonGraphic {PointOnGraphicFlag}, {PointOnGraphic_X}, {PointOnGraphic_Y}, {PointOnGraphic_Z}");

            first = true;
            //Calculating distance from the mouse location to the hole center points 
            foreach (HoleInfo H_dest in _holeInfos)
            {
                if (!first)
                {
                    query += ", ";
                }
                first = false;

                double e_dest = Math.Sqrt(Math.Pow(H_dest.x - (1000 * PointOnGraphic_X), 2) + Math.Pow(H_dest.y - (1000 * PointOnGraphic_Y), 2) + Math.Pow(H_dest.z - (1000 * PointOnGraphic_Z), 2));
                double e_dist_bucket = Math.Ceiling(e_dest / dist_bucket_size);
                string add_e = string.Format("[[\"{0:0.0}\", \"*\"], \"{1:0}\"]", H_dest.diameter, e_dist_bucket);
                
                string Hole_Normal_vector = string.Format("{0:0}{1:0}{2:0}", H_dest.nx, H_dest.ny, H_dest.nz);
                if (Hole_Normal_vector == Face_normal_vector)
                {
                    add_e+= string.Format(", [[\"{0:0.0}\", \"*\"], \"co_dir\"]", H_dest.diameter);
                }

                query += add_e;
            }
            query += "]}";

            MessageBox.Show($"{query}");


            //string query = "http://trapezohedron.shapespace.com:9985/v1/suggestions?query={\"status\": {\"v\": [\"32.0\", \"57.0\"], \"e\": [[[\"32.0\", \"57.0\"], \"co_dir\"]]}, \"location\": [[[\"32.0\", \"*\"], \"co_dir\"]]}";
            var values = new Dictionary<string, string> { };

            var content = new FormUrlEncodedContent(values);
            var response = await _client.GetAsync(query);
            var responseString = await response.Content.ReadAsStringAsync();
            MessageBox.Show(responseString);

            _getting_suggestions = false;
            //}
        }

        void ISEMouseEvents.MouseDblClick(short sButton, short sShift, double dX, double dY, double dZ, object pWindowDispatch, int lKeyPointType, object pGraphicDispatch)
        {

        }

        void ISEMouseEvents.MouseDrag(short sButton, short sShift, double dX, double dY, double dZ, object pWindowDispatch, short DragState, int lKeyPointType, object pGraphicDispatch)
        {
            
        }
    }
}
