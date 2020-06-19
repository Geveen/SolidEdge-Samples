using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DemoAddIn
{
    public partial class Form1 : Form
    {
        private static double _Hole_dia = 0.0;

        public Form1()
        {
            InitializeComponent();

            //Ribbon3d ribbon3D = new Ribbon3d();
            double[] non_rep = Ribbon3d.New_match.Distinct().ToArray();
            //double[] non_rep = ribbon3D.new_match.Distinct().ToArray(); 

            foreach(double d in non_rep)
            {
                    Holedia_combo.Items.Add(d);   
            }
        }

        private void OK_button_Click(object sender, EventArgs e)
        {
            _Hole_dia = Convert.ToDouble(Holedia_combo.Text);;
            this.Close();
            
        }

        public double Hole_dia = _Hole_dia;

        private void Cancel_button_Click(object sender, EventArgs e)
        {
            _Hole_dia = 0.0;
            this.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _Hole_dia = 0.0;
        }

        private void Holedia_combo_SelectedIndexChanged(object sender, EventArgs e)
        {
          
        }
    }
}
