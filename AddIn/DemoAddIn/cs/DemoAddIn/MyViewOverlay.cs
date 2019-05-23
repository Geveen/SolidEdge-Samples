using System;
using System.Globalization;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SolidEdgePart;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace DemoAddIn
{
    class MyViewOverlay : SolidEdgeCommunity.AddIn.ViewOverlay
    {
        private BoundingBoxInfo _boundingBoxInfo = default(BoundingBoxInfo);
        private bool _showOpenGlBoxes = false;
        private bool _showGdiPlus = false;
        private bool _showHole = false;


        private SolidEdgeCommunity.ConnectionPointController _connectionPointController;
        private static readonly HttpClient _client = new HttpClient();
        private static bool _getting_suggestions = false;
        private static SolidEdgeFramework.Command _cmd = null;
        private static SolidEdgeFramework.Mouse _mouse = null;
        private static SolidEdgeFramework.Application _application = null;

        public MyViewOverlay()
        {
            // Set the defaults.
            _boundingBoxInfo.LineColor = Color.Yellow;
            _boundingBoxInfo.LineWidth = 2f;
        }

        public override void BeginOpenGLMainDisplay(SolidEdgeSDK.IGL gl)
        {
            if (gl == null) return;

            DrawBoundingBox(gl);

            if (_showOpenGlBoxes)
            {
                float fSize = 0.025f;
                double[] matrix0 = { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
                double[] matrix1 = { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, fSize, 1 };
                double[] matrix2 = { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, fSize, -fSize, 1 };
                double[] matrix3 = { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, fSize, -fSize, 0, 1 };

                gl.glMatrixMode(SharpGL.OpenGL.GL_MODELVIEW);

                int mode = 0;
                int depth = 0;
                uint error;

                error = gl.glGetError();

                gl.glGetIntegerv(SharpGL.OpenGL.GL_MATRIX_MODE, ref mode);
                gl.glGetIntegerv(SharpGL.OpenGL.GL_MODELVIEW_STACK_DEPTH, ref depth);
                gl.glPushMatrix();
                gl.glGetIntegerv(SharpGL.OpenGL.GL_MODELVIEW_STACK_DEPTH, ref depth);

                gl.glLoadMatrixd(matrix0);
                gl.glColor3f(1, 0, 0);

                DrawCube(gl, fSize / 2.0f);

                gl.glPopMatrix();
                gl.glPushMatrix();

                {
                    gl.glMultMatrixd(matrix1);
                    gl.glColor3f(0, 1, 0);
                    DrawCube(gl, fSize / 2.0f);
                }

                {
                    gl.glMultMatrixd(matrix2);
                    gl.glColor3f(0, 0, 1);
                    DrawCube(gl, fSize / 2.0f);
                }

                {
                    gl.glMultMatrixd(matrix3);
                    gl.glColor4f(1, 1, 0, .25f);
                    DrawCube(gl, fSize / 2.0f);
                }

                gl.glPopMatrix();
            }
        }

        public override void EndDeviceContextMainDisplay(IntPtr hDC, ref double modelToDC, ref int rect)
        {
            if (_showGdiPlus)
            {
                //Demonstrate using GDI+ to write text on the device context (DC).
                using (Graphics graphics = Graphics.FromHdc(hDC))
                {
                    Point point = new Point(0, 0);

                    using (Font font = SystemFonts.DialogFont)
                    {
                        Color color = Color.Yellow;
                        string lastUpdate = DateTime.Now.ToString();

                        lastUpdate = String.Format("Last update: {0}", lastUpdate);

                        TextRenderer.DrawText(graphics, lastUpdate, font, point, color, Color.Black);
                        Size size = TextRenderer.MeasureText(lastUpdate, font);

                        point.Offset(0, size.Height);
                    }

                    using (var pen = new Pen(Color.Red, 2))
                    {

                        var clipBounds = graphics.VisibleClipBounds;

                        //Draw a line
                        //graphics.DrawLine(pen, 10, 5, 110, 15);
                        graphics.DrawLine(pen, this.Window.Left, this.Window.Top, this.Window.Width, this.Window.Height);
                    }

                    //Draw an ellipse
                    graphics.DrawEllipse(Pens.Blue, 10, 20, 110, 45);

                    //Draw a rectangle
                    graphics.DrawRectangle(Pens.Green, 10, 70, 110, 45);

                    //Fill an ellipse
                    graphics.FillEllipse(Brushes.Blue, 130, 20, 110, 45);

                    //Fill a rectangle
                    graphics.FillRectangle(Brushes.Green, 130, 70, 110, 45);

                }
            }
           
            if (_showHole)
            {
               
                Holebutton_Click();

                async void Holebutton_Click()
                {

                   // MessageBox.Show("You Clicked the camera");

                    //if (Keyboard.IsKeyDown(Key.LeftShift) && Keyboard.IsKeyDown(Key.S) && !_getting_suggestions)


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

                                double dist_bucket_size = 50;
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

                    query += "]}, \"location\": [[[\"32.0\", \"*\"], \"co_dir\"], [[\"32.0\", \"*\"], \"4\"]]}";

                    /*int PointOnGraphicFlag;
                    double PointOnGraphic_X;
                    double PointOnGraphic_Y;
                    double PointOnGraphic_Z;
                    _mouse.PointOnGraphic(out PointOnGraphicFlag, out PointOnGraphic_X, out PointOnGraphic_Y, out PointOnGraphic_Z);
                    MessageBox.Show($"GraphicFlag={PointOnGraphicFlag}, Graphic_X {PointOnGraphic_X}," +
                    $" Graphic_Y={PointOnGraphic_Y}, Graphic_Z={PointOnGraphic_Y}");*/

                    _mouse.PointOnGraphic(out int PointOnGraphicFlag, out double PointOnGraphic_X, out double PointOnGraphic_Y, out double PointOnGraphic_Z);
                    MessageBox.Show($"{PointOnGraphicFlag}, {PointOnGraphic_X}, {PointOnGraphic_Y}, {PointOnGraphic_Z}");

                    //string query = "http://trapezohedron.shapespace.com:9985/v1/suggestions?query={\"status\": {\"v\": [\"32.0\", \"57.0\"], \"e\": [[[\"32.0\", \"57.0\"], \"co_dir\"]]}, \"location\": [[[\"32.0\", \"*\"], \"co_dir\"]]}";
                    var values = new Dictionary<string, string> { };

                    var content = new FormUrlEncodedContent(values);
                    var response = await _client.GetAsync(query);
                    var responseString = await response.Content.ReadAsStringAsync();
                    MessageBox.Show(responseString);
                    _getting_suggestions = false;


                }

                _showHole = false;
            }
            

        }

       
        
            

        





        private void DrawBoundingBox(SolidEdgeSDK.IGL gl)
        {
            if (_boundingBoxInfo.Visible == false) return;

            if (gl == null) return;

            Vector3d min = new Vector3d();
            Vector3d max = new Vector3d();

            this.View.GetModelRange(out min.X, out min.Y, out min.Z, out max.X, out max.Y, out max.Z);

            gl.glColor3i(_boundingBoxInfo.LineColor.R, _boundingBoxInfo.LineColor.G, _boundingBoxInfo.LineColor.B);
            gl.glLineWidth(_boundingBoxInfo.LineWidth);
            gl.glHint(SharpGL.OpenGL.GL_LINE_SMOOTH_HINT, SharpGL.OpenGL.GL_NICEST);

            {
                gl.glBegin(SharpGL.OpenGL.GL_LINE_LOOP);

                gl.glVertex3d(min.X, min.Y, max.Z);
                gl.glVertex3d(max.X, min.Y, max.Z);
                gl.glVertex3d(max.X, max.Y, max.Z);
                gl.glVertex3d(min.X, max.Y, max.Z);

                gl.glEnd();
            }

            {
                gl.glBegin(SharpGL.OpenGL.GL_LINE_LOOP);

                gl.glVertex3d(min.X, min.Y, min.Z);
                gl.glVertex3d(max.X, min.Y, min.Z);
                gl.glVertex3d(max.X, max.Y, min.Z);
                gl.glVertex3d(min.X, max.Y, min.Z);

                gl.glEnd();
            }

            {
                gl.glBegin(SharpGL.OpenGL.GL_LINES);

                gl.glVertex3d(min.X, min.Y, min.Z);
                gl.glVertex3d(min.X, min.Y, max.Z);

                gl.glVertex3d(max.X, max.Y, min.Z);
                gl.glVertex3d(max.X, max.Y, max.Z);

                gl.glVertex3d(min.X, max.Y, min.Z);
                gl.glVertex3d(min.X, max.Y, max.Z);

                gl.glVertex3d(max.X, min.Y, min.Z);
                gl.glVertex3d(max.X, min.Y, max.Z);

                gl.glEnd();
            }

            {
                gl.glColor3f(1, 0, 0);
                gl.glBegin(SharpGL.OpenGL.GL_LINES);

                // Diagonal line between min & max points.
                gl.glVertex3d(min.X, min.Y, min.Z);
                gl.glVertex3d(max.X, max.Y, max.Z);

                gl.glEnd();
            }
        }

        private void DrawCube(SolidEdgeSDK.IGL gl, float fSize)
        {
            float[][] p0 = new float[][] 
            {
                new float[] { 0.0f, 0.0f, 0.0f },
                new float[] { 0.0f, fSize, 0.0f },
                new float[] { fSize, 0.0f, 0.0f },
                new float[] { fSize, fSize, 0.0f }
            };

            float[][] p1 = new float[][] 
            {
                new float[] { 0.0f, 0.0f, fSize },
                new float[] { 0.0f, fSize, fSize },
                new float[] { fSize, 0.0f, fSize },
                new float[] { fSize, fSize, fSize }
            };

            float[][] p2 = new float[][] 
            {
                new float[] { 0.0f, 0.0f, 0.0f },
                new float[] { 0.0f, 0.0f, fSize },
                new float[] { 0.0f, fSize, 0.0f },
                new float[] { 0.0f, fSize, fSize }
            };

            float[][] p3 = new float[][] 
            {
                new float[] { fSize, 0.0f, 0.0f },
                new float[] { fSize, 0.0f, fSize },
                new float[] { fSize, fSize, 0.0f },
                new float[] { fSize, fSize, fSize }
            };

            float[][] p4 = new float[][] 
            {
                new float[] { 0.0f, 0.0f, 0.0f },
                new float[] { 0.0f, 0.0f, fSize },
                new float[] { fSize, 0.0f, 0.0f },
                new float[] { fSize, 0.0f, fSize }
            };

            float[][] p5 = new float[][] 
            {
                new float[] { 0.0f, fSize, 0.0f },
                new float[] { 0.0f, fSize, fSize },
                new float[] { fSize, fSize, 0.0f },
                new float[] { fSize, fSize, fSize }
            };

            // Normals
            float[][] n0 = new float[][] 
            {
                new float[] { 0.0f, 0.0f, -1.0f },
                new float[] { 0.0f, 0.0f, 1.0f},
                new float[] { 0.0f, -1.0f, 0.0f},
                new float[] { 0.0f, 1.0f, 0.0f},
                new float[] { 0.0f, 0.0f, 1.0f},
                new float[] { 0.0f, 1.0f, 0.0f},
                new float[] { 0.0f, -1.0f, 0.0f }
            };

            {
                gl.glBegin(SharpGL.OpenGL.GL_TRIANGLES);
                gl.glNormal3fv(n0[0]);

                gl.glEdgeFlag((byte)SharpGL.OpenGL.GL_TRUE);
                gl.glVertex3fv(p0[0]);

                gl.glEdgeFlag(0);
                gl.glVertex3fv(p0[1]);

                gl.glEdgeFlag(1);
                gl.glVertex3fv(p0[2]);

                gl.glNormal3fv(n0[1]);

                gl.glEdgeFlag(0);
                gl.glVertex3fv(p0[1]);

                gl.glEdgeFlag(1);
                gl.glVertex3fv(p0[2]);

                gl.glEdgeFlag(1);
                gl.glVertex3fv(p0[3]);

                gl.glEnd();
            }

            {
                gl.glBegin(SharpGL.OpenGL.GL_TRIANGLE_STRIP);

                gl.glNormal3fv(n0[2]);

                gl.glEdgeFlag(1);
                gl.glVertex3fv(p1[0]);

                gl.glEdgeFlag(1);
                gl.glVertex3fv(p1[1]);

                gl.glEdgeFlag(1);
                gl.glVertex3fv(p1[2]);

                gl.glEdgeFlag(1);
                gl.glVertex3fv(p1[3]);

                gl.glEnd();
            }

            {
                gl.glBegin(SharpGL.OpenGL.GL_TRIANGLE_STRIP);
                gl.glNormal3fv(n0[3]);

                gl.glEdgeFlag(1);
                gl.glVertex3fv(p2[0]);

                gl.glEdgeFlag(1);
                gl.glVertex3fv(p2[1]);

                gl.glEdgeFlag(1);
                gl.glVertex3fv(p2[2]);

                gl.glEdgeFlag(1);
                gl.glVertex3fv(p2[3]);

                gl.glEnd();
            }

            {
                gl.glBegin(SharpGL.OpenGL.GL_TRIANGLE_STRIP);
                gl.glNormal3fv(n0[4]);

                gl.glEdgeFlag(1);
                gl.glVertex3fv(p3[0]);

                gl.glEdgeFlag(1);
                gl.glVertex3fv(p3[1]);

                gl.glEdgeFlag(1);
                gl.glVertex3fv(p3[2]);

                gl.glEdgeFlag(1);
                gl.glVertex3fv(p3[3]);

                gl.glEnd();
            }

            {
                gl.glBegin(SharpGL.OpenGL.GL_TRIANGLE_STRIP);

                gl.glNormal3fv(n0[5]);

                gl.glEdgeFlag(1);
                gl.glVertex3fv(p4[0]);

                gl.glEdgeFlag(1);
                gl.glVertex3fv(p4[1]);

                gl.glEdgeFlag(1);
                gl.glVertex3fv(p4[2]);

                gl.glEdgeFlag(1);
                gl.glVertex3fv(p4[3]);

                gl.glEnd();
            }

            {
                gl.glBegin(SharpGL.OpenGL.GL_TRIANGLE_STRIP);
                gl.glNormal3fv(n0[6]);

                gl.glEdgeFlag(1);
                gl.glVertex3fv(p5[0]);

                gl.glEdgeFlag(1);
                gl.glVertex3fv(p5[1]);

                gl.glEdgeFlag(1);
                gl.glVertex3fv(p5[2]);

                gl.glEdgeFlag(1);
                gl.glVertex3fv(p5[3]);

                gl.glEnd();
            }
        }
      

        public bool ShowBoundingBox
        {
            get { return _boundingBoxInfo.Visible; }
            set
            {
                _boundingBoxInfo.Visible = value;

                // Force the view to update.
                this.View.Update();
            }
        }

        public bool ShowOpenGlBoxes
        {
            get { return _showOpenGlBoxes; }
            set
            {
                _showOpenGlBoxes = value;

                // Force the view to update.
                this.View.Update();
            }
        }

        public bool ShowGDIPlus
        {
            get { return _showGdiPlus; }
            set
            {
                _showGdiPlus = value;

                // Force the view to update.
                this.View.Update();
            }
        }

        public bool ShowOpenHole
        {
            get
            {
                return _showHole;
            }
            set
            {
                _showHole = value;

                //Force the view to update
                this.View.Update();
            }
            
        }

       
    }
}
