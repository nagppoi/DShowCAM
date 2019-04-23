using Accord.Video;
using Accord.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DShowCAM
{
    public partial class Form1 : Form
    {
        private FilterInfoCollection videoDevices;
        private CameraForm[] cameraForms = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count != 0)
            {
                cameraForms = new CameraForm[videoDevices.Count];

                int i = 0;
                foreach (var device in videoDevices)
                {
                    Console.WriteLine("found: " + device.Name);
                    Console.WriteLine("MonikerString: " + device.MonikerString);
                    Console.WriteLine("------------------------------------");

                    CameraForm cam = new CameraForm(device);
                    cam.Show();
                    cameraForms[i] = cam;
                    i++;
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (var cam in cameraForms)
            {
                cam.Dispose();
            }
        }
    }
}
