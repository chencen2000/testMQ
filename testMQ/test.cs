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

namespace testMQ
{
    class Test
    {
        static void Main()
        {
            Program.logIt("Test Starts.");
            //test_haar_face();
            //ui_test();
            //test3();
            //test_ml();
            //test_ml_SMO();
            //extra_icon_from_home_screen_v2();
            //var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            //System.Collections.Generic.Dictionary<string, object> d = jss.Deserialize<System.Collections.Generic.Dictionary<string, object>>(System.IO.File.ReadAllText("HomeScreenLayout.json"));
        }
        static void test3()
        {
            Serializer.Load("test_svm.bin", out object load_svm);
            Serializer.Load("test_bow.bin", out object load_bow);
            //List<Bitmap> test = new List<Bitmap>();
            foreach (string s in System.IO.Directory.GetFiles(@"C:\test\iphone_icon"))
            {
                Bitmap b = new Bitmap(s);
                //test.Add(b);
                double[] fs = (load_bow as ITransform<Bitmap, double[]>).Transform(b);
                bool o1 = (load_svm as Accord.MachineLearning.VectorMachines.SupportVectorMachine<Accord.Statistics.Kernels.Linear>).Decide(fs);
                double s1 = (load_svm as Accord.MachineLearning.VectorMachines.SupportVectorMachine<Accord.Statistics.Kernels.Linear>).Score(fs);
                double[] p1 = (load_svm as Accord.MachineLearning.VectorMachines.SupportVectorMachine<Accord.Statistics.Kernels.Linear>).Probabilities(fs);
                Program.logIt(string.Format("{0}: {1} score={2}, p1={3}, p2={4}",
                    System.IO.Path.GetFileName(s), o1, s1, p1[0], p1[1]));
            }
            //var surfBow = BagOfVisualWords.Create(numberOfWords: 500);
            //IBagOfWords<Bitmap> bow = surfBow.Learn(test.ToArray());

        }
        static void test2()
        {
            foreach (string s in System.IO.Directory.GetFiles("mail_samples"))
            {
                Bitmap b = new Bitmap(s);//ImageDecoder.DecodeFromFile(s);
                //samples.Add(f1);
            }
            Bitmap f1 = new Bitmap(@"icons\temp_0x2.jpg");
            var surf = new SpeededUpRobustFeaturesDetector(threshold: 0.0002f, octaves: 5, initial: 2);
            var desc = surf.Transform(f1);
            List<double> fs = new List<double>();
            foreach(var v in desc)
            {
                foreach (var v1 in v.Descriptor)
                    fs.Add(v1);
            }
        }
        static void test()
        {
            Accord.Math.Random.Generator.Seed = 0;
            List<Bitmap> samples = new List<Bitmap>();
            List<Tuple<Bitmap, int>> data = new List<Tuple<Bitmap, int>>();
            foreach (string s in System.IO.Directory.GetFiles("mail_samples"))
            {
                Bitmap f1 = new Bitmap(s);//ImageDecoder.DecodeFromFile(s);
                //samples.Add(f1);
                data.Add(new Tuple<Bitmap, int>(f1, +1));
            }
            foreach (string s in System.IO.Directory.GetFiles("phone_icons"))
            {
                Bitmap f1 = new Bitmap(s); //ImageDecoder.DecodeFromFile(s);
                //samples.Add(f1);
                data.Add(new Tuple<Bitmap, int>(f1, -1));
            }
            var surfBow = BagOfVisualWords.Create(numberOfWords: 500);
            Bitmap[] bmps = new Bitmap[data.Count];
            for (int i = 0; i < bmps.Length; i++)
                bmps[i] = data[i].Item1;
            //bow.Learn(samples.ToArray());
            IBagOfWords<Bitmap> bow = surfBow.Learn(bmps);
            double[][] features = (bow as ITransform<Bitmap, double[]>).Transform(bmps);
            int[] labels = new int[data.Count];
            for(int i=0;i<labels.Length;i++)
            {
                labels[i] = data[i].Item2;
            }
            var teacher = new SequentialMinimalOptimization<Linear>()
            {
                Complexity = 10000 // make a hard margin SVM
            };
            var svm = teacher.Learn(features, labels);
            bool[] output = svm.Decide(features);

            Serializer.Save(obj: bow, path: "test_bow.bin");
            Serializer.Save(obj: svm, path: "test_svm.bin");

            Serializer.Load("test_svm.bin", out object load_svm);

            // test folder "icons\*.jpg"
            //Bitmap[] test = new Bitmap[]
            //{
            //    new Bitmap(@"C:\Users\qa\Desktop\picture\ios-7-app-icon-template_88183.png"),//ImageDecoder.DecodeFromFile("temp_1x1.jpg"),
            //    new Bitmap(@"C:\Users\qa\Desktop\picture\test_mail_icon.jpg"),//ImageDecoder.DecodeFromFile("temp_1x2.jpg"),
            //    new Bitmap(@"C:\Users\qa\Desktop\picture\iphone_icon\email_icon_t.jpg"),
            //};
            List<Bitmap> test = new List<Bitmap>();
            foreach (string s in System.IO.Directory.GetFiles("icons"))
            {
                Bitmap b = new Bitmap(s);
                test.Add(b);
            }
            double[][] fs = (bow as ITransform<Bitmap, double[]>).Transform(test.ToArray());
            output = svm.Decide(fs);
            double[] score = svm.Score(fs);
            double[][] prob = svm.Probabilities(fs);

            bool[] o1 = (load_svm as Accord.MachineLearning.VectorMachines.SupportVectorMachine<Accord.Statistics.Kernels.Linear>).Decide(fs);
            double[] s1 = (load_svm as Accord.MachineLearning.VectorMachines.SupportVectorMachine<Accord.Statistics.Kernels.Linear>).Score(fs);
            double[][] p1 = (load_svm as Accord.MachineLearning.VectorMachines.SupportVectorMachine<Accord.Statistics.Kernels.Linear>).Probabilities(fs);
            ImageBox.Show(test[1]).Hold();
        }
        static void extra_icon_from_home_screen_v2()
        {
            Bitmap f1 = ImageDecoder.DecodeFromFile(@"C:\test\save_00.jpg");
            int top_margin = (int) (0.026 * f1.Height); // 26;
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
            GaussianBlur gb = new GaussianBlur(2,10);
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
                foreach(var p in corners)
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
            int[] labels = { -1, +1, +1, +1, +1 ,+1};
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
            Accord.IntPoint [][] matches = matcher.Match(points1, points2);

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
                foreach(KeyValuePair<string,object> kv in locations)
                {
                    if(((Rectangle)kv.Value).Contains(p))
                    {
                        Program.logIt(String.Format("Found: in {0} and {1}", kv.Value, kv.Key));
                    }
                }
            }
        }

        static void extrac_blue_block(Bitmap src)
        {
            try
            {
                int w = src.Width / 3;
                int h = src.Height / 3;
                Crop c = new Crop(new Rectangle(0, 0, w, h));
                Bitmap b = c.Apply(src);
                GaussianBlur gb_filter = new GaussianBlur(4, 25);
                Bitmap src1 = gb_filter.Apply(b);
                src1.Save("temp_1.jpg");
                EuclideanColorFiltering ec_filter = new EuclideanColorFiltering();
                ec_filter.CenterColor= new RGB(Color.SlateBlue);
                ec_filter.Radius = 80;
                Bitmap img = ec_filter.Apply(src1);
                img.Save("temp_2.jpg");

                //ImageBox.Show(img);
                Bitmap grayImage = Grayscale.CommonAlgorithms.BT709.Apply(img);
                grayImage.Save("temp_3.jpg");
                // extrat biggest blob
                ExtractBiggestBlob eb_filter = new ExtractBiggestBlob();
                Bitmap bb = eb_filter.Apply(grayImage);
                //
                bb.Save("temp_4.jpg");
                // 
                Rectangle r = new Rectangle(eb_filter.BlobPosition.X, eb_filter.BlobPosition.Y, bb.Width, bb.Height);
                c = new Crop(r);
                b = c.Apply(src1);
                b.Save("temp_5.jpg");
                // 2nd round
                img = ec_filter.Apply(b);
                img.Save("temp_2.jpg");
                grayImage = Grayscale.CommonAlgorithms.BT709.Apply(img);
                grayImage.Save("temp_3.jpg");

                Threshold tf = new Threshold(50);
                tf.ApplyInPlace(grayImage);
                Invert invert = new Invert();
                invert.ApplyInPlace(grayImage);
                grayImage.Save("temp_3.jpg");

                ExtractBiggestBlob eb_filter_2 = new ExtractBiggestBlob();
                bb = eb_filter_2.Apply(grayImage);
                bb.Save("temp_4.jpg");
                Rectangle r2 = new Rectangle(r.X + eb_filter_2.BlobPosition.X, r.Y + eb_filter_2.BlobPosition.Y, bb.Width, bb.Height);
                Program.logIt(string.Format("r={0}", r2));
                c = new Crop(r2);
                b = c.Apply(src);
                b.Save("temp_1.jpg");
            }
            catch (Exception) { }
        }
    }
}
