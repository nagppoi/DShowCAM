using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Accord.Video;
using Accord.Video.DirectShow;
using Accord.Video.FFMPEG;


namespace DShowCAM
{
    public partial class CameraForm : Form
    {
        private Size initSize;
        private FilterInfo filterInfo;
        private VideoCaptureDevice videoCaptureDevice = null;
        private string filename;
        private VideoFileWriter videoWriter;


        public CameraForm(FilterInfo filterInfo, string filename)
        {
            InitializeComponent();
            this.filterInfo = filterInfo;
            this.filename = filename;
        }

        private void CameraForm_Load(object sender, EventArgs e)
        {
            // 最初のフォームサイズを記憶する
            this.initSize = new Size(this.Width, this.Height);

            label1.Text = "Name: " + this.filterInfo.Name;
            label2.Text = "ID: " + this.filterInfo.MonikerString;

            int idx = 0;
            this.videoCaptureDevice = new VideoCaptureDevice(this.filterInfo.MonikerString);
            this.videoCaptureDevice.NewFrame += NewFrame;
            VideoCapabilities[] videoCapabilities = videoCaptureDevice.VideoCapabilities;
            foreach (var capabilty in videoCapabilities)
            {
                comboBox1.Items.Add(makeCapText(videoCapabilities, idx));
                idx++;
            }

            if (videoCapabilities.Length > 0)
            {
                comboBox1.SelectedIndex = 0;
                StartVideo(videoCapabilities[0]);
            }

            videoWriter = new VideoFileWriter();
            videoWriter.Open(this.filename, 640, 480, 30, VideoCodec.Default, 4000000);
        }

        private string makeCapText(VideoCapabilities[] caps, int idx)
        {
            if (idx < caps.Length)
            {
                var cap = caps[idx];
                return string.Format("{0} x {1}, {2} fps.", cap.FrameSize.Width, cap.FrameSize.Height, cap.MaximumFrameRate);
            }
            return "[Error]";
        }

        private void StartVideo(VideoCapabilities cap)
        {
            this.videoCaptureDevice.Stop();
            this.videoCaptureDevice.VideoResolution = cap;
            resize(cap.FrameSize.Width, cap.FrameSize.Height);
            this.pictureBox1.Width = cap.FrameSize.Width;
            this.pictureBox1.Height = cap.FrameSize.Height;
            this.videoCaptureDevice.Start();
        }

        private void NewFrame(object sender, NewFrameEventArgs ev)
        {
            var img = (Bitmap)ev.Frame.Clone();
            pictureBox1.Image = img;
            videoWriter.WriteVideoFrame((Bitmap)ev.Frame.Clone());
            System.GC.Collect();
        }

        private void CameraForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (videoCaptureDevice != null)
            {
                videoCaptureDevice.NewFrame -= NewFrame;
                videoCaptureDevice.Stop();
                videoCaptureDevice.WaitForStop();
                videoCaptureDevice = null;
                videoWriter.Close();
            }
        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            StartVideo(this.videoCaptureDevice.VideoCapabilities[comboBox1.SelectedIndex]);
        }




        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(SystemMetric smIndex);
        public enum SystemMetric : int
        {
            SM_CXDLGFRAME = 7,
            SM_CYDLGFRAME = 8,
            SM_CXFRAME = 32,
            SM_CYFRAME = 33,
            SM_CXFIXEDFRAME = SM_CXDLGFRAME,
            SM_CYFIXEDFRAME = SM_CYDLGFRAME,
            SM_CXSIZEFRAME = SM_CXFRAME,
            SM_CYSIZEFRAME = SM_CYFRAME,
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="w">画像サイズ W</param>
        /// <param name="h">画像サイズ H</param>
        private void resize(int w, int h)
        {
            // 必要とするサイズを計算する
            int right = pictureBox1.Left + w + GetSystemMetrics(SystemMetric.SM_CXSIZEFRAME) * 2 + 32;
            int bottom = pictureBox1.Top + h + GetSystemMetrics(SystemMetric.SM_CYSIZEFRAME)*2 + SystemInformation.CaptionHeight + 32;

            // 初期サイズを下回っている場合は修正
            if (right < this.initSize.Width)
                right = this.initSize.Width;
            if (bottom < this.initSize.Height)
                bottom = this.initSize.Height;

            this.Width = right;
            this.Height = bottom;
        }
    }
}
