using Accord.Imaging.Filters;
using Accord.Video.DirectShow;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace testMQ
{
    //public class DeviceInfomation
    //{
    //    bool ready;
    //    public double angle;
    //    Rectangle rect;
    //    DateTime dt;
    //}
    public partial class Form1 : Form
    {
        //VideoCaptureDevice videoSource = null;
        System.Collections.Concurrent.ConcurrentQueue<Bitmap> new_frame_queue = new System.Collections.Concurrent.ConcurrentQueue<Bitmap>();
        System.Threading.AutoResetEvent quit = new System.Threading.AutoResetEvent(false);
        Bitmap current_image = null;
        Bitmap current_raw_image = null;
        Bitmap homescreen = null;
        DateTime last_frame_time = DateTime.Now - new TimeSpan(1, 0, 0);
        //double frame_angle = 0.0;
        bool busy = false;
        bool recording = false;
        Queue<Dictionary<string, object>> recording_actions = null;
        string busy_text = string.Empty;
        double raw_to_show = 0.75;
        Rectangle frame_roi = Rectangle.Empty;
        System.Collections.Generic.Dictionary<string, object> home_screen_info = new Dictionary<string, object>();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            //videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
            //videoSource.VideoResolution = videoSource.VideoCapabilities[0];
            //foreach (var vc in videoSource.VideoCapabilities)
            //{
            //    if (vc.FrameSize.Width > videoSource.VideoResolution.FrameSize.Width && vc.FrameSize.Height > videoSource.VideoResolution.FrameSize.Height)
            //        videoSource.VideoResolution = vc;
            //}
            //videoSource.NewFrame += VideoSource_NewFrame;
            //videoSource.Start();
            //// start image process thread
            //System.Threading.Thread t = new System.Threading.Thread(this.processImageThread);
            //t.IsBackground = true;
            //t.Start();
            timer1.Tick += timer1_Tick;
            timer1.Enabled = true;
            timer1.Start();
            // COM4
            string s = Program.args.ContainsKey("port") ? Program.args["port"] : "COM3";
            KeyInput.getInstance().setSerialPort(s);
            KeyInput.getInstance().start();
            //
            if (Program.args.ContainsKey("getimei"))
            {
                this.Visible = false;
                this.Opacity = 0;
                this.ShowInTaskbar = false;
                Task.Run(() =>
                {
                    run_getimei();
                });
            }
            else if (Program.args.ContainsKey("playback"))
            {
                this.Visible = false;
                this.Opacity = 0;
                this.ShowInTaskbar = false;
                Task.Run(() =>
                {
                    run_playback();
                });
            }
            
        }
        private void run_getimei()
        {
            while (current_raw_image == null)
                System.Threading.Thread.Sleep(1000);

            string s = read_imei();
            System.Console.WriteLine(string.Format("imei={0}", s));
            //System.Console.ReadKey();
            this.Invoke(new MethodInvoker(delegate
            {
                this.Close();
            }));
        }
        private void run_playback()
        {
            string pb = Program.args["playback"];
            if (System.IO.File.Exists(pb))
            {
                while (current_raw_image == null)
                    System.Threading.Thread.Sleep(1000);
                try
                {
                    run_script(pb);
                    Program.exit_code = 0;
                }
                catch (Exception ex)
                {
                    Program.logIt(ex.Message);
                    Program.logIt(ex.StackTrace);
                    Program.exit_code = 1;
                }
            }
            else
            {
                Program.exit_code = 2;
            }
            //System.Threading.Thread.Sleep(3000);
            this.Invoke(new MethodInvoker(delegate
            {
                this.Close();
            }));
        }

        private void VideoSource_NewFrame(object sender, Accord.Video.NewFrameEventArgs eventArgs)
        {
            if ((DateTime.Now - last_frame_time).TotalMilliseconds > 500)
            {
                // one frame per 500ms
                last_frame_time = DateTime.Now;
                Bitmap b = (Bitmap)eventArgs.Frame.Clone();
                new_frame_queue.Enqueue(b);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Program.logIt("Form1_FormClosing: ++");
            KeyInput.getInstance().close();
            //if (videoSource != null)
            //{
            //    videoSource.SignalToStop();
            //    videoSource.WaitForStop();
            //}
            Program.logIt("Form1_FormClosing: --");
        }

        void processImageThread(object obj)
        {
        }
        void saveCurrentImage()
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
                        current_raw_image.Save(fs,
                           System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;

                    case 2:
                        current_raw_image.Save(fs,
                           System.Drawing.Imaging.ImageFormat.Bmp);
                        break;

                    case 3:
                        current_raw_image.Save(fs,
                           System.Drawing.Imaging.ImageFormat.Gif);
                        break;
                }

                fs.Close();
            }
        }
        private async void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            //saveCurrentImage();
            Program.logIt(string.Format("mouse click: {0}, {1}", e.Button, e.Location));
            if (!busy)
            {
                busy = true;
                double x = 1.0 * e.Location.X / raw_to_show;
                double y = 1.0 * e.Location.Y / raw_to_show;
                busy_text = String.Format("Click at ({0},{1}).\n Please wait.", (int)x, (int)y);
                if (e.Button == MouseButtons.Left)
                {
                    await Task.Run(() =>
                    {
                        tapXY(new Point((int)x, (int)y), 0x52);
                    });
                }
                else if (e.Button == MouseButtons.Right)
                {
                    await Task.Run(() =>
                    {
                        tapXY(new Point((int)x, (int)y));
                    });
                }
                busy = false;
            }
        }

        void tapXY(Point XY, byte key=0, Bitmap checkpoint=null)
        {
            Program.logIt(string.Format("tapXY: ++ {0}", XY));
            Tuple<Bitmap, Rectangle> page = get_first_item_rectangle();
            Point center_of_first_item = new Point(page.Item2.X + page.Item2.Width / 2, page.Item2.Y + page.Item2.Height / 2);
            bool done = false;
            Bitmap b;
            // for recording:
            System.Collections.Generic.Dictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add("action", "tapxy");
            dict.Add("x", XY.X);
            dict.Add("y", XY.Y);
            dict.Add("key", key);
            dict.Add("timestamp", DateTime.Now.Ticks);
            bool skip = false;
            // check first item
            if (page.Item2.Contains(XY))
            {
                if (key != 0)
                {
                    //using (MemoryStream ms = new MemoryStream())
                    //{
                    //    current_raw_image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    //    dict.Add("check", ms.ToArray());
                    //}       
                    dict.Add("image", current_raw_image);
                    if (checkpoint != null)
                    {
                        if (!ImUtility.is_same_image(checkpoint, current_raw_image))
                        {
                            Program.logIt("check image Not same.");
                            skip = true;
                        }
                    }
                    // found;
                    if(!skip)
                        KeyInput.getInstance().sendKey(key);
                }
            }
            else
            {
                while (!done && page.Item1 != null)
                {
                    //page.Item1.Save("temp_1.jpg");
                    b = new Bitmap(current_raw_image);
                    KeyInput.getInstance().sendKey(0x4f);
                    waitForScreenChange(b);
                    b = new Bitmap(current_raw_image);
                    Rectangle br = ImUtility.detect_blue_rectangle(page.Item1, b);
                    br.Inflate((int)(0 - 0.15 * br.Width), (int)(0 - 0.15 * br.Height));
                    if (br.Contains(XY))
                    {
                        // found;
                        if (key != 0)
                        {
                            //using (MemoryStream ms = new MemoryStream())
                            //{
                            //    current_raw_image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                            //    dict.Add("check", ms.ToArray());
                            //}
                            dict.Add("image", current_raw_image);
                            if (checkpoint != null)
                            {
                                // remove fringe 30 pixel
                                Bitmap b1 = ImUtility.crop_image(checkpoint, new Rectangle(0, 30, checkpoint.Width, checkpoint.Height - 30));
                                Bitmap b2 = ImUtility.crop_image(current_raw_image, new Rectangle(0, 30, current_raw_image.Width, current_raw_image.Height - 30));
                                // just compare the blue rectangle
                                //Bitmap b1 = ImUtility.crop_image(checkpoint, br);
                                //Bitmap b2 = ImUtility.crop_image(current_raw_image, br);
                                //if (!ImUtility.is_same_image(checkpoint, current_raw_image))
                                if (!ImUtility.is_same_image(b1, b2))
                                {
                                    current_raw_image.Save("temp_1.jpg");
                                    Program.logIt("check image Not same.");
                                    skip = true;
                                }
                            }
                            if(!skip)
                                KeyInput.getInstance().sendKey(key);
                        }
                        break;
                    }
                    if (br.Contains(center_of_first_item))
                    {
                        // loop back
                        Program.logIt("look back to the first item");
                        break;
                    }
                }
            }
            if (recording_actions != null)
            {
                recording_actions.Enqueue(dict);
            }
            Program.logIt(string.Format("tapXY: --"));
        }
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // save current image to file
            saveCurrentImage();
        }

        private void moveNextItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (recording_actions != null)
            {
                System.Collections.Generic.Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("action", "key");
                dict.Add("char", 0x4f);
                dict.Add("timestamp", DateTime.Now.Ticks);
                recording_actions.Enqueue(dict);
            }
            KeyInput.getInstance().sendKey(0x4f);
        }

        private void movePreviusItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (recording_actions != null)
            {
                System.Collections.Generic.Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("action", "key");
                dict.Add("char", 0x50);
                dict.Add("timestamp", DateTime.Now.Ticks);
                recording_actions.Enqueue(dict);
            }
            KeyInput.getInstance().sendKey(0x50);
        }

        private void selectItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (recording_actions != null)
            {
                System.Collections.Generic.Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("action", "key");
                dict.Add("char", 0x51);
                dict.Add("timestamp", DateTime.Now.Ticks);
                recording_actions.Enqueue(dict);
            }
            KeyInput.getInstance().sendKey(0x51);
        }

        private void tapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (recording_actions != null)
            {
                System.Collections.Generic.Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("action", "key");
                dict.Add("char", 0x52);
                dict.Add("timestamp", DateTime.Now.Ticks);
                recording_actions.Enqueue(dict);
            }
            KeyInput.getInstance().sendKey(0x52);
        }

        private void goToHomeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (recording_actions != null)
            {
                System.Collections.Generic.Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("action", "gotohome");
                dict.Add("timestamp", DateTime.Now.Ticks);
                recording_actions.Enqueue(dict);
            }
            KeyInput.getInstance().sendKey(0x4a);
        }

        private void appSwitchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (recording_actions != null)
            {
                System.Collections.Generic.Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("action", "key");
                dict.Add("char", 0x4d);
                dict.Add("timestamp", DateTime.Now.Ticks);
                recording_actions.Enqueue(dict);
            }
            KeyInput.getInstance().sendKey(0x4d);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        void calibration_thread()
        {
            click_home_v2();
            Bitmap b1 = new Bitmap(current_raw_image);
            Tuple<Bitmap, Rectangle> res = get_first_item_rectangle();
            Bitmap b0 = res.Item1;
            Rectangle firstR = res.Item2;
            Bitmap b2 = new Bitmap(current_raw_image);
            if (ImUtility.is_same_image(b1, b2))
            {
                //Rectangle br = ImUtility.detect_blue_rectangle(b0, b1);
                homescreen = b0;
            }
            // 
            go_to_top_of_setting();
            close_all_apps_v2();
        }
        private void calibrationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //frame_angle = 0.0;
            //KeyInput.getInstance().sendKey(0x4a);
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                calibration_thread();
            });
            MessageBox.Show(this, "Will capture the home screen and close all apps.", "Calibration");
        }

        void close_all_apps()
        {
            click_home();
            Bitmap homescreen = (Bitmap)current_image.Clone();
            // click app switch
            KeyInput.getInstance().sendKey(0x4d);
            bool done = false;
            // wait for change
            while (!done)
            {
                System.Threading.Thread.Sleep(500);
                Bitmap bmp = (Bitmap)current_image.Clone();
                double psnr = ImUtility.get_PSNR(homescreen, bmp);
                if (psnr > 25)
                {
                    // same image
                    //done = true;
                }
                else
                {
                    done = true;
                }
            }
            // app swtich
            Rectangle r = new Rectangle(0, 0, homescreen.Width, homescreen.Height * 6 / 10);
            done = false;
            while (!done)
            {
                bool found = false;
                System.Threading.Thread.Sleep(2000);
                Bitmap b1 = (Bitmap)current_image.Clone();
                // click app switch
                KeyInput.getInstance().sendKey(0x51);
                // wait for image chage
                Program.logIt("click select");
                Bitmap b2 = (Bitmap)current_image.Clone();
                do
                {
                    System.Threading.Thread.Sleep(500);
                    b2 = (Bitmap)current_image.Clone();
                } while (ImUtility.is_same_image(b2, b1));
                Program.logIt("image ready after click select");
                //System.Threading.Thread.Sleep(1000);
                //b1.Save("temp_1.jpg");
                //b2.Save("temp_2.jpg");
                Tuple<bool, Rectangle, Bitmap> cm = ImUtility.extrac_context_menu(b1, b2);
                //Program.logIt(string.Format("{0}: {1}", cm.Item1, cm.Item2));
                if (cm.Item1)
                {
                    Program.logIt(string.Format("{0} vs {1}", cm.Item2, r));
                    if (r.Contains(cm.Item2))
                    {
                        found = false;
                        KeyInput.getInstance().sendKey(0x50);
                        KeyInput.getInstance().sendKey(0x51);
                    }
                    else
                    {
                        found = true;
                        KeyInput.getInstance().sendKey(0x4f);
                        KeyInput.getInstance().sendKey(0x52);
                        System.Threading.Thread.Sleep(2000);
                    }
                }
                if (!found)
                    done = true;
            }
        }
        void test_icon_location()
        {
            Bitmap b = (Bitmap)current_image.Clone();
            using (Graphics g = Graphics.FromImage(b))
            {
                foreach (KeyValuePair<string, object> kv in home_screen_info)
                {
                    System.Collections.Generic.Dictionary<string, object> page = (System.Collections.Generic.Dictionary<string, object>)kv.Value;
                    foreach (KeyValuePair<string, object> pkv in page)
                    {
                        System.Collections.Generic.Dictionary<string, object> info = (System.Collections.Generic.Dictionary<string, object>)pkv.Value;
                        if (info.ContainsKey("rectangle") && info["rectangle"].GetType() == typeof(Rectangle))
                        {
                            Rectangle r = (Rectangle)info["rectangle"];
                            if (!r.IsEmpty)
                                g.DrawRectangle(new Pen(Color.Red), r);
                        }
                    }
                }
            }
            b.Save("temp_1.jpg");
        }
        int waitForScreenChange(Bitmap src, int threshold = 30, int timeout = 5)
        {
            Program.logIt("waitForScreenChange: ++");
            // ret=0, screen changed,
            // ret=1, timeout
            int ret = 1;
            bool done = false;
            DateTime _s = DateTime.Now;
            // wait for change
            while (!done && (DateTime.Now - _s).TotalSeconds < 5)
            {
                System.Threading.Thread.Sleep(200);
                Bitmap b1 = new Bitmap(current_raw_image);
                if (!ImUtility.is_same_image(b1, src, threshold))
                {
                    ret = 0;
                    done = true;
                }
            }
            Program.logIt(string.Format("waitForScreenChange: -- ret={0}", ret));
            return ret;
        }
        bool waitForScreenStable(int interval = 200, int framecount = 5, int threshold = 50, int timeout = 5)
        {
            Program.logIt("waitForScreenStable: ++");
            bool ret = false;
            Bitmap[] bmps = new Bitmap[framecount];
            DateTime _start = DateTime.Now;
            int cnt = 0;
            while (!ret && (DateTime.Now - _start).TotalSeconds < timeout)
            {
                System.Threading.Thread.Sleep(interval);
                bmps[cnt++ % framecount] = new Bitmap(current_raw_image);
                //cnt++;
                if (cnt > framecount)
                {
                    if (ImUtility.is_all_image_same(bmps, threshold))
                        ret = true;
                    else
                        Program.logIt("not all same.");
                }
            }
            Program.logIt(string.Format("waitForScreenStable: -- ret={0}", ret));
            return ret;
        }
        Bitmap get_homescreen_without_bluebox()
        {
            Bitmap ret = null;
            Bitmap b1 = new Bitmap(current_raw_image);
            KeyInput.getInstance().sendKey(0x48);
            waitForScreenChange(b1);
            ret = new Bitmap(current_raw_image);
            ret.Save("temp_hs.jpg");
            return ret;
        }
        Tuple<Bitmap, Rectangle> get_first_item_rectangle()
        {
            Program.logIt("get_first_item_rectangle: ++");
            Bitmap retB = null;
            Rectangle retR = Rectangle.Empty;
            Bitmap b1 = new Bitmap(current_raw_image);
            KeyInput.getInstance().sendKey(0x48);
            System.Threading.Thread.Sleep(500);
            waitForScreenChange(b1, 30);
            retB = new Bitmap(current_raw_image);
            KeyInput.getInstance().sendKey(0x4f);
            System.Threading.Thread.Sleep(200);
            waitForScreenChange(retB, 30);
            b1 = new Bitmap(current_raw_image);
            retR = ImUtility.detect_blue_rectangle(retB, b1);
            Program.logIt(string.Format("get_first_item_rectangle: -- rect={0}", retR));
            return new Tuple<Bitmap, Rectangle>(retB, retR);
        }
        void close_all_apps_v2()
        {
            Program.logIt("close_all_apps_v2: ++");
            click_home_v2();
            Bitmap homescreen = new Bitmap(current_raw_image);
            // click app switch
            Program.logIt("click app switch");
            KeyInput.getInstance().sendKey(0x4d);
            waitForScreenStable(250, 4, 30, 10);

            bool done = false;
            while (!done)
            {
                // wait for menu
                Bitmap app = new Bitmap(current_raw_image);
                //waitForScreenChange(homescreen);
                if (ImUtility.is_same_image(homescreen, app, 30))
                {
                    Program.logIt("found homescreen.");
                    break;
                }

                //app.Save("temp_1.jpg");
                // click select
                //app.Save("temp_1.jpg");
                Program.logIt("click select 0x51");
                KeyInput.getInstance().sendKey(0x51);
                waitForScreenChange(app);
                //
                System.Threading.Thread.Sleep(1000);
                Bitmap app1 = new Bitmap(current_raw_image);
                //app1.Save("temp_2.jpg");
                Tuple<bool, Rectangle, Bitmap> menu = ImUtility.extrac_context_menu(app, app1);
                if (menu.Item1 && menu.Item3 != null)
                {
                    Bitmap t = new Bitmap(@"images\ios_close_icon.jpg");
                    Tuple<double, Point, Rectangle> r = ImUtility.multiscale_matchtemplate(menu.Item3, t);
                    Program.logIt(string.Format("look for close icon: {0}: {1}", r.Item1, new Rectangle(r.Item2, t.Size)));
                    if (r.Item1 > 0.95)
                    {
                        // found.
                        KeyInput.getInstance().sendKey(0x4f);
                        System.Threading.Thread.Sleep(500);
                        KeyInput.getInstance().sendKey(0x52);
                        System.Threading.Thread.Sleep(1000);
                        //app1 = new Bitmap(current_raw_image);
                        waitForScreenStable();
                    }
                    else
                    {
                        done = true;
                    }

                    //menu.Item3.Save("temp_3.jpg");
                    //am = new Image<Gray, byte>(menu.Item3);
                    /*
                    am = new Image<Gray, byte>(app1);
                    // 
                    Mat t = CvInvoke.Imread(@"images\ios_close_icon.jpg", ImreadModes.Grayscale);
                    Mat m = new Mat();
                    CvInvoke.MatchTemplate(am, t, m, TemplateMatchingType.CcoeffNormed);
                    double max = 0;
                    double min = 0;
                    Point maxP = new Point();
                    Point minP = new Point();
                    CvInvoke.MinMaxLoc(m, ref min, ref max, ref minP, ref maxP);
                    Program.logIt(string.Format("{0}: {1}", max, new Rectangle(maxP, t.Size)));
                    if (max > 0.95)
                    {
                        // found.
                        KeyInput.getInstance().sendKey(0x4f);
                        System.Threading.Thread.Sleep(500);
                        KeyInput.getInstance().sendKey(0x52);
                        System.Threading.Thread.Sleep(1000);
                        //app1 = new Bitmap(current_raw_image);
                        waitForScreenStable();
                    }
                    else
                    {
                        done = true;
                    }
                    */
                }
                else
                    done = true;
            }
            Program.logIt("close_all_apps_v2: --");
        }
        void scroll_screen(string way="right")
        {
            Program.logIt(string.Format("Scroll screen: ++ {0}", way));
            // popup context menu
            Bitmap b1 = new Bitmap(current_raw_image);
            KeyInput.getInstance().sendKey(0x51);
            waitForScreenChange(b1);
            waitForScreenStable();
            Bitmap b2 = new Bitmap(current_raw_image);
            Rectangle rmenu = ImUtility.detect_blue_rectangle(b1, b2);
            Bitmap menu = ImUtility.crop_image(b2, rmenu);
            if (menu != null)
            {
                //menu.Save("temp_1.jpg");
                Tuple<double, Point, Rectangle> res = ImUtility.multiscale_matchtemplate(menu, new Bitmap(string.Format(@"images\scroll_{0}_icon.jpg", way)));
                if (res.Item1 > 0.95)
                {
                    Point targetP = new Point(res.Item3.X + res.Item3.Width / 2, res.Item3.Y + res.Item3.Height / 2);
                    Program.logIt(string.Format("found scroll {0} icon. location={1}, similarity={2}", way, targetP, res.Item1));
                    // move cursor to the location and click
                    Point firstP = Point.Empty;
                    bool done = false;
                    do
                    {
                        Tuple<bool, Rectangle, Bitmap> mi = ImUtility.find_focused_menu(menu);
                        if(mi.Item1 && !mi.Item2.IsEmpty)
                        {
                            Program.logIt(string.Format("focus item: {0}", mi.Item2));
                            if (firstP.IsEmpty)
                            {
                                firstP = new Point(mi.Item2.X + mi.Item2.Width / 2, mi.Item2.Y + mi.Item2.Height / 2);
                            }
                            else
                            {
                                if (mi.Item2.Contains(firstP))
                                {
                                    // on first item point
                                    Program.logIt(string.Format("found first item: {0}", mi.Item2));
                                    done = true;
                                }
                            }
                            if (mi.Item2.Contains(targetP))
                            {
                                // on target point;
                                Program.logIt(string.Format("found target item: {0}", mi.Item2));
                                b1 = new Bitmap(current_raw_image);
                                KeyInput.getInstance().sendKey(0x52);
                                waitForScreenChange(b1);
                                done = true;
                                waitForScreenStable();
                            }
                            else
                            {
                                // move next
                                b1 = new Bitmap(current_raw_image);
                                KeyInput.getInstance().sendKey(0x4f);
                                waitForScreenChange(b1);
                                b2 = new Bitmap(current_raw_image);
                                menu = ImUtility.crop_image(b2, rmenu);
                            }
                        }
                        else
                        {
                            // fail to find focus menu item.
                            Program.logIt("fail to find focus the menu item.");
                            break;
                        }
                    } while (!done);
                }
                else
                {
                    Program.logIt(string.Format("fail to find scroll {0} icon. similarity={1}", way, res.Item1));
                }

                // esc context menu
                b1 = new Bitmap(current_raw_image);
                KeyInput.getInstance().sendKey(0x48);
                waitForScreenChange(b1);
                b1 = new Bitmap(current_raw_image);
                KeyInput.getInstance().sendKey(0x48);
                waitForScreenChange(b1);
                KeyInput.getInstance().sendKey(0x50);
                KeyInput.getInstance().sendKey(0x51);
            }
            else
            {
                Program.logIt("fail to popup context menu");
            }
            Program.logIt(string.Format("Scroll screen: --"));
        }
        void scroll_screen_v2(string way = "right")
        {
            //click_home_v2();
            Bitmap b1 = new Bitmap(current_raw_image);
            Bitmap org_screen = new Bitmap(b1);
            KeyInput.getInstance().sendKey(0x51);
            waitForScreenChange(b1);
            Bitmap b2 = new Bitmap(current_raw_image);
            Tuple<bool, Rectangle, Bitmap> menu = ImUtility.extrac_context_menu(b1, b2);
            Bitmap m1 = null;
            Rectangle rmenu = Rectangle.Empty;
            if (menu.Item1 && menu.Item3 != null)
            {
                m1 = new Bitmap(menu.Item3);
                rmenu = menu.Item2;
                m1.Save("temp_1.jpg");
            }
            if (m1 != null)
            {
                Tuple<double, Point, Rectangle> res = ImUtility.multiscale_matchtemplate(m1, new Bitmap(string.Format(@"images\scroll_icon.jpg")));
                Program.logIt(string.Format("Try to find scroll_icon: similarity={0}, rec={1}", res.Item1, res.Item3));
                if (res.Item1 > 0.95)
                {
                    Point targetP = new Point(res.Item3.X + res.Item3.Width / 2, res.Item3.Y + res.Item3.Height / 2);
                    Program.logIt(string.Format("found scroll con. location={0}, similarity={1}", targetP, res.Item1));
                    // move cursor to the location and click
                    Point firstP = Point.Empty;
                    bool done = false;
                    do
                    {
                        Tuple<bool, Rectangle, Bitmap> mi = ImUtility.find_focused_menu(m1);
                        Program.logIt(string.Format("focus item: {0}", mi.Item2));
                        if (firstP.IsEmpty)
                        {
                            firstP = new Point(mi.Item2.X + mi.Item2.Width / 2, mi.Item2.Y + mi.Item2.Height / 2);
                        }
                        else
                        {
                            if (mi.Item2.Contains(firstP))
                            {
                                // on first item point
                                Program.logIt(string.Format("found first item: {0}", mi.Item2));
                                done = true;
                            }
                        }
                        if (mi.Item2.Contains(targetP))
                        {
                            // on target point;
                            Program.logIt(string.Format("found target item: {0}", mi.Item2));
                            b1 = new Bitmap(current_raw_image);
                            KeyInput.getInstance().sendKey(0x52);
                            waitForScreenChange(b1);
                            done = true;
                            waitForScreenStable();
                        }
                        else
                        {
                            // move next
                            b1 = new Bitmap(current_raw_image);
                            KeyInput.getInstance().sendKey(0x4f);
                            waitForScreenChange(b1);
                            b2 = new Bitmap(current_raw_image);
                            m1 = ImUtility.crop_image(b2, rmenu);
                        }
                    } while (!done);

                    // enter the scroll sub_menu, 
                    //string way = "left";
                    b2 = new Bitmap(current_raw_image);
                    menu = ImUtility.extrac_context_menu(org_screen, b2);
                    m1 = null;
                    rmenu = Rectangle.Empty;
                    if (menu.Item1 && menu.Item3 != null)
                    {
                        m1 = new Bitmap(menu.Item3);
                        rmenu = menu.Item2;
                        m1.Save("temp_2.jpg");
                    }
                    if(m1!=null)
                    {
                        res = ImUtility.multiscale_matchtemplate(m1, new Bitmap(string.Format(@"images\scroll_{0}_icon.jpg", way)));
                        Program.logIt(string.Format("found scroll {0} icon. location={1}, similarity={2}", way, res.Item3, res.Item1));
                        if (res.Item1 > 0.95)
                        {
                            targetP = new Point(res.Item3.X + res.Item3.Width / 2, res.Item3.Y + res.Item3.Height / 2);
                            Program.logIt(string.Format("found scroll con. location={0}, similarity={1}", targetP, res.Item1));
                            // move cursor to the location and click
                            firstP = Point.Empty;
                            done = false;
                            do
                            {
                                Tuple<bool, Rectangle, Bitmap> mi = ImUtility.find_focused_menu(m1);
                                Program.logIt(string.Format("focus item: {0}", mi.Item2));
                                if (firstP.IsEmpty)
                                {
                                    firstP = new Point(mi.Item2.X + mi.Item2.Width / 2, mi.Item2.Y + mi.Item2.Height / 2);
                                }
                                else
                                {
                                    if (mi.Item2.Contains(firstP))
                                    {
                                        // on first item point
                                        Program.logIt(string.Format("found first item: {0}", mi.Item2));
                                        done = true;
                                    }
                                }
                                if (mi.Item2.Contains(targetP))
                                {
                                    // on target point;
                                    Program.logIt(string.Format("found target item: {0}", mi.Item2));
                                    b1 = new Bitmap(current_raw_image);
                                    KeyInput.getInstance().sendKey(0x52);
                                    waitForScreenChange(b1);
                                    done = true;
                                    waitForScreenStable();
                                }
                                else
                                {
                                    // move next
                                    b1 = new Bitmap(current_raw_image);
                                    KeyInput.getInstance().sendKey(0x4f);
                                    waitForScreenChange(b1);
                                    b2 = new Bitmap(current_raw_image);
                                    m1 = ImUtility.crop_image(b2, rmenu);
                                }
                            } while (!done);
                        }
                    }
                }

                // exit menu
                // exit menu
                b2 = new Bitmap(current_raw_image);
                KeyInput.getInstance().sendKey(0x48);
                waitForScreenChange(b2);
                b2 = new Bitmap(current_raw_image);
                KeyInput.getInstance().sendKey(0x48);
                waitForScreenChange(b2);
                KeyInput.getInstance().sendKey(0x50);
                KeyInput.getInstance().sendKey(0x51);
            }
        }
        void test() { }
        private async void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!busy)
            {
                this.busy = true;
                this.busy_text = "Testing in progress.\n Please wait.";
                await Task.Run(() =>
                {
                    Program.logIt("test: ++");
                    test();
                    Program.logIt("test: --");
                });
                this.busy = false;
            }
        }
        bool on_top_of_setting_menu(Bitmap src)
        {
            bool ret = false;
            Image<Bgra, Byte> b = new Image<Bgra, Byte>(src);
            UMat uimage = new UMat();
            CvInvoke.CvtColor(b, uimage, ColorConversion.Bgr2Gray);
            Image<Gray, Byte> img2 = uimage.ToImage<Gray, Byte>().Not();
            MCvScalar m1 = new MCvScalar();
            MCvScalar m2 = new MCvScalar();
            CvInvoke.MeanStdDev(img2, ref m1, ref m2);
            Image<Gray, Byte> t = img2.ThresholdBinary(new Gray(m1.V0 + m2.V0), new Gray(255));
            Rectangle r = new Rectangle(0, 50, img2.Width, 40);
            Image<Gray, Byte> roi = t.GetSubRect(r);
            string s = ocrEng.ocr_easy(roi.Bitmap);
            if (!string.IsNullOrEmpty(s) )
            {
                s = s.Trim();
                if( string.Compare(s, "Settings", true) == 0)
                    ret = true;
            }
            return ret;
        }
        void go_to_top_of_setting()
        {
            go_to_settings(true, true);
            double width_limit = 0.75* current_raw_image.Width;
            // 
            bool done = false;
            while (!done)
            {
                Tuple<Bitmap, Rectangle> rt = get_first_item_rectangle();
                Bitmap current_bitmap = rt.Item1;
                Rectangle first_rect = rt.Item2;
                Point first_center = new Point(first_rect.X + first_rect.Width / 2, first_rect.Y + first_rect.Height / 2);
                Program.logIt(string.Format("focus rect: {0} limit={1}", first_rect, width_limit));
                if (first_rect.Width > (int)width_limit)
                    done = true;
                else
                {
                    KeyInput.getInstance().sendKey(0x52);
                    waitForScreenStable();
                }
            }
        }
        void put_device_ready()
        {
            // 1. go to settings and go back to top level menu of setting
            // 2. go to app switch, and close all apps
            //
            go_to_settings(true, true);
            //go_to_top_of_setting();
        }
        void click_home_v2(int wait = 5000)
        {
            Program.logIt("click_home_v2: ++");
            KeyInput.getInstance().sendKey(0x4a);
            waitForScreenStable(250, 4, 50, 15);
            //Bitmap b = new Bitmap(current_raw_image);
            //b.Save("temp_1.jpg");
            if (homescreen == null)
            {
                Tuple<Bitmap, Rectangle> res = get_first_item_rectangle();
                if (res.Item1 != null)
                {
                    homescreen = res.Item1;
                    //homescreen.Save("temp_2.jpg");
                }
            }
            Program.logIt("click_home_v2: --");
        }
        void click_home(int wait = 5000)
        {
            bool done = false;
            DateTime _start = DateTime.Now;
            Program.logIt(string.Format("click_home: ++ {0}", _start.ToString("o")));
            Bitmap b1 = new Bitmap(current_raw_image);
            //b1.Save("temp_0.jpg");
            KeyInput.getInstance().sendKey(0x4a);
            //System.Threading.Thread.Sleep(1000);
            while (!done && (DateTime.Now - _start).TotalMilliseconds < wait)
            {
                System.Threading.Thread.Sleep(50);
                try
                {
                    Bitmap b2 = new Bitmap(current_raw_image);
                    if (!ImUtility.is_same_image(b1, b2))
                    {
                        Tuple<bool, Rectangle, Bitmap> r = ImUtility.extrac_blue_block(b2, new Size(100, 100));
                        if (r.Item1)
                        {
                            Program.logIt(string.Format("Found blue Block: {0}", r.Item2));
                            done = true;
                        }
                        else
                            Program.logIt(string.Format("NOT Found blue Block: "));
                        b1 = b2;
                    }
                }
                catch (Exception) { }
            }
            Program.logIt(string.Format("click_home: -- {0}", DateTime.Now.ToString("o")));
        }
        string read_imei()
        {
            string ret = string.Empty;
            Bitmap b1 = new Bitmap(current_raw_image);
            //go_to_dialer_v2(true,true);
            go_to_dialer(true, true);
            //Bitmap dialer = new Bitmap(@"C:\logs\pictures\dialer_keypad.jpg");
            //bool done = false;
            //while (!done)
            //{
            //    b1 = new Bitmap(current_raw_image);
            //    if (ImUtility.is_same_image(b1, dialer, 30))
            //    {
            //        done = true;
            //    }
            //}


            //if (false)
            {
                // wait for dialer;
                //System.Threading.Thread.Sleep(2500);
                //KeyInput.getInstance().sendKey(0x50);
                //KeyInput.getInstance().sendKey(0x50);
                //KeyInput.getInstance().sendKey(0x52);
                System.Threading.Thread.Sleep(2000);
                // check diarler screen
                // click *#06#
                int i = 10;
                // *
                for (int j = 0; j < i; j++)
                    KeyInput.getInstance().sendKey(0x4f);
                KeyInput.getInstance().sendKey(0x52);
                System.Threading.Thread.Sleep(1000);
                // #
                i = 11;
                for (int j = 0; j < i; j++)
                    KeyInput.getInstance().sendKey(0x4f);
                KeyInput.getInstance().sendKey(0x52);
                System.Threading.Thread.Sleep(1000);
                // 0
                i = 10;
                for (int j = 0; j < i; j++)
                    KeyInput.getInstance().sendKey(0x4f);
                KeyInput.getInstance().sendKey(0x52);
                System.Threading.Thread.Sleep(1000);
                // 6
                i = 5;
                for (int j = 0; j < i; j++)
                    KeyInput.getInstance().sendKey(0x4f);
                KeyInput.getInstance().sendKey(0x52);
                System.Threading.Thread.Sleep(1000);
                // #
                i = 11;
                for (int j = 0; j < i; j++)
                    KeyInput.getInstance().sendKey(0x4f);
                KeyInput.getInstance().sendKey(0x52);
                //System.Threading.Thread.Sleep(1000);
                // imei will display on the screen 
                System.Threading.Thread.Sleep(2000);
                Image<Bgr, Byte> b = new Image<Bgr, Byte>(current_image);
                UMat uimage = new UMat();
                CvInvoke.CvtColor(b, uimage, ColorConversion.Bgr2Gray);
                MCvScalar m1 = new MCvScalar();
                MCvScalar m2 = new MCvScalar();
                CvInvoke.MeanStdDev(uimage, ref m1, ref m2);
                Image<Gray, Byte> t = uimage.ToImage<Gray, Byte>().ThresholdBinary(new Gray(m1.V0 + m2.V0), new Gray(255));
                Rectangle r = new Rectangle(0, t.Height / 2 - 200, t.Width, 200);
                Image<Gray, Byte> roi = t.GetSubRect(r);
                ret = ocrEng.ocr_easy(roi.Bitmap);
                //if (!string.IsNullOrEmpty(s))
                //{
                //    //Program.logIt(string.Format("IMEI={0}", s));
                //    MessageBox.Show(s, "IMEI");
                //}
                b1 = new Bitmap(current_raw_image);
                KeyInput.getInstance().sendKey(0x52);
                waitForScreenChange(b1);
                waitForScreenStable(250, 4);
                //KeyInput.getInstance().sendKey(0x52);
                //System.Threading.Thread.Sleep(1000);
                //waitForScreenStable(250, 4);
                KeyInput.getInstance().sendKey(0x4a);
            }
            return ret;
        }
        void go_to_dialer_v2(bool tap = false, bool clickhome = false)
        {
            if (clickhome) click_home();
            int n = 20;
            for (int i = 0; i < n; i++)
            {
                KeyInput.getInstance().sendKey(0x4f);
                System.Threading.Thread.Sleep(100);
            }
            if (tap)
                KeyInput.getInstance().sendKey(0x52);
        }
        void go_to_dialer(bool tap=false, bool clickhome = false)
        {
            if (clickhome)
                click_home_v2();
            // look for phone icon rectangle;
            Rectangle phone_rect = Rectangle.Empty;
            try
            {
                Image<Bgr, Byte> img = new Image<Bgr, byte>(current_raw_image);
                CascadeClassifier haar_email = new CascadeClassifier(@"trained\phone_cascade.xml");
                Rectangle[] rects = haar_email.DetectMultiScale(img);
                foreach (var r in rects)
                {
                    Program.logIt(string.Format("phone position: {0}", r));
                    if (r.Width * r.Height > phone_rect.Width * phone_rect.Height)
                        phone_rect = r;
                }
            }
            catch (Exception) { }
            if (!phone_rect.IsEmpty)
            {
                int n = 36;
                bool done = false;
                bool not_found = false;
                //Bitmap home_screen = get_homescreen_without_bluebox();
                Bitmap home_screen = new Bitmap(homescreen);
                Point point_of_first_item = Point.Empty;
                while (!done && n>0 && !not_found)
                {
                    Bitmap b = new Bitmap(current_raw_image);
                    KeyInput.getInstance().sendKey(0x4f);
                    waitForScreenChange(b, 30);
                    b = new Bitmap(current_raw_image);
                    Rectangle br = ImUtility.detect_blue_rectangle(home_screen, b);
                    if (point_of_first_item.IsEmpty)
                    {
                        point_of_first_item = new Point(br.X + br.Width / 2, br.Y + br.Height / 2);
                        Program.logIt(string.Format("set first item: {0}", br));
                    }
                    else
                    {
                        if (br.Contains(point_of_first_item))
                        {
                            Program.logIt(string.Format("Loop back to first item: {0}", br));
                            not_found = true;
                        }
                    }
                    //foreach (Rectangle pr in phone_rect)
                    {
                        if (br.Contains(phone_rect))
                        {
                            done = true;
                            break;
                        }
                    }
                }
                if (done)
                {
                    Program.logIt("go_to_dialer: done.");
                    if (tap)
                    {
                        Bitmap b = new Bitmap(current_raw_image);
                        KeyInput.getInstance().sendKey(0x52);
                        waitForScreenChange(b);
                        waitForScreenStable(250, 4);
                        //System.Threading.Thread.Sleep(delay);
                        //b = new Bitmap(current_raw_image);
                        //b.Save("temp_1.jpg");
                    }
                }
            }
        }
        void go_to_settings(bool tap=false, bool clickhome = false)
        {
            Program.logIt("go_to_settings: ++");
            if (clickhome) click_home_v2();
            // look for phone icon rectangle;
            Rectangle[] phone_rect = null;
            try
            {
                Image<Bgr, Byte> img = new Image<Bgr, byte>(current_raw_image);
                CascadeClassifier haar_email = new CascadeClassifier(@"trained\settings_cascade.xml");
                phone_rect = haar_email.DetectMultiScale(img);
                foreach (var r in phone_rect)
                {
                    Program.logIt(string.Format("setting={0}", r));
                }
            }
            catch (Exception) { }
            if (phone_rect.Length > 0)
            {
                int n = 36;
                bool done = false;
                bool not_found = false;
                //Bitmap home_screen = get_homescreen_without_bluebox();
                Bitmap home_screen = new Bitmap(homescreen);
                Point point_of_first_item = Point.Empty;
                while (!done && n > 0 && !not_found)
                {
                    Bitmap b = new Bitmap(current_raw_image);
                    KeyInput.getInstance().sendKey(0x4f);
                    waitForScreenChange(b, 30);
                    b = new Bitmap(current_raw_image);
                    Rectangle br = ImUtility.detect_blue_rectangle(home_screen, b);
                    //Program.logIt(string.Format("Found blue Block: {0}", br));
                    if (point_of_first_item.IsEmpty)
                    {
                        point_of_first_item = new Point(br.X + br.Width / 2, br.Y + br.Height / 2);
                        Program.logIt(string.Format("set first item: {0}", br));
                    }
                    else
                    {
                        if (br.Contains(point_of_first_item))
                        {
                            Program.logIt(string.Format("Loop back to first item: {0}", br));
                            not_found = true;
                        }
                    }
                        
                    foreach (Rectangle pr in phone_rect)
                    {
                        if (br.Contains(pr))
                        {
                            done = true;
                            break;
                        }
                    }
                }
                if (done)
                {
                    Program.logIt("go_to_settings: done.");
                    if (tap)
                    {
                        Bitmap b = new Bitmap(current_raw_image);
                        KeyInput.getInstance().sendKey(0x52);
                        waitForScreenChange(b);
                        waitForScreenStable(250, 4);
                        //System.Threading.Thread.Sleep(delay);
                        //b = new Bitmap(current_raw_image);
                        //b.Save("temp_1.jpg");
                    }
                }
            }
            Program.logIt("go_to_settings: --");
        }
        void get_icon_rectangle()
        {
            /*
            System.Collections.Generic.Dictionary<int, Rectangle> homepage_icons = new Dictionary<int, Rectangle>();
            // click home
            KeyInput.getInstance().sendKey(0x4a);
            System.Threading.Thread.Sleep(2000);
            System.Threading.Thread.Sleep(2000);
            Image<Bgr, Byte> home_img = new Image<Bgr, Byte>(current_image);
            // click previous
            KeyInput.getInstance().sendKey(0x50);
            System.Threading.Thread.Sleep(2000);
            Image<Bgr, Byte> b2 = new Image<Bgr, Byte>(current_image);
            //home_img.Save("temp_1.jpg");
            //b2.Save("temp_2.jpg");
            Rectangle[] rects = ImUtility.detect_blue_rectangle(home_img, b2);
            if (rects.Length == 2)
            {
                if (rects[0].X < rects[1].X)
                {
                    homepage_icons[10] = rects[0];
                    homepage_icons[03] = rects[1];
                }
                else
                {
                    homepage_icons[10] = rects[1];
                    homepage_icons[03] = rects[0];
                }
            }

            // click mext 2 times
            KeyInput.getInstance().sendKey(0x4f);
            KeyInput.getInstance().sendKey(0x4f);
            KeyInput.getInstance().sendKey(0x4f);
            System.Threading.Thread.Sleep(2000);
            b2 = new Image<Bgr, Byte>(current_image);
            home_img.Save("temp_1.jpg");
            b2.Save("temp_2.jpg");
            rects = ImUtility.detect_blue_rectangle(home_img, b2);
            if (rects.Length == 2)
            {
                if (rects[0].X < rects[1].X)
                {
                    //homepage_icons[10] = rects[0];
                    homepage_icons[12] = rects[1];
                }
                else
                {
                    //homepage_icons[10] = rects[1];
                    homepage_icons[12] = rects[0];
                }
            }
            // 
            KeyInput.getInstance().sendKey(0x4f);
            KeyInput.getInstance().sendKey(0x4f);
            KeyInput.getInstance().sendKey(0x4f);
            KeyInput.getInstance().sendKey(0x4f);
            KeyInput.getInstance().sendKey(0x4f);
            System.Threading.Thread.Sleep(2000);
            b2 = new Image<Bgr, Byte>(current_image);
            rects = ImUtility.detect_blue_rectangle(home_img, b2);
            if (rects.Length == 2)
            {
                if (rects[0].X < rects[1].X)
                {
                    //homepage_icons[11] = rects[0];
                    homepage_icons[23] = rects[1];
                }
                else
                {
                    //homepage_icons[11] = rects[1];
                    homepage_icons[23] = rects[0];
                }
            }

            // next line
            KeyInput.getInstance().sendKey(0x4f);
            KeyInput.getInstance().sendKey(0x4f);
            System.Threading.Thread.Sleep(2000);
            //Image<Bgr, Byte> b1 = new Image<Bgr, Byte>(current_image);
            // click mext 2 times
            b2 = new Image<Bgr, Byte>(current_image);
            rects = ImUtility.detect_blue_rectangle(home_img, b2);
            if (rects.Length == 2)
            {
                if (rects[0].X < rects[1].X)
                {
                    //homepage_icons[11] = rects[0];
                    homepage_icons[31] = rects[1];
                }
                else
                {
                    //homepage_icons[11] = rects[1];
                    homepage_icons[31] = rects[0];
                }
            }

            // next line
            if(false)
            {
                KeyInput.getInstance().sendKey(0x4f);
                KeyInput.getInstance().sendKey(0x4f);
                KeyInput.getInstance().sendKey(0x4f);
                KeyInput.getInstance().sendKey(0x4f);
                System.Threading.Thread.Sleep(2000);
                //Image<Bgr, Byte> b1 = new Image<Bgr, Byte>(current_image);
                // click mext 2 times
                b2 = new Image<Bgr, Byte>(current_image);
                rects = ImUtility.detect_blue_rectangle(home_img, b2);
                if (rects.Length == 2)
                {
                    if (rects[0].Y < rects[1].Y)
                    {
                        //homepage_icons[11] = rects[0];
                        homepage_icons[41] = rects[1];
                    }
                    else
                    {
                        //homepage_icons[11] = rects[1];
                        homepage_icons[41] = rects[0];
                    }
                }
            }
            KeyInput.getInstance().sendKey(0x4a);
            // dump 
            foreach (var i in homepage_icons)
            {
                Program.logIt(string.Format("{0}={1}", i.Key, i.Value));
            }
            // make all icon position
            // we know: 
            //[2018-08-17T21:00:25.1911092-07:00]: 10={X=37,Y=26,Width=108,Height=151}
            //[2018-08-17T21:00:25.1921071-07:00]: 3={X=401,Y=852,Width=157,Height=147}
            //[2018-08-17T21:00:25.1931083-07:00]: 12={X=238,Y=65,Width=237,Height=155}
            //[2018-08-17T21:00:25.1931083-07:00]: 23={X=404,Y=131,Width=174,Height=224}
            //[2018-08-17T21:00:25.1941083-07:00]: 31={X=139,Y=263,Width=158,Height=194}
            int[] xs = new int[] 
            {
                (homepage_icons.ContainsKey(10))? homepage_icons[10].X:37,
                (homepage_icons.ContainsKey(31))? homepage_icons[31].X:139,
                (homepage_icons.ContainsKey(12))? homepage_icons[12].X:238,
                (homepage_icons.ContainsKey(23))? homepage_icons[23].X:404,
            };
            int[] ys = new int[]
            {
                (homepage_icons.ContainsKey(10))? homepage_icons[10].Y:26,
                (homepage_icons.ContainsKey(23))? homepage_icons[23].Y:131,
                (homepage_icons.ContainsKey(31))? homepage_icons[31].Y:263,
                //(homepage_icons.ContainsKey(23))? homepage_icons[23].X:404,
            };
            int y_step = 132;
            int y_end = (homepage_icons.ContainsKey(3)) ? homepage_icons[3].Y : 852;
            int w = 157;
            int h = 155;
            int y = 0;
            for (int row=0; row < 10; row++)
            {
                if (row >= ys.Length) y += y_step;
                else y = ys[row];
                if (y+h >= y_end) break;
                for (int x = 0; x < 4; x++)
                {
                    if (!homepage_icons.ContainsKey(row * 10 +10+ x))
                        homepage_icons.Add(row * 10+10 + x, new Rectangle(xs[x], y, w, h));
                }
            }
            for(int x = 0; x < 4; x++)
            {
                if (!homepage_icons.ContainsKey(x))
                    homepage_icons.Add(x, new Rectangle(xs[x], homepage_icons[3].Y, homepage_icons[3].Width, homepage_icons[3].Height));
            }
            // all home page icons location:
            Program.logIt(string.Format("All icons location:"));
            Bitmap b1 = (Bitmap)current_image.Clone();
            foreach (var i in homepage_icons)
            {
                Program.logIt(string.Format("{0}={1}", i.Key, i.Value));
                Crop c = new Crop(i.Value);
                c.Apply(b1).Save(string.Format("icon_{0}.jpg", i.Key));
            }
            */
        }
        void get_homescreen_icons(System.Collections.Generic.Dictionary<string, object> homescreeninfo, int page, Bitmap src=null)
        {
            Program.logIt(string.Format("get_homescreen_icons: ++ page={0}", page));
            try
            {
                var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                System.Collections.Generic.Dictionary<string, object> conf = jss.Deserialize<System.Collections.Generic.Dictionary<string, object>>(System.IO.File.ReadAllText("HomeScreenLayout.json"));
                Image<Bgra, Byte> b = new Image<Bgra, Byte>(src == null ? current_image : src);
                System.Collections.Generic.Dictionary<string, object> loc = ImUtility.extra_icon(b);
                if (conf.ContainsKey("icon_label"))
                {
                    System.Collections.Generic.Dictionary<string, object> d = (System.Collections.Generic.Dictionary<string, object>)conf["icon_label"];
                    foreach(KeyValuePair<string,object> kv in d)
                    {
                        Rectangle[] r = ImUtility.haar_detect(kv.Value as string, b);
                        if(r!=null)
                        {
                            if(r.Length==1)
                            {
                                System.Drawing.Point p = new Point(r[0].X + r[0].Width / 2, r[0].Y + r[0].Height / 2);
                                string s = find_location(loc, p);
                                if (!string.IsNullOrEmpty(s))
                                {
                                    if (loc.ContainsKey(s))
                                    {
                                        System.Collections.Generic.Dictionary<string, object> d1 = (System.Collections.Generic.Dictionary<string, object>)loc[s];
                                        d1["label"] = kv.Key;
                                    }
                                }
                            }
                        }
                    }
                }
                homescreeninfo[page.ToString()] = loc;
            }
            catch (Exception) { }
            // dump icon labels
            if(homescreeninfo.ContainsKey(page.ToString()))
            {
                System.Collections.Generic.Dictionary<string, object> p = (System.Collections.Generic.Dictionary<string, object>)homescreeninfo[page.ToString()];
                foreach(KeyValuePair<string,object> kv in p)
                {
                    System.Collections.Generic.Dictionary<string, object> i = (System.Collections.Generic.Dictionary<string, object>)kv.Value;
                    if(i.ContainsKey("label") && !string.IsNullOrEmpty(i["label"] as string))
                    {
                        Program.logIt(string.Format("Found: {0}: {1}, rect={2}", kv.Key, i["label"], i["rectangle"]));
                    }
                }
            }
            Program.logIt(string.Format("get_homescreen_icons: --"));
        }
        string find_location(System.Collections.Generic.Dictionary<string,object> locs, System.Drawing.Point p)
        {
            string ret = "";
            foreach(KeyValuePair<string,object> kv in locs)
            {
                System.Collections.Generic.Dictionary<string, object> d = (System.Collections.Generic.Dictionary<string, object>)kv.Value;
                if (d.ContainsKey("rectangle"))
                {
                    Rectangle r = (Rectangle)d["rectangle"];
                    if (r.Contains(p))
                    {
                        ret = kv.Key;
                        break;
                    }
                }
            }
            return ret;
        }
        Tuple<bool, string, System.Collections.Generic.Dictionary<string, object>> find_icon_by_label(string label)
        {
            bool retB = false;
            string retPage = "0";
            System.Collections.Generic.Dictionary<string, object> retD = new Dictionary<string, object>();
            foreach(KeyValuePair<string,object> page in home_screen_info)
            {
                System.Collections.Generic.Dictionary<string, object> p = (System.Collections.Generic.Dictionary<string, object>)page.Value;
                foreach (KeyValuePair<string, object> kv in p)
                {
                    System.Collections.Generic.Dictionary<string, object> i = (System.Collections.Generic.Dictionary<string, object>)kv.Value;
                    if (i.ContainsKey("label") && string.Compare(label, i["label"] as string, true)==0)
                    {
                        retB = true;
                        retPage = page.Key;
                        retD = i;
                        break;
                    }
                }
                if (retB)
                    break;
            }
            return new Tuple<bool, string, Dictionary<string, object>>(retB,retPage,retD);
        }

        private void readIMEIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                string s = read_imei();
                if (!string.IsNullOrEmpty(s))
                    MessageBox.Show(s, "IMEI");
            });
        }
        Bitmap get_image_v2()
        {
            Bitmap ret = null;
            try
            {
                string s = Program.args.ContainsKey("label") ? Program.args["label"] : "1";
                string url = string.Format(@"http://localhost:21173/getScreen?label={0}", s);
                WebClient wc = new WebClient();
                byte[] data = wc.DownloadData(url);
                using (var ms = new MemoryStream(data))
                {
                    ret = new Bitmap(ms);
                    //ret = ImUtility.fromPPM(data);
                }
            }
            catch(Exception ex)
            {
                Program.logIt(ex.Message);
                Program.logIt(ex.StackTrace);
            }
            return ret;
        }
        Bitmap get_image()
        {
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
                                //using (FileStream stream = new FileStream(fn, FileMode.Open, FileAccess.Read))
                                //{
                                //    Image img = Image.FromStream(stream);
                                //    bmp = new Bitmap(img);
                                //}
                                using (var bmpTemp = new Bitmap(fn))
                                {
                                    bmp = new Bitmap(bmpTemp);
                                }
                                break;
                            }
                            catch (Exception ex)
                            {
                                //Program.logIt(ex.Message);
                            }
                        }
                    }
                } while ((DateTime.Now - _s).TotalSeconds < 3);
            }
            catch (Exception ex)
            {
                Program.logIt(ex.Message);
                Program.logIt(ex.StackTrace);
            }
            return bmp;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            // get frame
            Bitmap frame = get_image_v2();
            if (frame != null)
            {
                if(frame_roi.IsEmpty)
                {
                    Image<Bgr, Byte> img = new Image<Bgr, byte>(frame);
                    int x1 = -1, x2 = 0;
                    for (int i = 0; i < img.Width; i++)
                    {
                        Byte b = img.Data[0, i, 0];
                        Byte g = img.Data[0, i, 1];
                        Byte r = img.Data[0, i, 2];
                        if (b != 0 || g != 0 || r != 0)
                        {
                            if (x1 < 0) x1 = i;
                            x2 = i;
                        }
                    }
                    frame_roi = new Rectangle(x1, 0, x2 - x1 + 1, img.Height);
                    Program.logIt(string.Format("rect={0}", frame_roi));
                }
                if(!frame_roi.IsEmpty)
                {
                    Image<Bgr, Byte> img = new Image<Bgr, byte>(frame);
                    Image<Bgr, Byte> img1 = img.GetSubRect(frame_roi);
                    current_raw_image = new Bitmap(img1.Bitmap);
                    current_image = new Bitmap(img1.Resize(raw_to_show, Inter.Cubic).Bitmap);
                    pictureBox1.Invoke(new MethodInvoker(delegate
                    {
                        pictureBox1.Parent.ClientSize= current_image.Size;
                        pictureBox1.Size = current_image.Size;                        
                        pictureBox1.Image = new Bitmap(current_image);
                        if (busy)
                        {
                            using (Graphics g = Graphics.FromImage(pictureBox1.Image))
                            {
                                g.FillRectangle(new SolidBrush(Color.FromArgb(150, Color.DarkGray)), new Rectangle(0, 0, current_image.Width, current_image.Height));
                                StringFormat format = new StringFormat();
                                format.LineAlignment = StringAlignment.Center;
                                format.Alignment = StringAlignment.Center;
                                Font font = new Font("Times New Roman", 24.0f);
                                g.DrawString(this.busy_text, font, Brushes.MediumVioletRed, pictureBox1.ClientRectangle, format);
                            }
                            //pictureBox1.Image = b;
                        }                        
                    }));
                }
                //current_raw_image = new Bitmap(frame);
                //pictureBox1.Invoke(new MethodInvoker(delegate
                //{
                //    current_image = new Bitmap(frame);
                //    pictureBox1.Image = new Bitmap(current_image);
                //}));
            }
        }

        private async void closeAppsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!busy)
            {
                busy = true;
                busy_text = "Close all apps in progress.\nPlease wait.";
                await Task.Run(() =>
                {
                    go_to_top_of_setting();
                    close_all_apps_v2();
                });
                busy = false;
            }
        }

        private void stopScanningToolStripMenuItem_Click(object sender, EventArgs e)
        {
            KeyInput.getInstance().sendKey(0x48);
        }

        private async void upToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!busy)
            {
                busy = true;
                busy_text = "Scroll Up in progress.\nPlease wait.";
                await Task.Run(() =>
                {
                    if (recording_actions != null)
                    {
                        System.Collections.Generic.Dictionary<string, object> dict = new Dictionary<string, object>();
                        dict.Add("action", "scroll");
                        dict.Add("way", "up");
                        dict.Add("timestamp", DateTime.Now.Ticks);
                        recording_actions.Enqueue(dict);
                    }
                    scroll_screen_v2("up");
                });
                busy = false;
            }            
        }

        private async void downToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!busy)
            {
                busy = true;
                busy_text = "Scroll Down in progress.\nPlease wait.";
                await Task.Run(() =>
                {
                    if (recording_actions != null)
                    {
                        System.Collections.Generic.Dictionary<string, object> dict = new Dictionary<string, object>();
                        dict.Add("action", "scroll");
                        dict.Add("way", "down");
                        dict.Add("timestamp", DateTime.Now.Ticks);
                        recording_actions.Enqueue(dict);
                    }
                    scroll_screen_v2("down");
                });
                busy = false;
            }
        }

        private async void leftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!busy)
            {
                busy = true;
                busy_text = "Scroll Left in progress.\nPlease wait.";
                await Task.Run(() =>
                {
                    if (recording_actions != null)
                    {
                        System.Collections.Generic.Dictionary<string, object> dict = new Dictionary<string, object>();
                        dict.Add("action", "scroll");
                        dict.Add("way", "left");
                        dict.Add("timestamp", DateTime.Now.Ticks);
                        recording_actions.Enqueue(dict);
                    }
                    scroll_screen_v2("left");
                });
                busy = false;
            }
        }

        private async void rightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!busy)
            {
                busy = true;
                busy_text = "Scroll Right in progress.\nPlease wait.";
                await Task.Run(() =>
                {
                    if (recording_actions != null)
                    {
                        System.Collections.Generic.Dictionary<string, object> dict = new Dictionary<string, object>();
                        dict.Add("action", "scroll");
                        dict.Add("way", "right");
                        dict.Add("timestamp", DateTime.Now.Ticks);
                        recording_actions.Enqueue(dict);
                    }
                    scroll_screen_v2("right");
                });
                busy = false;
            }
        }

        private async void goToSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!busy)
            {
                busy = true;
                busy_text = "Go to Settings in progress.\nPlease wait.";
                await Task.Run(() =>
                {
                    go_to_settings(true, true);
                });
                busy = false;
            }
        }
        void run_script(string zipfilename)
        {
            Program.logIt(string.Format("run_script: ++ {0}", zipfilename));
            string folder = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(zipfilename), "temp");
            try
            {
                System.IO.Directory.CreateDirectory(folder);
                ZipFile.ExtractToDirectory(zipfilename, folder);
            }
            catch (Exception) { }
            try
            {
                string fn = System.IO.Path.Combine(folder, "mobileQ.json");
                if (System.IO.File.Exists(fn))
                {
                    var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                    var mq = jss.Deserialize<Dictionary<string, object>>(System.IO.File.ReadAllText(fn));
                    if (mq.ContainsKey("mobileQ"))
                    {
                        ArrayList a = (ArrayList)mq["mobileQ"];
                        foreach(var obj in a)
                        {
                            Dictionary<string, object> data = (Dictionary<string, object>)obj;
                            string action = data.ContainsKey("action") ? data["action"] as string : string.Empty;
                            if (!string.IsNullOrEmpty(action))
                            {
                                if (string.Compare(action, "gotohome", true) == 0)
                                {
                                    click_home_v2();
                                }
                                else if (string.Compare(action, "key", true) == 0)
                                {
                                    if(data.ContainsKey("char"))
                                    {
                                        KeyInput.getInstance().sendKey((byte)data["char"]);
                                    }
                                }
                                else if (string.Compare(action, "scroll", true) == 0)
                                {
                                    if (data.ContainsKey("way"))
                                    {
                                        string s = data["way"] as string;
                                        scroll_screen(s);
                                    }
                                }
                                else if (string.Compare(action, "tapxy", true) == 0)
                                {
                                    int x = data.ContainsKey("x") ? (int)data["x"] : -1;
                                    int y = data.ContainsKey("y") ? (int)data["y"] : -1; ;
                                    int k = data.ContainsKey("key") ? (int)data["key"] : -1; ;
                                    string s = data.ContainsKey("image") ? data["image"] as string : string.Empty;
                                    s = System.IO.Path.Combine(folder, s);
                                    if (x > 0 && y > 0)
                                    {
                                        Bitmap b = null;
                                        if (System.IO.File.Exists(s))
                                            b = new Bitmap(s);
                                        tapXY(new Point(x, y), (Byte)k, b);
                                    }
                                }
                            }

                            waitForScreenStable();
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Program.logIt(ex.Message);
                Program.logIt(ex.StackTrace);
            }
            try
            {
                System.IO.Directory.Delete(folder, true);
            }
            catch (Exception) { }
            Program.logIt(string.Format("run_script: --"));
        }
        public void run_script_file(string filename)
        {
            Program.logIt(string.Format("run_script_file: ++ {0}", filename));
            XmlDocument sd = new XmlDocument();
            try { sd.Load(filename); }
            catch (Exception) { }
            if (sd.DocumentElement != null)
            {
                string s = sd.DocumentElement.Name;
                foreach(XmlNode n in sd.DocumentElement.ChildNodes)
                {
                    if(n.NodeType==XmlNodeType.Element && string.Compare(n.Name, "action") == 0)
                    {
                        run_script_action(n);
                    }
                }
            }
            sd.Save(filename);
            Program.logIt(string.Format("run_script_file: --"));
        }
        private void run_script_action(XmlNode node)
        {
            Program.logIt(string.Format("run_script_action: ++ {0}", node.OuterXml));
            if (node!=null && node.NodeType == XmlNodeType.Element && string.Compare(node.Name, "action") == 0)
            {
                bool err = false;
                //foreach(XmlNode n in node.ChildNodes)
                //{
                //    string s = n.Name;
                //    if (string.Compare(s, "save") == 0)
                //        run_script_savenode(n);
                //    if (string.Compare(s, "check") == 0)
                //        err = !run_script_checknode(n);
                //    else if (string.Compare(s, "key") == 0)
                //        run_script_keynode(n);
                //    else if (string.Compare(s, "wait") == 0)
                //        run_script_waitnode(n);
                //    else
                //    {
                //        Program.logIt(string.Format("Not support: {0}", n.OuterXml));
                //    }
                //    if (err)
                //    {
                //        Program.logIt("Error occurs.");
                //        break;
                //    }
                //}
                if (node["precheck"] != null)
                    run_script_checknode(node["precheck"]);
                if (node["key"] != null)
                    run_script_keynode(node["key"]);
                if (node["wait"] != null)
                    run_script_waitnode(node["wait"]);
                if (node["save"] != null)
                    run_script_waitnode(node["save"]);
                if (node["postcheck"] != null)
                    run_script_checknode(node["postcheck"]);
            }
            Program.logIt(string.Format("run_script_action: --"));
        }
        private bool run_script_checknode(XmlNode node)
        {
            bool ret = false;
            Program.logIt(string.Format("run_script_checknode: ++ {0}", node.OuterXml));
            if (node != null && node.NodeType == XmlNodeType.Element && (string.Compare(node.Name, "precheck") == 0|| string.Compare(node.Name, "postcheck") == 0))
            {
                XmlElement e = (XmlElement)node;
                if (e.Attributes["id"] != null)
                {
                    string id = e.Attributes["id"].Value;
                    XmlNode imgNode = node.OwnerDocument.SelectSingleNode(string.Format("//image[@id='{0}']", id));
                    if (imgNode != null)
                    {
                        Bitmap b = null;
                        using (XmlNodeReader r = new XmlNodeReader(imgNode))
                        {
                            r.MoveToContent();
                            using (MemoryStream memStream = new MemoryStream())
                            {
                                int read = 0;
                                byte[] data = new byte[1024 * 100];
                                do
                                {
                                    read = r.ReadElementContentAsBase64(data, 0, data.Length);
                                    if (read > 0)
                                        memStream.Write(data, 0, data.Length);
                                } while (read > 0);
                                //ret = memStream.ToArray();
                                b = new Bitmap(memStream);
                            }
                            r.Close();
                        }
                        if (b != null && ImUtility.is_same_image(b, current_raw_image))
                            ret = true;
                    }
                }
            }
            Program.logIt(string.Format("run_script_checknode: -- ret={0}", ret));
            return ret;
        }
        private void run_script_savenode(XmlNode node)
        {
            Program.logIt(string.Format("run_script_savenode: ++ {0}", node.OuterXml));
            if (node != null && node.NodeType == XmlNodeType.Element && string.Compare(node.Name, "save") == 0)
            {
                XmlElement e = (XmlElement)node;
                if (e.Attributes["id"] != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        current_raw_image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        string id = e.Attributes["id"].Value;
                        XmlNode n = node.OwnerDocument.SelectSingleNode(string.Format("//image[@id='{0}']", id));
                        if (n != null)
                        {
                            n.ParentNode.RemoveChild(n);
                        }
                        n = node.OwnerDocument.SelectSingleNode("//images");
                        if (n == null)
                        {
                            n = node.OwnerDocument.CreateElement("images");
                            n = node.OwnerDocument.DocumentElement.AppendChild(n);
                        }
                        using (XmlWriter xmlWrite = n.CreateNavigator().AppendChild())
                        {
                            xmlWrite.WriteStartElement("image");
                            xmlWrite.WriteAttributeString("id", id);
                            byte[] d = ms.ToArray();
                            xmlWrite.WriteBase64(d, 0, d.Length);
                            xmlWrite.WriteEndElement();
                            xmlWrite.Flush();
                        }
                    }
                }
            }
            Program.logIt(string.Format("run_script_savenode: --"));
        }
        private void run_script_waitnode(XmlNode node)
        {
            Program.logIt(string.Format("run_script_waitnode: ++ {0}", node.OuterXml));
            if (node != null && node.NodeType == XmlNodeType.Element && string.Compare(node.Name, "wait") == 0)
            {
                XmlElement e = (XmlElement)node;
                int interval = 200;
                int framecount = 5;
                int threshold = 50;
                int timeout = 5;
                if (e.Attributes["interval"] != null)
                {
                    if (!Int32.TryParse(e.Attributes["interval"].Value, out interval))
                        interval = 200;
                }
                if (e.Attributes["framecount"] != null)
                {
                    if (!Int32.TryParse(e.Attributes["framecount"].Value, out framecount))
                        framecount = 5;
                }
                if (e.Attributes["threshold"] != null)
                {
                    if (!Int32.TryParse(e.Attributes["threshold"].Value, out threshold))
                        threshold = 50;
                }
                if (e.Attributes["timeout"] != null)
                {
                    if (!Int32.TryParse(e.Attributes["timeout"].Value, out timeout))
                        timeout = 5;
                }
                Program.logIt(string.Format("interval={0}, framecount={1}, threshold={2}, timeout={3}", interval, framecount, threshold, timeout));
                waitForScreenStable(interval, framecount, threshold, timeout);
            }
            Program.logIt("run_script_waitnode: --");
        }
        private void run_script_keynode(XmlNode node)
        {
            Program.logIt(string.Format("run_script_keynode: ++ {0}", node.OuterXml));
            if (node != null && node.NodeType == XmlNodeType.Element && string.Compare(node.Name, "key") == 0)
            {
                XmlElement e = (XmlElement)node;
                int count = 1;
                if (e.Attributes["count"] != null)
                {
                    if (!Int32.TryParse(e.Attributes["count"].Value, out count))
                        count = 1;
                }
                int delay = 0;
                if (e.Attributes["delay"] != null)
                {
                    if (!Int32.TryParse(e.Attributes["delay"].Value, out delay))
                        delay = 0;
                }
                bool wait = true;
                if (e.Attributes["wait"] != null)
                {
                    if (string.Compare(e.Attributes["wait"].Value, bool.FalseString, true) == 0)
                        wait = false;
                }
                Byte key = 0x0;
                if (e.Attributes["char"] != null)
                {
                    key =(Byte) Convert.ToInt32(e.Attributes["char"].Value, 16);
                }
                Program.logIt(string.Format("Char=0x{0:x2}, count={1}, wait={2}, dealy={3}", key, count, wait, delay));
                do
                {
                    Bitmap b1 = new Bitmap(current_raw_image);
                    KeyInput.getInstance().sendKey(key);
                    if (delay > 0)
                        System.Threading.Thread.Sleep(delay);
                    if (wait)
                        waitForScreenChange(b1);
                    count--;
                } while (count > 0);
            }
            Program.logIt("run_script_keynode: --");
        }
        private void  record_process(string target_folder)
        {
            Program.logIt("record_process: ++");
            // 0. close all apps and gsettings
            this.busy = true;
            this.busy_text = "Prepare recording in progress.\n Please wait.";
            //go_to_top_of_setting();
            //close_all_apps_v2();
            this.busy = false;
            this.recording = true;
            // first action is go to home
            {
                System.Collections.Generic.Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("action", "gotohome");
                dict.Add("timestamp", DateTime.Now.Ticks);
                if(recording_actions!=null)
                    recording_actions.Enqueue(dict);
            }
            // wait for imput
            while (recording)
                System.Threading.Thread.Sleep(1000);
            // save actions
            //save_action(target_folder);
            Program.logIt("record_process: --");
        }
        private async void recordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!busy)
            {
                string folderName = System.IO.Path.Combine(Application.StartupPath, "temp");
                string s = recordToolStripMenuItem.Text;
                if (string.Compare(s, "record", true) == 0)
                {
                    System.IO.Directory.CreateDirectory(folderName);
                    recording_actions = new Queue<Dictionary<string, object>>();
                    recordToolStripMenuItem.Text = "Stop";
                    //this.busy = true;
                    //this.busy_text = "Testing in progress.\n Please wait.";
                    await Task.Run(() =>
                    {
                        Program.logIt("record: ++");
                        record_process(folderName);
                        Program.logIt("record: --");
                    });
                    //this.busy = false;
                    recordToolStripMenuItem.Text = "Record";
                }
                else if (string.Compare(s, "stop", true) == 0)
                {
                    this.recording = false;
                    // ask for filename to save
                    SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                    saveFileDialog1.Filter = "Script File|*.zip";
                    saveFileDialog1.Title = "Save a Script File";
                    saveFileDialog1.ShowDialog();
                    if (saveFileDialog1.FileName != "")
                    {
                        await Task.Run(() =>
                        {
                            save_action(saveFileDialog1.FileName, folderName);
                            System.IO.Directory.Delete(folderName, true);
                        });
                    }
                }
            }

        }
        private void save_action(string target_name, string source_name)
        {
            if (recording_actions != null)
            {
                Dictionary<string, object> d = new Dictionary<string, object>();
                var a = recording_actions.ToArray();
                foreach(var i in a)
                {
                    Dictionary<string, string> td = new Dictionary<string, string>();
                    foreach(var j in i)
                    {
                        if (j.Value.GetType() == typeof(Bitmap))
                        {
                            Bitmap b = (Bitmap)j.Value;
                            string fn = System.IO.Path.Combine(source_name, System.IO.Path.GetRandomFileName());
                            fn = System.IO.Path.ChangeExtension(fn, ".jpg");
                            b.Save(fn);
                            td.Add(j.Key, System.IO.Path.GetFileName(fn));
                        }
                    }
                    foreach(var j in td)
                    {
                        i[j.Key] = j.Value;
                    }
                }
                d.Add("mobileQ", a);
                var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                string s = jss.Serialize(d);
                System.IO.File.WriteAllText(System.IO.Path.Combine(source_name, "mobileQ.json"), s);
                //
                // zip target tmp to target folder
                ZipFile.CreateFromDirectory(source_name, target_name);
            }
        }

        private async void playScripToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!busy)
            {
                OpenFileDialog saveFileDialog1 = new OpenFileDialog();
                saveFileDialog1.Filter = "Script File|*.zip";
                saveFileDialog1.Title = "Save a Script File";
                DialogResult result = saveFileDialog1.ShowDialog();
                if (saveFileDialog1.FileName != "")
                {
                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(saveFileDialog1.FileName))
                    {
                        this.busy = true;
                        this.busy_text = String.Format("Playing script in progress.\n Please wait.");
                        await Task.Run(() =>
                        {
                            Program.logIt(string.Format("Play script: ++", saveFileDialog1.FileName));
                            run_script(saveFileDialog1.FileName);
                            Program.logIt("Play script: --");
                        });
                    }
                }
                /*
                using (var fbd = new FolderBrowserDialog())
                {
                    DialogResult result = fbd.ShowDialog();

                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        this.busy = true;
                        this.busy_text = String.Format("Playing script in progress.\n Please wait.");
                        await Task.Run(() =>
                        {
                            Program.logIt(string.Format("Play script: ++", fbd.SelectedPath));
                            run_script(fbd.SelectedPath);
                            Program.logIt("Play script: --");
                        });
                    }
                }*/
                this.busy = false;
            }
        }
    }
}
