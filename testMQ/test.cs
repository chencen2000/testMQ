using Accord;
using Accord.Controls;
using Accord.Imaging;
using Accord.Imaging.Filters;
using Accord.Imaging.Formats;
using Accord.IO;
using Accord.MachineLearning;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Math.Geometry;
using Accord.Math;
using Accord.Math.Optimization.Losses;
using Accord.Statistics.Kernels;
using Accord.Vision.Detection;
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
            //test_text();
            //train_svm();
            //test_svm();
            //img.ToImage<Bgra, byte>().Rotate(-2.5, new Bgra(0, 0, 0, 255), false).Save("temp_1.jpg");
            //camera_init();
            //test_blue_color();
            //search_color();
            //Tuple<bool, Rectangle>  r= extra_blue_block_in_settings(new Bitmap(@"C:\test\setting_01.jpg"));
            //getMSSIM();
            test_1();
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
        }
        static void test_text()
        {
            Mat img = CvInvoke.Imread(@"C:\test\menu_item.jpg", ImreadModes.Grayscale);
            Mat channels = new Mat();
            //Emgu.CV.Text.TextInvoke.ComputeNMChannels(img, channels);
            ERFilterNM1 f1 = new ERFilterNM1(@"C:\Users\qa\PycharmProjects\test\data\trained_classifierNM1.xml");
            VectorOfERStat regions = new VectorOfERStat();
            f1.Run(img, regions);
            ERFilterNM2 f2 = new ERFilterNM2(@"C:\Users\qa\PycharmProjects\test\data\trained_classifierNM2.xml");
            f2.Run(img, regions);
            int n = regions.Size;
            for(int i = 0; i < n; i++)
            {
                MCvERStat ers = regions[i];
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
            Mat img = CvInvoke.Imread(@"C:\test\save_01.jpg", ImreadModes.AnyColor);
            Tuple< bool, Rectangle, Bitmap> res = ImUtility.extrac_blue_block(img.Bitmap);
            if (res.Item3 != null && res.Item1)
            {
                res.Item3.Save("temp_1.jpg");
            }

        }
        static void test3()
        {
            Bitmap b1 = new Bitmap(@"C:\test\test_1\temp_1.jpg");
            Bitmap b2 = new Bitmap(@"C:\test\test_1\temp_2.jpg");
            Bitmap sl = new Bitmap(@"C:\test\scroll_left.jpg");
            Image<Gray, Byte> slicon = new Image<Gray, byte>(sl);
            slicon = slicon.Not();
            slicon.Save("temp_1.jpg");
            Tuple<bool, Rectangle, Bitmap> cm = ImUtility.extrac_context_menu(b1, b2);
            if (cm.Item1 && cm.Item3 != null)
            {
                cm.Item3.Save("temp_1.jpg");
                Image<Gray, Byte> uimage = new Image<Gray, Byte>(cm.Item3);
                UMat pyrDown = new UMat();
                //CvInvoke.PyrDown(uimage, pyrDown);
                //CvInvoke.PyrUp(pyrDown, uimage);
                CvInvoke.Threshold(uimage, pyrDown, 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
                uimage = pyrDown.ToImage<Gray, Byte>();
                uimage.Save("temp_1.jpg");
                double cannyThreshold = 180.0;
                double cannyThresholdLinking = 120.0;
                UMat cannyEdges = new UMat();
                CvInvoke.Canny(uimage, cannyEdges, cannyThreshold, cannyThresholdLinking);
                cannyEdges.Save("temp_1.jpg");
                using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                {
                    CvInvoke.FindContours(cannyEdges, contours, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
                    int count = contours.Size;
                    for (int i = 0; i < count; i++)
                    {
                        VectorOfPoint contour = contours[i];
                        VectorOfPoint approxContour = new VectorOfPoint();
                        double al = CvInvoke.ArcLength(contour, true);
                        CvInvoke.ApproxPolyDP(contour, approxContour, al * 0.05, true);
                        if (approxContour.Size == 4)
                        {
                            double a1 = CvInvoke.ContourArea(contours[i], false);
                            if (a1 > 100.0)
                            {
                                Rectangle r = CvInvoke.BoundingRectangle(contours[i]);
                                Program.logIt(String.Format("{0}", r));
                                using (Graphics g = Graphics.FromImage(cm.Item3))
                                {
                                    g.DrawRectangle(new Pen(Color.White), r);
                                }
                                if (r.Width == r.Height)
                                {
                                    uimage.GetSubRect(r).Save(string.Format("temp_{0}.jpg", i));
                                }
                                //Mat m = new Mat();
                                //CvInvoke.MatchTemplate(slicon, uimage.GetSubRect(r), m, TemplateMatchingType.CcorrNormed);
                                //double min=0;
                                //double max = 0;
                                //System.Drawing.Point minp = new System.Drawing.Point();
                                //System.Drawing.Point maxp = new System.Drawing.Point();
                                //CvInvoke.MinMaxLoc(m, ref min, ref max, ref minp, ref maxp);
                            }
                        }
                    }
                }
                cm.Item3.Save("temp_2.jpg");
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
        static void extra_icon_from_home_screen_v2()
        {
            Bitmap f1 = ImageDecoder.DecodeFromFile(@"C:\test\save_00.jpg");
            int top_margin = (int)(0.026 * f1.Height); // 26;
            int left_margin = (int)(0.05 * f1.Width); //30;
            int middle_margin = (int)(0.05 * f1.Height); //50;
            int botton_line = (int)(0.13 * f1.Height); //130;
            int total_col = 4;
            int total_row = 6;
            int w = (f1.Width - left_margin - left_margin) / total_col;
            int h = (f1.Height - top_margin - botton_line - middle_margin) / total_row;
            for (int row = 1; row <= 6; row++)
            {
                int x = left_margin;
                int y = top_margin + h * (row - 1);
                for (int col = 1; col <= 4; col++)
                {
                    x = 28 + w * (col - 1);
                    Rectangle r = new Rectangle(x, y, 140, 140);
                    Crop c = new Crop(r);
                    Bitmap b = c.Apply(f1);
                    b.Save(string.Format("icons\\temp_{0}x{1}.jpg", row, col));
                }
            }
            // for dock
            for (int col = 1; col <= 4; col++)
            {
                int x = left_margin + w * (col - 1);
                int y = f1.Height - botton_line;
                Rectangle r = new Rectangle(x, y, w, botton_line);
                Crop c = new Crop(r);
                Bitmap b = c.Apply(f1);
                b.Save(string.Format("icons\\temp_0x{0}.jpg", col));
            }
        }
        static void extra_icon_from_home_screen()
        {
            Bitmap f1 = ImageDecoder.DecodeFromFile(@"C:\test\save_12.jpg");
            int row = 1;
            int col = 1;
            int x = 28;
            int y = 18;
            int xstep = 140;
            int ystep = 145;
            int ybottom = 930;
            for (row = 1; row <= 6; row++)
            {
                x = 28;
                y = 18 + ystep * (row - 1);
                for (col = 1; col <= 4; col++)
                {
                    x = 28 + xstep * (col - 1);
                    Rectangle r = new Rectangle(x, y, 140, 140);
                    Crop c = new Crop(r);
                    Bitmap b = c.Apply(f1);
                    b.Save(string.Format("icons\\temp_{0}x{1}.jpg", row, col));
                }
            }
            // for dock
            for (col = 1; col <= 4; col++)
            {
                x = 28 + xstep * (col - 1);
                Rectangle r = new Rectangle(x, ybottom, 140, 140);
                Crop c = new Crop(r);
                Bitmap b = c.Apply(f1);
                b.Save(string.Format("icons\\temp_0x{0}.jpg", col));
            }
        }
        static void test_2()
        {
            Bitmap f1 = ImageDecoder.DecodeFromFile(@"C:\Users\qa\Desktop\picture\save_10.jpg");
            Program.logIt(string.Format("w={0}, h={0}", f1.Width, f1.Height));
            ImageStatistics stat = new ImageStatistics(f1);
            GaussianBlur gb = new GaussianBlur(2, 10);
            Bitmap src = gb.Apply(f1);
            src.Save("temp_2.jpg");
            ColorFiltering cf = new ColorFiltering();
            cf.Red = new IntRange(60, 130);
            cf.Green = new IntRange(90, 200);
            cf.Blue = new IntRange(150, 230);
            Bitmap img = cf.Apply(src);
            img.Save("temp_1.jpg");
            Bitmap gs = Grayscale.CommonAlgorithms.BT709.Apply(img);
            gs.Save("temp_2.jpg");
            BlobCounter bc = new BlobCounter();
            bc.BlobsFilter = null;
            bc.MinHeight = 80;
            bc.MinWidth = 80;
            bc.CoupledSizeFiltering = true;
            bc.FilterBlobs = true;
            bc.ObjectsOrder = ObjectsOrder.Area;
            bc.ProcessImage(gs);
            Blob[] blobs = bc.GetObjectsInformation();
            Blob roi = blobs[0];
            List<IntPoint> ps = bc.GetBlobsEdgePoints(roi);
            List<IntPoint> corners;
            SimpleShapeChecker shapeChecker = new SimpleShapeChecker();
            bool b = shapeChecker.IsQuadrilateral(ps, out corners);
            var t = shapeChecker.CheckShapeType(ps);
            var t1 = shapeChecker.CheckPolygonSubType(corners);
            using (Graphics g = Graphics.FromImage(f1))
            {
                g.DrawRectangle(new Pen(Color.Yellow), roi.Rectangle);
                List<System.Drawing.Point> sp = new List<System.Drawing.Point>();
                foreach (var p in corners)
                {
                    sp.Add(new System.Drawing.Point(p.X, p.Y));
                }
                g.DrawLines(new Pen(Color.Red), sp.ToArray());
            }
            f1.Save("temp_3.jpg");
            //int margin = 10;
            //int w = (f1.Width - 2 * margin) / 4;
            //for (int i = 0; i < 4; i++)
            //{
            //    Rectangle r = new Rectangle(margin + w * i, 0, w+margin, f1.Height);
            //    Crop c = new Crop(r);
            //    Bitmap b = c.Apply(f1);
            //    b.Save(string.Format("temp_{0}.jpg", i + 1));
            //}


        }
        static void test_1()
        {
            Mat img1 = CvInvoke.Imread(@"C:\test\pictures\ios11-settings-icon-100741874-large.jpg");
            Mat img2 = CvInvoke.Imread(@"C:\test\save_01.jpg");
            long l;
            Mat m = DrawMatches.Draw(img1, img2, out l);
            m.Save("temp_1.jpg");

            // haar test
            CascadeClassifier haar_email = new CascadeClassifier(@"trained\settings_cascade.xml");
            Rectangle[] ret = haar_email.DetectMultiScale(img2);
            foreach (Rectangle r in ret)
                Program.logIt(string.Format("{0}", r));
        }
        static void test_ml_SMO()
        {
            double[][] features = new double[][]
            {
                                new double[] { 1.73407600308642,0,5.62588471221633},
                                new double[] { 53.6045780285494,8,79.2103241645548 },
                                new double[] {  53.1015615354938,7,79.3755488866987 },
                                new double[] {  53.2143330439815,7,79.3365665756378 },
                                new double[]{ 8.12086950231481, 3, 11.8416794313788 },
                                new double[]{ 4.01413675864101, 1, 13.026857481124 },
            };
            int[] labels = { -1, +1, +1, +1, +1, +1 };
            var teacher = new SequentialMinimalOptimization<Linear>()
            {
                Complexity = 10000 // make a hard margin SVM
            };
            var svm = teacher.Learn(features, labels);
            bool[] output = svm.Decide(features);
            double error = new ZeroOneLoss(labels).Loss(output);

            Bitmap f1 = ImageDecoder.DecodeFromFile(@"C:\Users\qa\Desktop\picture\save_10.jpg");
            Bitmap f2 = ImageDecoder.DecodeFromFile(@"C:\Users\qa\Desktop\picture\save_11.jpg");
            Bitmap f3 = ImageDecoder.DecodeFromFile(@"C:\Users\qa\Desktop\picture\save_12.jpg");
            {
                Bitmap g = Grayscale.CommonAlgorithms.BT709.Apply(f1);
                ImageStatistics stat = new ImageStatistics(g);
                double[][] ds = { new double[] { stat.Gray.Mean, stat.Gray.Median, stat.Gray.StdDev } };
                Program.logIt(string.Format("{0},{1},{2}", ds[0][0], ds[0][1], ds[0][2]));
                bool[] o = svm.Decide(ds);
            }
            {
                Bitmap mask = new Bitmap(f2.Width, f2.Height);
                using (Graphics g = Graphics.FromImage(mask))
                {
                    g.FillRectangle(new SolidBrush(Color.Black), new Rectangle(0, 0, mask.Width, mask.Height));
                }
                //Bitmap g = Grayscale.CommonAlgorithms.BT709.Apply(mask);
                ImageStatistics stat = new ImageStatistics(Grayscale.CommonAlgorithms.BT709.Apply(mask));
                double[][] ds = { new double[] { stat.Gray.Mean, stat.Gray.Median, stat.Gray.StdDev } };
                Program.logIt(string.Format("{0},{1},{2}", ds[0][0], ds[0][1], ds[0][2]));
                bool[] o = svm.Decide(ds);
            }
            {
                Subtract s = new Subtract(f1);
                Bitmap r = s.Apply(f2);
                r.Save("temp_1.jpg");
                bool empty = CheckEmptyImageByML.getInstance().isImageEmpty(r);
                Bitmap g = Grayscale.CommonAlgorithms.BT709.Apply(r);
                ImageStatistics stat = new ImageStatistics(g);
                double[][] ds = { new double[] { stat.Gray.Mean, stat.Gray.Median, stat.Gray.StdDev } };
                Program.logIt(string.Format("{0},{1},{2}", ds[0][0], ds[0][1], ds[0][2]));
                bool[] o = svm.Decide(ds);
            }
            {
                Subtract s = new Subtract(f1);
                Bitmap r = s.Apply(f3);
                r.Save("temp_1.jpg");
                bool empty = CheckEmptyImageByML.getInstance().isImageEmpty(r);
                Bitmap g = Grayscale.CommonAlgorithms.BT709.Apply(r);
                ImageStatistics stat = new ImageStatistics(g);
                double[][] ds = { new double[] { stat.Gray.Mean, stat.Gray.Median, stat.Gray.StdDev } };
                Program.logIt(string.Format("{0},{1},{2}", ds[0][0], ds[0][1], ds[0][2]));
                bool[] o = svm.Decide(ds);
            }

        }
        static void test_ml()
        {
            List<double[]> inputList = new List<double[]>();
            // load all samles from "samples" folder,
            //string s = System.Environment.CurrentDirectory;
            foreach (string s in System.IO.Directory.GetFiles("samples"))
            {
                Bitmap b = ImageDecoder.DecodeFromFile(s);
                Bitmap g = Grayscale.CommonAlgorithms.BT709.Apply(b);
                ImageStatistics stat = new ImageStatistics(g);
                double[] ds = new double[] { stat.Gray.Mean, stat.Gray.Median, stat.Gray.StdDev };
                inputList.Add(ds);
                Program.logIt(string.Format("{0},{1},{2}", ds[0], ds[1], ds[2]));
            }
            Accord.Math.Random.Generator.Seed = 0;
            KMeans kmeans = new KMeans(2);
            KMeansClusterCollection clusters = kmeans.Learn(inputList.ToArray());

            // test
            Bitmap f1 = ImageDecoder.DecodeFromFile(@"C:\test\save_00.jpg");
            Bitmap f2 = ImageDecoder.DecodeFromFile(@"C:\test\save_01.jpg");
            {
                Bitmap g = Grayscale.CommonAlgorithms.BT709.Apply(f1);
                ImageStatistics stat = new ImageStatistics(g);
                double[][] ds = { new double[] { stat.Gray.Mean, stat.Gray.Median, stat.Gray.StdDev } };
                Program.logIt(string.Format("{0},{1},{2}", ds[0][0], ds[0][1], ds[0][2]));
                int[] res = clusters.Decide(ds);
            }
            {
                Bitmap g = Grayscale.CommonAlgorithms.BT709.Apply(f2);
                ImageStatistics stat = new ImageStatistics(g);
                double[][] ds = { new double[] { stat.Gray.Mean, stat.Gray.Median, stat.Gray.StdDev } };
                Program.logIt(string.Format("{0},{1},{2}", ds[0][0], ds[0][1], ds[0][2]));
                int[] res = clusters.Decide(ds);
            }
            {
                Bitmap mask = new Bitmap(f2.Width, f2.Height);
                using (Graphics g = Graphics.FromImage(mask))
                {
                    g.FillRectangle(new SolidBrush(Color.Black), new Rectangle(0, 0, mask.Width, mask.Height));
                }
                //Bitmap g = Grayscale.CommonAlgorithms.BT709.Apply(mask);
                ImageStatistics stat = new ImageStatistics(Grayscale.CommonAlgorithms.BT709.Apply(mask));
                double[][] ds = { new double[] { stat.Gray.Mean, stat.Gray.Median, stat.Gray.StdDev } };
                Program.logIt(string.Format("{0},{1},{2}", ds[0][0], ds[0][1], ds[0][2]));
                int[] res = clusters.Decide(ds);
            }
            Tuple<bool, float, Bitmap> r = ImUtility.diff_images(f1, f2);
            if (r.Item3 != null)
            {
                Bitmap g = Grayscale.CommonAlgorithms.BT709.Apply(r.Item3);
                ImageStatistics stat = new ImageStatistics(g);
                double[][] ds = { new double[] { stat.Gray.Mean, stat.Gray.Median, stat.Gray.StdDev } };
                Program.logIt(string.Format("{0},{1},{2}", ds[0][0], ds[0][1], ds[0][2]));
                int[] res = clusters.Decide(ds);
            }

            /*
            double[][] observations =
                {
                    new double[] { -5, -2, -1 },
                    new double[] { -5, -5, -6 },
                    new double[] {  2,  1,  1 },
                    new double[] {  1,  1,  2 },
                    new double[] {  1,  2,  2 },
                    new double[] {  3,  1,  2 },
                    new double[] { 11,  5,  4 },
                    new double[] { 15,  5,  6 },
                    new double[] { 10,  5,  6 },
                };
            KMeans kmeans = new KMeans(2);
            KMeansClusterCollection clusters = kmeans.Learn(observations);
            int[] labels = clusters.Decide(observations);
            */

        }
        static void test_haar_face()
        {
            Bitmap img1 = new Bitmap(@"C:\test\save_00.jpg");
            Bitmap img2 = new Bitmap(@"C:\test\iphone_icon\setting_icon.jpg");
            var surf = new SpeededUpRobustFeaturesDetector();
            var points1 = surf.Transform(img1);
            var points2 = surf.Transform(img2);
            var matcher = new KNearestNeighborMatching(4);
            Accord.IntPoint[][] matches = matcher.Match(points1, points2);

            Concatenate concat = new Concatenate(img1);
            Bitmap img3 = concat.Apply(img2);
            PairsMarker pairs = new PairsMarker(
                    matches[0], // Add image1's width to the X points
                                // to show the markings correctly
                    matches[1].Apply(pp => new IntPoint(pp.X + img1.Width, pp.Y)));
            pairs.ApplyInPlace(img3);

            img3.Save("temp_1.jpg");

            List<double[]> data = new List<double[]>();
            foreach (var v in matches[0])
            {
                data.Add(new double[] { v.X, v.Y });
            }

            KMeans kmeans = new KMeans(k: 4);
            var clusters = kmeans.Learn(data.ToArray());
            int[] labels = clusters.Decide(data.ToArray());


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
    }
}
