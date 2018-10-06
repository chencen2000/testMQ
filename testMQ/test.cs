using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using Emgu.CV.XFeatures2D;
using System.Diagnostics;
using Emgu.CV.Features2D;
using Emgu.CV.ML;
using Emgu.CV.Flann;
using Emgu.CV.Text;
using HtmlAgilityPack;
using System.Xml;
using System.Collections;
using System.IO;

namespace testMQ
{
    public static class DrawMatches
    {
        public static void FindMatch(Mat modelImage, Mat observedImage, out long matchTime, out VectorOfKeyPoint modelKeyPoints, out VectorOfKeyPoint observedKeyPoints, VectorOfVectorOfDMatch matches, out Mat mask, out Mat homography)
        {
            int k = 2;
            double uniquenessThreshold = 0.8;
            double hessianThresh = 300;

            Stopwatch watch;
            homography = null;

            modelKeyPoints = new VectorOfKeyPoint();
            observedKeyPoints = new VectorOfKeyPoint();

            {
                //using (UMat uModelImage = modelImage.ToUMat(AccessType.Read))
                //using (UMat uObservedImage = observedImage.ToUMat(AccessType.Read))
                //UMat uModelImage = new UMat();
                //UMat uObservedImage = new UMat();
                //CvInvoke.convert
                {
                    Brisk surfCPU = new Brisk();
                    //SURF surfCPU = new SURF(hessianThresh);
                    //extract features from the object image
                    UMat modelDescriptors = new UMat();
                    surfCPU.DetectAndCompute(modelImage, null, modelKeyPoints, modelDescriptors, false);

                    watch = Stopwatch.StartNew();

                    // extract features from the observed image
                    UMat observedDescriptors = new UMat();
                    surfCPU.DetectAndCompute(observedImage, null, observedKeyPoints, observedDescriptors, false);
                    BFMatcher matcher = new BFMatcher(DistanceType.L2);
                    matcher.Add(modelDescriptors);

                    matcher.KnnMatch(observedDescriptors, matches, k, null);
                    mask = new Mat(matches.Size, 1, DepthType.Cv8U, 1);
                    mask.SetTo(new MCvScalar(255));
                    Features2DToolbox.VoteForUniqueness(matches, uniquenessThreshold, mask);

                    int nonZeroCount = CvInvoke.CountNonZero(mask);
                    if (nonZeroCount >= 4)
                    {
                        nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(modelKeyPoints, observedKeyPoints,
                           matches, mask, 1.5, 20);
                        if (nonZeroCount >= 4)
                            homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(modelKeyPoints,
                               observedKeyPoints, matches, mask, 2);
                    }

                    watch.Stop();
                }
            }
            matchTime = watch.ElapsedMilliseconds;
        }

        /// <summary>
        /// Draw the model image and observed image, the matched features and homography projection.
        /// </summary>
        /// <param name="modelImage">The model image</param>
        /// <param name="observedImage">The observed image</param>
        /// <param name="matchTime">The output total time for computing the homography matrix.</param>
        /// <returns>The model image and observed image, the matched features and homography projection.</returns>
        public static Mat Draw(Mat modelImage, Mat observedImage, out long matchTime)
        {
            Mat homography;
            VectorOfKeyPoint modelKeyPoints;
            VectorOfKeyPoint observedKeyPoints;
            using (VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch())
            {
                Mat mask;
                FindMatch(modelImage, observedImage, out matchTime, out modelKeyPoints, out observedKeyPoints, matches,
                   out mask, out homography);

                //Draw the matched keypoints
                Mat result = new Mat();
                Features2DToolbox.DrawMatches(modelImage, modelKeyPoints, observedImage, observedKeyPoints,
                   matches, result, new MCvScalar(255, 255, 255), new MCvScalar(255, 255, 255), mask);

                #region draw the projected region on the image

                if (homography != null)
                {
                    //draw a rectangle along the projected model
                    Rectangle rect = new Rectangle(System.Drawing.Point.Empty, modelImage.Size);
                    PointF[] pts = new PointF[]
                    {
                  new PointF(rect.Left, rect.Bottom),
                  new PointF(rect.Right, rect.Bottom),
                  new PointF(rect.Right, rect.Top),
                  new PointF(rect.Left, rect.Top)
                    };
                    pts = CvInvoke.PerspectiveTransform(pts, homography);

                    System.Drawing.Point[] points = Array.ConvertAll<PointF, System.Drawing.Point>(pts, System.Drawing.Point.Round);
                    using (VectorOfPoint vp = new VectorOfPoint(points))
                    {
                        CvInvoke.Polylines(result, vp, true, new MCvScalar(255, 0, 0, 255), 5);
                    }

                }

                #endregion

                return result;

            }
        }
    }

    class Test
    {
        static void Main()
        {
            Program.logIt("Test Starts.");
            test_similarity();
            //test_ppm();
            //test_ocr();
            //test_text();
            //train_svm();
            //test_svm();
            //img.ToImage<Bgra, byte>().Rotate(-2.5, new Bgra(0, 0, 0, 255), false).Save("temp_1.jpg");
            //camera_init();
            //test_blue_color();
            //search_color();
            //Tuple<bool, Rectangle>  r= extra_blue_block_in_settings(new Bitmap(@"C:\test\setting_01.jpg"));
            //getMSSIM();
            //test_2();
            //test_haar_face();
            //ui_test();
            //test3();
            //test_match();
            //test_ml();
            //test_ml_SMO();
            //extra_icon_from_home_screen_v2();
            //var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            //System.Collections.Generic.Dictionary<string, object> d = jss.Deserialize<System.Collections.Generic.Dictionary<string, object>>(System.IO.File.ReadAllText("HomeScreenLayout.json"));
            //test_base64();
            //capture_screen();
        }
        static void capture_screen()
        {
            System.Threading.Thread.Sleep(5000);
            Bitmap memoryImage = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics g = Graphics.FromImage(memoryImage);
            g.CopyFromScreen(0, 0, 0, 0, memoryImage.Size);
            memoryImage.Save("temp_1.jpg");
        }
        static void test_text()
        {
            string s = @"test.json";
            if (System.IO.File.Exists(s))
            {
                var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                var obj = jss.Deserialize<System.Collections.Generic.Dictionary<string, object>>(System.IO.File.ReadAllText(s));
                ArrayList a = (ArrayList)obj["mobileQ"];
                var b =(Dictionary<string,object>) a[0];
                b = (Dictionary<string, object>)a[1];
                if (b.ContainsKey("timestamp"))
                {
                    var i = b["timestamp"];
                }
                if (b.ContainsKey("check"))
                {
                    var d = (ArrayList)b["check"];
                    int[] dd = d.OfType<int>().ToArray();
                    byte[] bytes = dd.Select(x => (byte)x).ToArray();
                    //Byte[] dd = (Byte[])d.ToArray(typeof(Byte));
                    System.IO.File.WriteAllBytes("temp_1.jpg", bytes);
                }
            }
        }
        static void test_base64()
        {
            var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            System.Collections.Generic.Dictionary<string, object> dict = jss.Deserialize<System.Collections.Generic.Dictionary<string, object>>(System.IO.File.ReadAllText(@"C:\projects\local\radi\iosTester-master-580aa36cb4c56ef07a08e839b0c077671a5fa634\scripts\data.json"));
            var data = System.Convert.FromBase64String(dict["recording"] as string);
            System.IO.File.WriteAllBytes("test.wav", data);
        }
        public static Tuple<bool, Rectangle, Bitmap> extrac_blue_block(Bitmap src, Size minS = default(Size))
        {
            bool retB = false;
            Rectangle retR = Rectangle.Empty;
            Bitmap retImg = null;
            Image<Bgr, Byte> img = new Image<Bgr, byte>(src);
            Mat img1 = new Mat();
            CvInvoke.GaussianBlur(img, img1, new Size(11, 11), 0);
            img1.Save("temp_1.jpg");

            Bgr c1 = new Bgr(160, 50, 10); //new Bgr(160, 50, 30);
            Bgr c2 = new Bgr(250, 220, 100); //new Bgr(250, 200, 100);
            Image<Gray, Byte> g1 = (img1.ToImage<Bgr, Byte>()).InRange(c1, c2);
            g1.Save("temp_2.jpg");
            Rectangle maxR = new Rectangle(0, 0, minS.Width, minS.Height);
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.FindContours(g1, contours, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
                int count = contours.Size;
                for (int i = 0; i < count; i++)
                {
                    Rectangle r = CvInvoke.BoundingRectangle(contours[i]);
                    if (r.Height * r.Width > maxR.Height * maxR.Width)
                    {
                        maxR = r;
                        retB = true;
                    }
                }
            }
            if (retB)
            {
                retB = true;
                retR = maxR;
                retImg = img.GetSubRect(retR).Bitmap;
            }
            return new Tuple<bool, Rectangle, Bitmap>(retB, retR, retImg);
        }
        static void camera_init()
        {
            Mat img = CvInvoke.Imread(@"C:\logs\pictures\save_10.jpg", ImreadModes.AnyColor);
            Tuple< bool, Rectangle, Bitmap> res = ImUtility.extrac_blue_block(img.Bitmap);
            if (res.Item3 != null && res.Item1)
            {
                res.Item3.Save("temp_1.jpg");
            }

            Tuple<bool, Rectangle> r = ImUtility.extra_blue_block_in_settings(img.Bitmap);
            if (r.Item1 && !r.Item2.IsEmpty)
            {
                Bitmap b=ImUtility.crop_image(img.Bitmap, r.Item2);
                b.Save("temp_2.jpg");
            }

        }
        static void test3()
        {
            Bitmap b1 = new Bitmap(@"C:\logs\pictures\save_01.jpg");
            Bitmap b2 = new Bitmap(@"C:\logs\pictures\save_06.jpg");
            Rectangle r = ImUtility.detect_blue_rectangle(b1, b2);
            Bitmap m = ImUtility.crop_image(b2, r);
            m.Save("temp_1.jpg");
            //ImUtility.crop_image(m, r).Save("temp_2.jpg");
            if (m != null)
            {
                Image<Gray, Byte> bb1 = new Image<Gray, byte>(m);
                Mat tmp = new Mat();
                CvInvoke.Threshold(bb1, tmp, 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
                tmp.Save("temp_1.jpg");
                Rectangle ret = Rectangle.Empty;
                using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                {
                    CvInvoke.FindContours(tmp, contours, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
                    int count = contours.Size;
                    double mx = 0.0;
                    for (int i = 0; i < count; i++)
                    {
                        double d = CvInvoke.ContourArea(contours[i]);
                        if (d > mx)
                        {
                            mx = d;
                            ret = CvInvoke.BoundingRectangle(contours[i]);
                            Program.logIt(string.Format("{0}: {1}", d, ret));
                        }
                    }
                }
                Image<Bgr, Byte> b = new Image<Bgr, byte>(m);
                b.GetSubRect(ret).Save("temp_2.jpg");
            }
        }
        static void test_svm()
        {
            FileStorage fs = new FileStorage("test.yaml", FileStorage.Mode.Read);
            FileNode n = fs["voca"];
            Mat voca = new Mat();
            n.ReadMat(voca);

            SURF surf = new SURF(400);
            BFMatcher matcher = new BFMatcher(DistanceType.L2);
            BOWImgDescriptorExtractor bowDex = new BOWImgDescriptorExtractor(surf, matcher);
            bowDex.SetVocabulary(voca);

            SVM svm = new SVM();
            //FileStorage fs1 = new FileStorage("svm.yaml", FileStorage.Mode.Read);
            svm.Read(fs.GetRoot());

            foreach (string s in System.IO.Directory.GetFiles(@"C:\projects\local\testMQ\testMQ\bin\Debug\icons"))
            {
                Image<Bgr, Byte> test_img = new Image<Bgr, byte>(s);
                //Image<Bgr, Byte> test_img = new Image<Bgr, byte>(@"C:\projects\local\testMQ\testMQ\bin\Debug\mail_samples\email_icon_t.jpg");
                //Image<Bgr, Byte> test_img = new Image<Bgr, byte>(@"C:\projects\local\testMQ\testMQ\bin\Debug\phone_icons\icon_2.jpg");
                //Image<Bgr, Byte> test_img = new Image<Bgr, byte>(@"C:\test\35928233-email-icon-on-blue-background-clean-vector.jpg");
                Mat ii = new Mat();
                CvInvoke.CvtColor(test_img, ii, ColorConversion.Bgr2Gray);
                MKeyPoint[] kp = surf.Detect(ii);
                Mat desc = new Mat();
                bowDex.Compute(ii, new VectorOfKeyPoint(kp), desc);
                float r = svm.Predict(desc);
                Program.logIt(string.Format("{0}={1}", s, r));
            }

        }
        static void train_svm()
        {
            int n_samples = 0;
            SURF surf = new SURF(400);
            List<Bitmap> samples = new List<Bitmap>();
            List<Tuple<Bitmap, int>> data = new List<Tuple<Bitmap, int>>();
            /*
            foreach (string s in System.IO.Directory.GetFiles("mail_samples"))
            {
                Bitmap f1 = new Bitmap(s);//ImageDecoder.DecodeFromFile(s);
                data.Add(new Tuple<Bitmap, int>(f1, +1));
            }
            foreach (string s in System.IO.Directory.GetFiles("phone_icons"))
            {
                Bitmap f1 = new Bitmap(s); //ImageDecoder.DecodeFromFile(s);
                data.Add(new Tuple<Bitmap, int>(f1, -1));
            }
            */
            foreach (string s in System.IO.Directory.GetFiles(@"C:\test\iphone_icon"))
            {
                Bitmap f1 = new Bitmap(s);//ImageDecoder.DecodeFromFile(s);
                if (string.Compare(System.IO.Path.GetFileNameWithoutExtension(s), "temp_1") == 0 ||
                    string.Compare(System.IO.Path.GetFileNameWithoutExtension(s), "scoll_selected_icon") == 0
                    )
                    data.Add(new Tuple<Bitmap, int>(f1, +1));
                else
                    data.Add(new Tuple<Bitmap, int>(f1, 0));
            }

            n_samples = data.Count;

            // computr bow 
            Mat m = new Mat();
            foreach (Tuple<Bitmap, int> v in data)
            {
                Image<Bgr, Byte> i = new Image<Bgr, byte>(v.Item1);
                Mat ii = new Mat();
                CvInvoke.CvtColor(i, ii, ColorConversion.Bgr2Gray);
                MKeyPoint[] kp = surf.Detect(ii);
                Mat desc = new Mat();
                surf.Compute(ii, new VectorOfKeyPoint(kp), desc);
                m.PushBack(desc);
            }
            // Create the vocabulary with KMeans.
            MCvTermCriteria tc = new MCvTermCriteria(100, 0.00001);
            BOWKMeansTrainer bowTrainer = new BOWKMeansTrainer(16, tc, 3, KMeansInitType.PPCenters);
            bowTrainer.Add(m);
            Mat voca = new Mat();
            bowTrainer.Cluster(voca);
            //
            BFMatcher matcher = new BFMatcher(DistanceType.L2);
            BOWImgDescriptorExtractor bowDex = new BOWImgDescriptorExtractor(surf, matcher);
            bowDex.SetVocabulary(voca);

            // 
            Mat tDesc = new Mat();
            //Matrix<int> tLabel = new Matrix<int>(1, n_samples);
            Matrix<int> tLabel = new Matrix<int>(n_samples, 1);
            //foreach (Tuple<Bitmap, int> v in data)
            for (int j = 0; j < data.Count; j++)
            {
                Image<Bgr, Byte> i = new Image<Bgr, byte>(data[j].Item1);
                Mat ii = new Mat();
                CvInvoke.CvtColor(i, ii, ColorConversion.Bgr2Gray);
                MKeyPoint[] kp = surf.Detect(ii);
                Mat desc = new Mat();
                bowDex.Compute(ii, new VectorOfKeyPoint(kp), desc);
                tDesc.PushBack(desc);
                //tLabel[0, j] = data[j].Item2;
                tLabel[j, 0] = data[j].Item2;
            }
            //
            //SVM model = new SVM();
            //model.SetKernel(Emgu.CV.ML.SVM.SvmKernelType.Linear);
            //model.Type = SVM.SvmType.CSvc;
            //model.C = 1;
            //model.TermCriteria = new MCvTermCriteria(100, 0.00001);

            SVM svm = new SVM();
            svm.C = 312.5;
            svm.Gamma = 0.50625000000000009;
            svm.SetKernel(SVM.SvmKernelType.Rbf);
            svm.Type = SVM.SvmType.CSvc;
            svm.Nu = 0.5;

            TrainData td = new TrainData(tDesc, Emgu.CV.ML.MlEnum.DataLayoutType.RowSample, tLabel);
            bool tained = svm.TrainAuto(td);

            using (FileStorage fs = new FileStorage("voca.yaml", FileStorage.Mode.Write))
            {
                svm.Write(fs);
                fs.Write(voca, "voca");
            }
            //using (FileStorage fs = new FileStorage("svm.yaml", FileStorage.Mode.Write))
            //{
            //    svm.Write(fs);
            //}
            //svm.Save("svm.yaml");
            // test
            {
                //Image<Bgr, Byte> test_img = new Image<Bgr, byte>(@"C:\test\scroll_left.jpg");
                Image<Bgr, Byte> test_img = new Image<Bgr, byte>(@"C:\test\iphone_icon\temp_1.jpg");
                //Image<Bgr, Byte> test_img = new Image<Bgr, byte>(@"C:\projects\local\testMQ\testMQ\bin\Debug\phone_icons\icon_2.jpg");
                //Image<Bgr, Byte> test_img = new Image<Bgr, byte>(@"C:\test\35928233-email-icon-on-blue-background-clean-vector.jpg");
                Mat ii = new Mat();
                CvInvoke.CvtColor(test_img, ii, ColorConversion.Bgr2Gray);
                MKeyPoint[] kp = surf.Detect(ii);
                Mat desc = new Mat();
                bowDex.Compute(ii, new VectorOfKeyPoint(kp), desc);
                float r = svm.Predict(desc);

            }
        }

        static void test_1()
        {
            Bitmap app = new Bitmap(@"C:\logs\pictures\save_11.jpg");
            Bitmap app1 = new Bitmap(@"C:\logs\pictures\save_12.jpg");
            
            Tuple<bool, Rectangle, Bitmap> menu = ImUtility.extrac_context_menu(app, app1);
            if(menu.Item1 && menu.Item3 != null)
            {
                menu.Item3.Save("temp_1.jpg");
            }
            Image<Bgr, byte> am = new Image<Bgr, byte>(menu.Item3);
            // 
            Mat t = CvInvoke.Imread(@"C:\logs\pictures\ios_icons\scroll.jpg");
            Mat m = new Mat();
            CvInvoke.MatchTemplate(am, t, m, TemplateMatchingType.CcoeffNormed);
            double max = 0;
            double min = 0;
            Point maxP = new Point();
            Point minP = new Point();
            CvInvoke.MinMaxLoc(m, ref min, ref max, ref minP, ref maxP);
            Program.logIt(string.Format("{0}: {1}", max, new Rectangle(maxP, t.Size)));
        }
        public static void ui_test(Image<Bgra, Byte> src, System.Collections.Generic.Dictionary<string, object> locations)
        {
            CascadeClassifier haar_email = new CascadeClassifier(@"trained\settings_cascade.xml");
            Rectangle[] rects = haar_email.DetectMultiScale(src);
            Program.logIt(string.Format("Found: {0}", rects.Length));
            foreach (var v in rects)
            {
                System.Drawing.Point p = new System.Drawing.Point(v.X + v.Width / 2, v.Y + v.Height / 2);
                foreach (KeyValuePair<string, object> kv in locations)
                {
                    if (((Rectangle)kv.Value).Contains(p))
                    {
                        Program.logIt(String.Format("Found: in {0} and {1}", kv.Value, kv.Key));
                    }
                }
            }
        }
        static Tuple<bool, Rectangle> extra_blue_block_in_settings(Bitmap src)
        {
            bool retB = false;
            Rectangle retR = Rectangle.Empty;
            Image<Bgr, Byte> b1 = new Image<Bgr, Byte>(src);
            Mat img1 = new Mat();
            CvInvoke.GaussianBlur(b1, img1, new Size(7, 7), 0);
            //img1.Save("temp_1.jpg");
            Bgr c1 = new Bgr(160, 150, 70);
            Bgr c2 = new Bgr(240, 230, 150);
            Image<Gray, Byte> g1 = (img1.ToImage<Bgr, Byte>()).InRange(c1, c2);
            //g1.Save("temp_2.jpg");
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.FindContours(g1, contours, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
                int count = contours.Size;
                for (int i = 0; i < count; i++)
                {
                    VectorOfPoint contour = contours[i];
                    VectorOfPoint approxContour = new VectorOfPoint();
                    CvInvoke.ApproxPolyDP(contour, approxContour, CvInvoke.ArcLength(contour, true) * 0.05, true);
                    if (approxContour.Size == 4)
                    {
                        if (CvInvoke.ContourArea(approxContour, false) > 250)
                        {
                            Rectangle r = CvInvoke.BoundingRectangle(approxContour);
                            Rectangle r1 = CvInvoke.BoundingRectangle(contour);
                            //Program.logIt(string.Format("{0}", r));
                            //Program.logIt(string.Format("{0}", r1));
                            retR = r1;
                            retB = true;
                        }
                    }
                }
            }
            return new Tuple<bool, Rectangle>(retB, retR);
        }
        static void search_color()
        {
            Image<Bgr, Byte> img = new Image<Bgr, byte>(new Bitmap(@"C:\test\save_00.jpg"));
            //Image<Gray, Byte> mask = new Image<Gray, byte>(img.Size);
            //Bgr v;
            //for (int i = 0; i < img.Height; i++)
            //{
            //    for (int j = 0; j < img.Width; j++)
            //    {
            //        v = img[i, j];
            //        mask[i, j] = new Gray(v.Blue);
            //    }
            //}
            //mask.Save("temp_1.jpg");
            Bgr c1 = new Bgr(150, 100, 50);
            Bgr c2 = new Bgr(250, 200, 100);
            Image<Gray, Byte> g1 = img.InRange(c1, c2);
            g1.Save("temp_2.jpg");
        }
        static void test_blue_color()
        {
            Mat img = CvInvoke.Imread(@"C:\test\save_00.jpg", ImreadModes.AnyColor);
            Mat img1 = new Mat();
            CvInvoke.GaussianBlur(img, img1, new Size(11, 11), 0);
            img1.Save("temp_1.jpg");
            Bgr c1 = new Bgr(100, 50, 0); //new Bgr(160, 50, 30);
            Bgr c2 = new Bgr(250, 220, 100); //new Bgr(250, 200, 100);
            Bgr c3 = new Bgr(80, 20, 10); //new Bgr(160, 50, 30);
            Bgr c4 = new Bgr(160, 100, 60); //new Bgr(250, 200, 100);
            Image<Gray, Byte> g1 = (img1.ToImage<Bgr, Byte>()).InRange(c3, c4);
            g1.Save("temp_2.jpg");
            Rectangle maxR = new Rectangle(0, 0, 60, 60);
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.FindContours(g1, contours, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
                int count = contours.Size;
                for (int i = 0; i < count; i++)
                {
                    Rectangle r = CvInvoke.BoundingRectangle(contours[i]);
                    if (r.Height * r.Width > maxR.Height * maxR.Width)
                    {
                        maxR = r;
                    }
                }
            }
            Program.logIt(string.Format("{0}", maxR));
            if (maxR.X > 0 && maxR.Y > 0)
                g1.GetSubRect(maxR).Save("temp_3.jpg");
        }
        static void test_match()
        {
            Bitmap b1 = new Bitmap(@"C:\test\test_1\temp_menu.jpg");
            Bitmap sl = new Bitmap(@"C:\test\scroll_left.jpg");
            Image<Gray, Byte> slicon = new Image<Gray, byte>(sl);
            slicon = slicon.Not();
            slicon.Save("temp_1.jpg");
            Image<Gray, Byte> test = new Image<Gray, Byte>(b1);
            //long l;
            //Mat r = DrawMatches.Draw(slicon.Mat, test.Mat, out l);
            //r.Save("temp_2.jpg");

            //SURF surfCPU = new SURF(400);
            //Brisk surfCPU = new Brisk();
            SIFT surfCPU = new SIFT();
            VectorOfKeyPoint modelKeyPoints = new VectorOfKeyPoint();
            VectorOfKeyPoint observedKeyPoints = new VectorOfKeyPoint();
            UMat modelDescriptors = new UMat();
            UMat observedDescriptors = new UMat();
            surfCPU.DetectAndCompute(slicon, null, modelKeyPoints, modelDescriptors, false);
            surfCPU.DetectAndCompute(test, null, observedKeyPoints, observedDescriptors, false);

            var indices = new Matrix<int>(observedDescriptors.Rows, 2);
            var dists = new Matrix<float>(observedDescriptors.Rows, 2);
            var flannIndex = new Index(modelDescriptors, new KMeansIndexParams());
            flannIndex.KnnSearch(observedDescriptors, indices, dists, 2);
            for (int i = 0; i < indices.Rows; i++)
            {
                if (dists.Data[i, 0] < (0.6 * dists.Data[i, 1]))
                {
                    int idx1 = indices[i, 0];
                    int idx2 = indices[i, 1];
                    Program.logIt(string.Format("{0}-{1}", indices[i, 0], indices[i, 1]));
                    MKeyPoint p1 = modelKeyPoints[idx1];
                    MKeyPoint p2 = observedKeyPoints[idx2];
                    Program.logIt(string.Format("{0}-{1}", p1.Point, p2.Point));
                }
            }
        }
        static void test_ocr()
        {
            Bitmap b = new Bitmap(@"temp_menu.jpg");
            string s = ocrEng.test(b);
            var doc = new HtmlAgilityPack.HtmlDocument();
            try
            {
                doc.LoadHtml(s);
                if (doc.ParseErrors.Count() == 0)
                {
                    HtmlNode root = doc.DocumentNode;
                    var ocr_line = doc.DocumentNode.SelectNodes("//span[@class='ocr_line']");
                    foreach(var l in ocr_line)
                    {
                        Program.logIt(l.InnerText);
                    }
                }
            }
            catch (Exception) { }
        }
        public static Bitmap ReadBitmapFromPPM(string file)
        {
            var reader = new BinaryReader(new FileStream(file, FileMode.Open));
            if (reader.ReadChar() != 'P' || reader.ReadChar() != '6')
                return null;
            reader.ReadChar(); //Eat newline
            string widths = "", heights = "";
            char temp;
            while ((temp = reader.ReadChar()) != ' ')
                widths += temp;
            while ((temp = reader.ReadChar()) >= '0' && temp <= '9')
                heights += temp;
            if (reader.ReadChar() != '2' || reader.ReadChar() != '5' || reader.ReadChar() != '5')
                return null;
            reader.ReadChar(); //Eat the last newline
            int width = int.Parse(widths),
                height = int.Parse(heights);
            Bitmap bitmap = new Bitmap(width, height);
            //Read in the pixels
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    bitmap.SetPixel(x, y, Color.FromArgb(reader.ReadByte(), reader.ReadByte(), reader.ReadByte()));
                    //bitmap.SetPixel(x, y, new Bitmap.Color()
                    //{
                    //    Red = reader.ReadByte(),
                    //    Green = reader.ReadByte(),
                    //    Blue = reader.ReadByte()
                    //});
            return bitmap;
        }
        static void test_ppm()
        {
            //string folder = @"C:\projects\local\ca3\ca3";
            //foreach(string fn in System.IO.Directory.GetFiles(folder, "*.ppm"))
            //{
            //    ReadBitmapFromPPM(fn).Save("temp_1.jpg");
            //}
            //ReadBitmapFromPPM(@"C:\projects\svn\FDDev\GreenT Enterprise\Tools\AirPlay\AirplaySdkExample\Debug\screen\frame_0.ppm").Save("temp_1.bmp");
            byte[] data = System.IO.File.ReadAllBytes(@"C:\projects\svn\FDDev\GreenT Enterprise\Tools\AirPlay\release\screen\label_1.ppm");
            Bitmap b = ImUtility.fromPPM(data);
            b.Save("temp_1.jpg");
            //Bitmap b1 = new Bitmap(@"C:\projects\svn\FDDev\GreenT Enterprise\Tools\AirPlay\release\screen\label_1.ppm");
        }
        static void test_similarity()
        {
            Bitmap b1 = new Bitmap("temp_1.jpg");
            Bitmap t = new Bitmap(@"C:\projects\local\github\testMQ\testMQ\scripts\get_imei\xhahnbyc.jpg");
            Rectangle r = new Rectangle(33, 908, 102, 102);
            Bitmap b10 = ImUtility.crop_image(b1, r);
            Bitmap b20 = ImUtility.crop_image(t, r);

            Image<Gray, Byte> b11 = new Image<Gray, byte>(b10);
            Image<Gray, Byte> t11 = new Image<Gray, byte>(b20);
            Mat m = new Mat();
            CvInvoke.Threshold(b11, m, 0, 255, ThresholdType.Otsu | ThresholdType.Binary);
            b1 = m.Bitmap;
            CvInvoke.Threshold(t11, m, 0, 255, ThresholdType.Otsu | ThresholdType.Binary);
            t = m.Bitmap;
            //var res = ImUtility.multiscale_matchtemplate(b1, t);
            ImUtility.is_same_image(b1, t);

            //b11.Save("temp_2.jpg");
            //b21.Save("temp_3.jpg");
            //ImUtility.is_same_image(b11, b21);
        }
        public static Tuple<double, Point, Rectangle> multiscale_matchtemplate(Bitmap src, Bitmap template, double threshold = 0.95)
        {
            double retD = 0.0;
            Point retP = Point.Empty;
            Rectangle retR = Rectangle.Empty;
            if (src != null && template != null && src.Width > template.Width && src.Height > template.Height)
            {
                Image<Bgr, Byte> img1 = new Image<Bgr, Byte>(src);
                Image<Bgr, Byte> it = new Image<Bgr, Byte>(template);
                Mat resized = new Mat();
                Mat res = new Mat();
                for (double d = 0.95; d < 1.05 && retD<threshold; d = d + 0.003)
                {
                    CvInvoke.Resize(it, resized, new Size(0, 0), d, d);
                    CvInvoke.MatchTemplate(img1, resized, res, TemplateMatchingType.CcoeffNormed);
                    double min = 0;
                    double max = 0;
                    Point minP = new Point();
                    Point maxP = new Point();
                    CvInvoke.MinMaxLoc(res, ref min, ref max, ref minP, ref maxP);
                    if (max > threshold)
                    {
                        retD = max;
                        retP = maxP;
                        retR = new Rectangle(retP, resized.Size);
                        Program.logIt(string.Format("s={0}, r{1}", retD, d));
                    }
                }
            }
            return new Tuple<double, Point, Rectangle>(retD, retP, retR);
        }
        static void test_2()
        {
            //Bitmap b1 = new Bitmap(@"C:\logs\pictures\save_09.jpg");
            //Bitmap b2 = new Bitmap(@"C:\logs\pictures\save_10.jpg");
            //// remove fringe 30 pixel
            //Bitmap b11 = ImUtility.crop_image(b1, new Rectangle(0, 30, b1.Width, b1.Height - 30));
            //Bitmap b21 = ImUtility.crop_image(b2, new Rectangle(0, 30, b2.Width, b2.Height - 30));
            //Tuple<bool, Rectangle, Bitmap> menu = extrac_context_menu(b11, b21);
            //if(menu.Item1 && menu.Item3 != null)
            //{
            //    menu.Item3.Save("temp_1.jpg");
            //}
        }
    }
}
