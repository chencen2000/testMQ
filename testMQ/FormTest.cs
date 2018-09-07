using Accord.Video;
using Accord.Video.DirectShow;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace testMQ
{
    public partial class FormTest : Form
    {
        VideoCaptureDevice videoSource = null;
        MJPEGStream stream = null;
        VideoCapture vc = null;
        public FormTest()
        {
            InitializeComponent();
        }

        private void FormTest_Load(object sender, EventArgs e)
        {
            
            //var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            //videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
            //videoSource.VideoResolution = videoSource.VideoCapabilities[0];
            //foreach (var vc in videoSource.VideoCapabilities)
            //{
            //    if (vc.FrameSize.Width > videoSource.VideoResolution.FrameSize.Width && vc.FrameSize.Height > videoSource.VideoResolution.FrameSize.Height)
            //        videoSource.VideoResolution = vc;
            //}
            ////videoSource.DesiredFrameSize = videoSource.VideoResolution.FrameSize;
            //videoSource.NewFrame += VideoSource_NewFrame;
            //videoSource.SnapshotFrame += VideoSource_SnapshotFrame;
            //videoSource.Start();
            
/*
            vc = new VideoCapture(1);
            vc.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, 1920);
            vc.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, 1080);
            var v = vc.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth);
            v = vc.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight);
            v = vc.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps);
            v = vc.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Format);
            v = vc.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FourCC);
            v = vc.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameCount);
            v = vc.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Guid);
            v = vc.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Gain);
            v = vc.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Gamma);
            v = vc.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Hue);
            v = vc.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Mode);
            v = vc.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Settings);
            v = vc.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Sharpness);
            v = vc.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Zoom);
            v = vc.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Focus);
            vc.ImageGrabbed += Vc_ImageGrabbed;
            vc.Start();
            */
            timer1.Enabled = true;
            timer1.Start();

        }

        private void VideoSource_SnapshotFrame(object sender, NewFrameEventArgs eventArgs)
        {
            //throw new NotImplementedException();
        }

        private void Vc_ImageGrabbed(object sender, EventArgs e)
        {
            Mat m = new Mat();
            vc.Retrieve(m);
            pictureBox1.Invoke(new MethodInvoker(delegate
            {
                pictureBox1.Image = m.Bitmap;
            }));

        }

        private void VideoSource_NewFrame(object sender, Accord.Video.NewFrameEventArgs eventArgs)
        {
            if (videoSource.IsRunning)
            {
                Bitmap b = (Bitmap)eventArgs.Frame.Clone();
                try
                {
                    pictureBox1.Invoke(new MethodInvoker(delegate
                    {
                        try
                        {
                            pictureBox1.Image = b;
                        }
                        catch (Exception) { }
                    }));
                }
                catch (Exception) { }
            }
        }

        private void FormTest_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (videoSource != null)
            {
                //videoSource.Stop();
                videoSource.SignalToStop();
                //videoSource.WaitForStop();
            }
            if (stream != null)
            {
                stream.Stop();
            }
            if (vc != null)
                vc.Stop();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            videoSource.DisplayPropertyPage(this.Handle);

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif";
            saveFileDialog1.Title = "Save an Image File";
            saveFileDialog1.ShowDialog();
            if (saveFileDialog1.FileName != "")
            {
                Image current_image = pictureBox1.Image;
                // Saves the Image via a FileStream created by the OpenFile method.  
                System.IO.FileStream fs =
                   (System.IO.FileStream)saveFileDialog1.OpenFile();
                // Saves the Image in the appropriate ImageFormat based upon the  
                // File type selected in the dialog box.  
                // NOTE that the FilterIndex property is one-based.  
                switch (saveFileDialog1.FilterIndex)
                {
                    case 1:
                        current_image.Save(fs,
                           System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;

                    case 2:
                        current_image.Save(fs,
                           System.Drawing.Imaging.ImageFormat.Bmp);
                        break;

                    case 3:
                        current_image.Save(fs,
                           System.Drawing.Imaging.ImageFormat.Gif);
                        break;
                }

                fs.Close();
            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //if(videoSource!=null)
            //{
            //    videoSource.SimulateTrigger();

            //}
            //if (vc != null)
            //{
            //    Mat m = vc.QueryFrame();
            //    pictureBox1.Invoke(new MethodInvoker(delegate
            //    {
            //        pictureBox1.Image = m.Bitmap;
            //    }));
            //}
            string fn = @"C:\projects\local\xindawn-airplay-sdk-example\screen.jpeg";
            DateTime l = DateTime.Now - new TimeSpan(0, 1, 0);
            Bitmap bmp = null;
            try
            {
                if (System.IO.File.Exists(fn))
                {
                    l = System.IO.File.GetLastWriteTime(fn);
                }
                System.Threading.EventWaitHandle evt = System.Threading.EventWaitHandle.OpenExisting(@"Global\captureimage");
                evt.Set();
                evt.Close();
                DateTime _s = DateTime.Now;
                do
                {
                    if (System.IO.File.Exists(fn))
                    {
                        DateTime _l = System.IO.File.GetLastWriteTime(fn);
                        if (_l > l)
                        {
                            try
                            {
                                using (var bmpTemp = new Bitmap(fn))
                                {
                                    bmp = new Bitmap(bmpTemp);
                                }
                                //bmp = new Bitmap(fn);                            
                                break;
                            }
                            catch (Exception) { }
                        }
                    }
                } while ((DateTime.Now - _s).TotalSeconds < 3);
                if (bmp != null)
                {
                    pictureBox1.Invoke(new MethodInvoker(delegate
                    {
                        try
                        {
                            pictureBox1.Image = bmp;
                        }
                        catch (Exception ex)
                        {
                            Program.logIt(ex.Message);
                            Program.logIt(ex.StackTrace);
                        }
                    }));
                }
            }
            catch(Exception ex)
            {
                Program.logIt(ex.Message);
                Program.logIt(ex.StackTrace);
            }
        }
    }
}
