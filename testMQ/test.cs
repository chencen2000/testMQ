using Accord;
using Accord.Imaging;
using Accord.Imaging.Filters;
using Accord.Imaging.Formats;
using Accord.MachineLearning;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Math.Geometry;
using Accord.Math.Optimization.Losses;
using Accord.Statistics.Kernels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace testMQ
{
    class Test
    {
        static void Main()
        {
            Program.logIt("Test Starts.");
            test();
            //test_ml();
            //test_ml_SMO();
            //extra_icon_from_home_screen();
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
            var bow = BagOfVisualWords.Create(numberOfWords: 100);
            Bitmap[] bmps = new Bitmap[data.Count];
            for (int i = 0; i < bmps.Length; i++)
                bmps[i] = data[i].Item1;
            //bow.Learn(samples.ToArray());
            bow.Learn(bmps);
            double[][] features = bow.Transform(bmps);
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

            Bitmap[] test = new Bitmap[]
            {
                new Bitmap("temp_1x1.jpg"),//ImageDecoder.DecodeFromFile("temp_1x1.jpg"),
                new Bitmap("temp_2x1.jpg"),//ImageDecoder.DecodeFromFile("temp_1x2.jpg"),
                new Bitmap(@"C:\Users\qa\Desktop\picture\iphone_icon\email_icon_t.jpg"),
            };
            double[][] fs = bow.Transform(test);
            output = svm.Decide(fs);
        }
        static void extra_icon_from_home_screen()
        {
            Bitmap f1 = ImageDecoder.DecodeFromFile(@"C:\Users\qa\Desktop\picture\save_12.jpg");
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
                    b.Save(string.Format("temp_{0}x{1}.jpg", row, col));
                }
            }
            // for dock
            for (col = 1; col <= 4; col++)
            {
                x = 28 + xstep * (col - 1);
                Rectangle r = new Rectangle(x, ybottom, 140, 140);
                Crop c = new Crop(r);
                Bitmap b = c.Apply(f1);
                b.Save(string.Format("temp_0x{0}.jpg", col));
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
            Bitmap f1 = ImageDecoder.DecodeFromFile(@"C:\test\save_00.jpg");
            Bitmap f2 = ImageDecoder.DecodeFromFile(@"C:\test\save_01.jpg");
            Tuple<bool, float, Bitmap> r = ImUtility.diff_images(f1, f2);
            if(r.Item3!=null)
            {
                r.Item3.Save("temp_1.jpg");
                bool b = CheckEmptyImageByML.getInstance().isImageEmpty(r.Item3);
            }
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
        
    }
}
