using Accord.Imaging.Filters;
using Accord.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace testMQ
{
    public class DeviceInfomation
    {
        bool ready;
        public double angle;
        Rectangle rect;
        DateTime dt;
    }
    public partial class Form1 : Form
    {
        VideoCaptureDevice videoSource = null;
        System.Collections.Concurrent.ConcurrentQueue<Bitmap> new_frame_queue = new System.Collections.Concurrent.ConcurrentQueue<Bitmap>();
        System.Threading.AutoResetEvent quit = new System.Threading.AutoResetEvent(false);
        Bitmap current_image = null;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
            videoSource.VideoResolution = videoSource.VideoCapabilities[0];
            foreach (var vc in videoSource.VideoCapabilities)
            {
                if (vc.FrameSize.Width > videoSource.VideoResolution.FrameSize.Width && vc.FrameSize.Height > videoSource.VideoResolution.FrameSize.Height)
                    videoSource.VideoResolution = vc;
            }
            videoSource.NewFrame += VideoSource_NewFrame;
            videoSource.Start();
            // start image process thread
            System.Threading.Thread t = new System.Threading.Thread(this.processImageThread);
            t.IsBackground = true;
            t.Start();
        }

        private void VideoSource_NewFrame(object sender, Accord.Video.NewFrameEventArgs eventArgs)
        {
            Bitmap b = (Bitmap)eventArgs.Frame.Clone();
            new_frame_queue.Enqueue(b);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Program.logIt("Form1_FormClosing: ++");
            if (videoSource != null)
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();
            }
            Program.logIt("Form1_FormClosing: --");
        }

        void processImageThread(object obj)
        {
            Program.logIt("processImageThread: ++");
            DateTime dt = DateTime.Now;
            double angle = 0.0;
            Rectangle rect = Rectangle.Empty;
            while (!quit.WaitOne(10))
            {
                if (new_frame_queue.Count > 0)
                {
                    Bitmap cf = null;
                    while(new_frame_queue.TryDequeue(out cf))
                    {
                        if ((DateTime.Now - dt).TotalMilliseconds > 50)
                        {
                            dt = DateTime.Now;
                            //Tuple<bool, float, Bitmap> diff = ImUtility.diff_images((Bitmap)cf.Clone(), (Bitmap)current_image.Clone());
                            bool same_frame = (current_image == null) ? false : ImUtility.is_same_image((Bitmap)cf.Clone(), (Bitmap)current_image.Clone());
                            if (!same_frame)
                            {
                                if (angle == 0.0)
                                {
                                    Tuple<Bitmap, double, Rectangle> res = ImUtility.smate_rotate((Bitmap)cf.Clone());
                                    if(res.Item1!=null)
                                    {
                                        cf = res.Item1;
                                        angle = res.Item2;
                                        rect = res.Item3;
                                    }
                                }
                                else
                                {
                                    // turn and crop
                                    RotateBicubic filter = new RotateBicubic(angle);
                                    Bitmap src1 = filter.Apply(cf);
                                    Crop c_filter = new Crop(rect);
                                    cf = c_filter.Apply(src1);

                                }
                                pictureBox1.Invoke(new MethodInvoker(delegate
                                {
                                    current_image = cf;
                                    pictureBox1.Image = cf;
                                }));
                            }
                        }
                    }
                }
            }
            Program.logIt("processImageThread: --");
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif";
            saveFileDialog1.Title = "Save an Image File";
            saveFileDialog1.ShowDialog();
            if (saveFileDialog1.FileName != "")
            {
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
    }
}
