using Accord.Imaging;
using Accord.Imaging.Filters;
using Accord.Imaging.Formats;
using Accord.MachineLearning;
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
        }
        static void test()
        {
            Bitmap f1 = ImageDecoder.DecodeFromFile(@"C:\test\save_00.jpg");
            Tuple<Bitmap, double, Rectangle> r = smate_rotate(f1);
            if (r.Item1 != null) 
                r.Item1.Save("temp_1.jpg");

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
        static Tuple<Bitmap, double, Rectangle> smate_rotate(Bitmap src)
        {
            Bitmap retB = null;
            Rectangle retR = Rectangle.Empty;
            double retAngle = 90.0;
            double angle = 0.0;
            RotateBicubic filter = new RotateBicubic(retAngle);
            Bitmap src1 = filter.Apply(src);
            Bitmap g = Grayscale.CommonAlgorithms.BT709.Apply(src1);
            Invert it = new Invert();
            it.ApplyInPlace(g);
            g.Save("temp_1.jpg");
            ImageStatistics stat = new ImageStatistics(g);
            Threshold t = new Threshold((int)(stat.Gray.Mean - stat.Gray.StdDev));
            t.ApplyInPlace(g);
            g.Save("temp_2.jpg");
            stat = new ImageStatistics(g);
            DifferenceEdgeDetector edgeDetector = new DifferenceEdgeDetector();
            edgeDetector.ApplyInPlace(g);
            g.Save("temp_3.jpg");
            HoughLineTransformation lineTransform = new HoughLineTransformation();
            lineTransform.ProcessImage(g);
            HoughLine[] lines = lineTransform.GetLinesByRelativeIntensity(0.8);
            foreach (HoughLine l in lines)
            {
                Program.logIt(string.Format("Intensity={0}, Radius={1}, Theta={2}", l.Intensity, l.Radius, l.Theta));
                if (l.Radius < 0)
                {
                    if (l.Theta < 90) angle = -l.Theta;
                    else angle = 180.0 - l.Theta;
                }
                else
                {
                    if (l.Theta < 90) angle = -l.Theta;
                    else angle = 180.0 - l.Theta;
                }
                if (Math.Abs(angle) < 45.0)
                    break;
            }
            Program.logIt(string.Format("angle={0}", angle));
            retAngle += angle;
            RotateBicubic r_filter = new RotateBicubic(angle);
            Bitmap rotated = r_filter.Apply(src1);
            // crop
            if (rotated != null)
            {
                Grayscale g_filter = new Grayscale(0.2125, 0.7154, 0.0721);
                Bitmap grayImage = g_filter.Apply(rotated);
                Blur bf = new Blur();
                bf.ApplyInPlace(grayImage);
                OtsuThreshold o_filter = new OtsuThreshold();
                o_filter.ApplyInPlace(grayImage);
                BlobCounter blobCounter = new BlobCounter();
                blobCounter.MinHeight = 20;
                blobCounter.MinWidth = 20;
                blobCounter.FilterBlobs = false;
                blobCounter.BlobsFilter = null;
                blobCounter.ObjectsOrder = ObjectsOrder.YX;
                blobCounter.ProcessImage(grayImage);
                Blob[] blobs = blobCounter.GetObjectsInformation();
                Program.logIt(string.Format("blobs={0}", blobCounter.ObjectsCount));
                Rectangle r = Rectangle.Empty;
                for (int i = 1; i < blobs.Length; i++)
                {
                    Blob b = blobs[i];
                    Program.logIt(string.Format("{0}: {1}", b.ID, b.Rectangle));
                    if (r == Rectangle.Empty) r = b.Rectangle;
                    else r = Rectangle.Union(r, b.Rectangle);
                }
                Program.logIt(string.Format("rect: {0}", r));
                retR = r;
                Crop c_filter = new Crop(r);
                retB = c_filter.Apply(rotated);
            }
            return new Tuple<Bitmap, double, Rectangle>(retB, retAngle, retR);
        }
    }
}
