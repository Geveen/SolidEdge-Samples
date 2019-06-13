using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DemoAddIn
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public static double _Hole_dia =0.0;

        private void OK_button_Click(object sender, EventArgs e)
        {
            _Hole_dia = Convert.ToDouble(Holedia_combo.Text);
            Application.Exit();
            
        }

        public double Hole_dia = _Hole_dia;

        private void Cancel_button_Click(object sender, EventArgs e)
        {
            _Hole_dia = 0.0;
            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }
    }
}
