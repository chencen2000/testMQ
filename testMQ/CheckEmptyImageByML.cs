using Accord.Imaging;
using Accord.Imaging.Filters;
using Accord.MachineLearning;
using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace testMQ
{
    class CheckEmptyImageByML
    {
        // data
        //new double[] { 1.73407600308642,0,5.62588471221633},  -1
        //new double[] { 53.6045780285494,8,79.2103241645548 }, +1
        //new double[] {  53.1015615354938,7,79.3755488866987 },+1
        //new double[] {  53.2143330439815,7,79.3365665756378 },+1
        //new double[]{ 8.12086950231481, 3, 11.8416794313788 },+1
        //new double[]{ 4.01413675864101, 1, 13.026857481124 }, +1

        static private CheckEmptyImageByML pThis = null;
        static public CheckEmptyImageByML getInstance()
        {
            if (pThis == null)
                pThis = new CheckEmptyImageByML();
            return pThis;
        }
        List<double[]> inputList = null;
        int[] labels = null;
        SupportVectorMachine svm = null;
        CheckEmptyImageByML()
        {
            inputList = new List<double[]>(
                new double[][]
                {
                    new double[] { 1.73407600308642,0,5.62588471221633},
                    new double[] { 53.6045780285494,8,79.2103241645548 },
                    new double[] {  53.1015615354938,7,79.3755488866987 },
                    new double[] {  53.2143330439815,7,79.3365665756378 },
                    new double[]{ 8.12086950231481, 3, 11.8416794313788 },
                    new double[]{ 4.01413675864101, 1, 13.026857481124 },
                    new double[]{ 2.56249693146112,0,4.74375226238893},
                });
            labels = new int[] { -1, +1, +1, +1, +1, +1, -1 };
            var learn = new SequentialMinimalOptimization()
            {
                UseComplexityHeuristic = true,
                UseKernelEstimation = false
            };
            svm = learn.Learn(inputList.ToArray(), labels);
        }
        public bool isImageEmpty(Bitmap src)
        {
            bool ret = false;
            Bitmap g = Grayscale.CommonAlgorithms.BT709.Apply(src);
            ImageStatistics stat = new ImageStatistics(g);
            double[][] ds = { new double[] { stat.Gray.Mean, stat.Gray.Median, stat.Gray.StdDev } };
            Program.logIt(string.Format("{0},{1},{2}", ds[0][0], ds[0][1], ds[0][2]));
            bool[] res = svm.Decide(ds);
            ret = !res[0];
            return ret;
        }

    }
    class CheckEmptyImageByML_V1
    {
        // data:
        // 53.6045780285494,8,79.2103241645548
        // 53.1015615354938,7,79.3755488866987
        // 53.2143330439815,7,79.3365665756378
        // 1.73407600308642,0,5.62588471221633
        List<double[]> inputList = null;
        KMeans kmeans = null;
        KMeansClusterCollection clusters = null;
        System.Collections.Generic.Dictionary<int, bool> output;
        static private CheckEmptyImageByML_V1 pThis = null;
        static public CheckEmptyImageByML_V1 getInstance()
        {
            if (pThis == null)
                pThis = new CheckEmptyImageByML_V1();
            return pThis;
        }
        CheckEmptyImageByML_V1()
        {
            inputList = new List<double[]>(
                new double[][]
                {
                    new double[] { 1.73407600308642,0,5.62588471221633},
                    new double[] { 53.6045780285494,8,79.2103241645548 },
                    new double[] {  53.1015615354938,7,79.3755488866987 },
                    new double[] {  53.2143330439815,7,79.3365665756378 },
                    new double[]{ 8.12086950231481, 3, 11.8416794313788 },
                });
            output = new Dictionary<int, bool>();
            output.Add(0, true);
            output.Add(1, true);
            kmeans = new KMeans(2);
            train();
        }
        void train()
        {
            clusters = kmeans.Learn(inputList.ToArray());
            // make a empty picture  
            {
                Bitmap mask = new Bitmap(300, 300);
                using (Graphics g = Graphics.FromImage(mask))
                {
                    g.FillRectangle(new SolidBrush(Color.Black), new Rectangle(0, 0, mask.Width, mask.Height));
                }
                //Bitmap g = Grayscale.CommonAlgorithms.BT709.Apply(mask);
                ImageStatistics stat = new ImageStatistics(Grayscale.CommonAlgorithms.BT709.Apply(mask));
                double[][] ds = { new double[] { stat.Gray.Mean, stat.Gray.Median, stat.Gray.StdDev } };
                //Program.logIt(string.Format("{0},{1},{2}", ds[0][0], ds[0][1], ds[0][2]));
                int[] res = clusters.Decide(ds);
                output[0] = true;
                output[1] = true;
                output[res[0]] = false;
            }
        }
        public bool isImageEmpty(Bitmap src)
        {
            bool ret = false;
            Bitmap g = Grayscale.CommonAlgorithms.BT709.Apply(src);
            ImageStatistics stat = new ImageStatistics(g);
            double[][] ds = { new double[] { stat.Gray.Mean, stat.Gray.Median, stat.Gray.StdDev } };
            Program.logIt(string.Format("{0},{1},{2}", ds[0][0], ds[0][1], ds[0][2]));
            int[] res = clusters.Decide(ds);
            ret = !output[res[0]];
            return ret;
        }
    }
}
