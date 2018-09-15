using Accord.Imaging;
using Accord.Imaging.Filters;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Tesseract;

namespace testMQ
{
    class ocrEng
    {
        ocrEng()
        {
            //thisEng = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
        }
        static public string ocr_easy(Bitmap src)
        {
            string ret = string.Empty;
            TesseractEngine eng = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
            var res = eng.Process(src);
            ret = res.GetText();
            return ret;
        }
        static public void test(Bitmap src)
        {
            TesseractEngine eng = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
            var res = eng.Process(src);
            var r1 = res.GetIterator();
            var s = res.GetHOCRText(0, true);
        }
    }
    class MachineLearning
    {
        public static Rectangle[] blue_block_filter(Rectangle[] goodR)
        {
            List<Rectangle> retR = new List<Rectangle>();
            if (goodR.Length > 2)
            {
                Matrix<float> xy = new Matrix<float>(goodR.Length, 2);
                for (int i = 0; i < xy.Rows; i++)
                {
                    xy[i, 0] = goodR[i].X;
                    xy[i, 1] = goodR[i].Y;
                }
                Matrix<int> rx = new Matrix<int>(goodR.Length, 1);
                double dr = CvInvoke.Kmeans(xy, 2, rx, new MCvTermCriteria(), 2, 0);
                Rectangle[] rrr = new Rectangle[] { Rectangle.Empty, Rectangle.Empty };
                for (int i = 0; i < rx.Rows; i++)
                {
                    if (rx[i, 0] == 0)
                    {
                        if (rrr[0] == Rectangle.Empty) rrr[0] = goodR[i];
                        else rrr[0] = Rectangle.Union(rrr[0], goodR[i]);
                    }
                    if (rx[i, 0] == 1)
                    {
                        if (rrr[1] == Rectangle.Empty) rrr[1] = goodR[i];
                        else rrr[1] = Rectangle.Union(rrr[1], goodR[i]);
                    }
                }
                retR = new List<Rectangle>(rrr);
            }
            else if (goodR.Length == 2)
                retR = new List<Rectangle>(goodR);
            return retR.ToArray();
        }
    }

    class ImUtility
    {
        public static Bitmap bitwies_and(Bitmap src, Bitmap mask)
        {
            Bitmap ret = null;
            Image<Bgr, Byte> img1 = new Image<Bgr, byte>(src);
            Image<Bgr, Byte> img2 = new Image<Bgr, byte>(mask);
            Mat r = new Mat();
            CvInvoke.BitwiseAnd(img1, img2, r);
            ret = r.Bitmap;
            return ret;
        }
        public static double get_PSNR(Bitmap b1, Bitmap b2)
        {
            double psnr = 0.0;
            if (b1 != null && b2 != null)
            {
                //Image<Bgra, Byte> img1 = new Image<Bgra, byte>(b1);
                //Image<Bgra, Byte> img2 = new Image<Bgra, byte>(b2);
                Image<Gray, Byte> img1 = new Image<Gray, byte>(b1);
                Image<Gray, Byte> img2 = new Image<Gray, byte>(b2);
                /*
                Mat m1 = img1.Mat; // CvInvoke.Imread(@"C:\test\save_00.jpg", ImreadModes.AnyColor);
                Mat m2 = img2.Mat; // CvInvoke.Imread(@"C:\test\save_01.jpg", ImreadModes.AnyColor);
                Mat diff = new Mat();
                CvInvoke.AbsDiff(m1, m2, diff);
                Mat bf = new Mat();
                diff.ConvertTo(bf, DepthType.Cv32F);
                CvInvoke.Multiply(bf, bf, bf);
                MCvScalar sum = CvInvoke.Sum(bf);
                double sse = sum.V0 + sum.V1 + sum.V2;
                double mse = sse / (double)(m1.NumberOfChannels * m1.Total.ToInt32());
                psnr = 10.0 * Math.Log10((255 * 255) / mse); // less than 15, meaning b1 is diff as b2
                */
                psnr = CvInvoke.PSNR(img1, img2);
            }
            Program.logIt(string.Format("psnr={0}", psnr));
            return psnr;
        }

        public static Rectangle[] haar_detect(string cascadefile, Image<Bgra, byte> source)
        {
            Rectangle[] ret = null;
            try
            {
                CascadeClassifier haar_email = new CascadeClassifier(cascadefile);
                ret = haar_email.DetectMultiScale(source);
            }
            catch (Exception) { }
            return ret;
        }
        public static System.Collections.Generic.Dictionary<string, object> extra_icon(Image<Bgra, byte> source)
        {
            System.Collections.Generic.Dictionary<string, object> ret = new Dictionary<string, object>();
            double margin_left = 0.07;
            double margin_width = 0.07;
            double margin_top = 0.034;
            double top_of_botton_line = 0.88;
            double margin_height = 0.04;
            double icon_width = 0.163;
            double icon_higth = 0.093;
            double icon_marging = 0.1;
            //Image<Bgra, byte> source = new Image<Bgra, byte>(@"C:\Users\qa\Desktop\picture\test_02.jpg");
            double y = margin_top * source.Height;
            double w = icon_width * source.Width;
            double h = icon_higth * source.Height;
            double botton_y = top_of_botton_line * source.Height;
            int row = 1;
            int col = 1;
            int idx = 0;
            while (y + h < botton_y)
            {
                col = 1;
                double x = margin_left * source.Width;
                while (x + w < source.Width)
                {
                    double xr = x - w * icon_marging;
                    double yr = y - h * icon_marging;
                    double wr = w + w * icon_marging * 2;
                    double hr = h + h * icon_marging * 2;
                    Rectangle r = new Rectangle((int)xr, (int)yr, (int)wr, (int)hr);
                    //source.GetSubRect(r).Save(string.Format(@"icons\icon_{0}_{1}.jpg", row, col++));
                    System.Collections.Generic.Dictionary<string, object> d = new Dictionary<string, object>();
                    d.Add("rectangle", r);
                    d.Add("index", idx++);
                    d.Add("label", "");
                    d.Add("row", row);
                    d.Add("col", col);
                    ret.Add(string.Format("{0}_{1}", row, col++), d);
                    source.Draw(r, new Bgra(0, 0, 255, 255));
                    x = x + w + margin_width * source.Width;
                }
                y = y + h + margin_height * source.Height;
                row++;
            }
            // dotton line
            {
                idx = -4;
                row = 0;
                col = 1;
                y = botton_y;
                double x = margin_left * source.Width;
                while (x + w < source.Width)
                {
                    double xr = x - w * icon_marging;
                    double yr = y - h * icon_marging;
                    double wr = w + w * icon_marging * 2;
                    double hr = h + h * icon_marging * 2;
                    Rectangle r = new Rectangle((int)xr, (int)yr, (int)wr, (int)hr);
                    //source.GetSubRect(r).Save(string.Format(@"icons\icon_{0}_{1}.jpg", row, col++));
                    System.Collections.Generic.Dictionary<string, object> d = new Dictionary<string, object>();
                    d.Add("rectangle", r);
                    d.Add("index", idx++);
                    d.Add("label", "");
                    d.Add("row", row);
                    d.Add("col", col);
                    ret.Add(string.Format("{0}_{1}", row, col++), d);
                    source.Draw(r, new Bgra(0, 0, 255, 255));
                    x = x + w + margin_width * source.Width;
                }
            }
            //source.Save("temp_1.jpg");
            return ret;
        }
        public static Bitmap extract_different_mask(Bitmap b1, Bitmap b2)
        {
            Bitmap ret = null;
            Image<Gray, Byte> img1 = new Image<Gray, byte>(b1);
            Image<Gray, Byte> img2 = new Image<Gray, byte>(b2);
            Mat diff = new Mat();
            CvInvoke.AbsDiff(img1, img2, diff);
            Mat tmp = new Mat();
            CvInvoke.Threshold(diff, tmp, 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
            diff = tmp;
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.FindContours(diff, contours, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
                int count = contours.Size;
                double m = 0.0;
                int idx = -1;
                for (int i = 0; i < count; i++)
                {
                    double d = CvInvoke.ContourArea(contours[i]);
                    if (d > m)
                    {
                        m = d;
                        idx = i;
                    }
                }
                if (idx >= 0)
                {
                    tmp= Mat.Ones(img2.Mat.Rows, img2.Mat.Cols, img2.Mat.Depth, img2.Mat.NumberOfChannels);
                    VectorOfVectorOfPoint vvp = new VectorOfVectorOfPoint();
                    vvp.Push(contours[idx]);
                    CvInvoke.FillPoly(tmp, vvp, new MCvScalar(255));
                    CvInvoke.CvtColor(tmp, diff, ColorConversion.Gray2Bgr);
                    ret = diff.Bitmap;
                }
            }
            return ret;
        }
        public static Rectangle detect_blue_rectangle(Bitmap b1, Bitmap b2)
        {
            Rectangle ret = Rectangle.Empty;
            Image<Bgr, Byte> img1 = new Image<Bgr, byte>(b1);
            Image<Bgr, Byte> img2 = new Image<Bgr, byte>(b2);
            Mat diff = new Mat();
            CvInvoke.AbsDiff(img1, img2, diff);
            Mat tmp = new Mat();
            CvInvoke.CvtColor(diff, tmp, ColorConversion.Bgr2Gray);
            CvInvoke.Threshold(tmp, diff, 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.FindContours(diff, contours, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
                int count = contours.Size;
                double m = 0.0;
                for(int i = 0; i < count; i++)
                {
                    double d = CvInvoke.ContourArea(contours[i]);
                    if (d > m)
                    {
                        m = d;
                        ret = CvInvoke.BoundingRectangle(contours[i]);
                    }
                }
            }
            Program.logIt(string.Format("detect_blue_rectangle: -- {0}", ret));
            return ret;
        }
        public static Rectangle[] detect_blue_rectangle_1(Image<Bgr, Byte> img1, Image<Bgr, Byte> img2)
        {
            List<Rectangle> ret = new List<Rectangle>();
            if (img1.Size == img2.Size)
            {
                Image<Bgr, Byte> diff = img2.AbsDiff(img1);
                UMat uimage = new UMat();
                CvInvoke.CvtColor(diff, uimage, ColorConversion.Bgr2Gray);
                UMat pyrDown = new UMat();
                CvInvoke.PyrDown(uimage, pyrDown);
                CvInvoke.PyrUp(pyrDown, uimage);
                //uimage.Save("temp_1.jpg");
                double cannyThreshold = 20.0;
                double cannyThresholdLinking = 120.0;
                UMat cannyEdges = new UMat();
                CvInvoke.Canny(uimage, cannyEdges, cannyThreshold, cannyThresholdLinking);
                using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                {
                    CvInvoke.FindContours(cannyEdges, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);
                    int count = contours.Size;
                    for (int i = 0; i < count; i++)
                    {
                        RotatedRect rr = CvInvoke.MinAreaRect(contours[i]);
                        Rectangle r = rr.MinAreaRect();
                        if (r.X > 0 && r.Y > 0 && r.Width > 100 && r.Height > 100)
                        {
                            //System.Diagnostics.Trace.WriteLine(string.Format("rect={0}", r));
                            ret.Add(r);
                        }
                    }
                }
                /*
                LineSegment2D[] lines = CvInvoke.HoughLinesP(
                               cannyEdges,
                               1, //Distance resolution in pixel-related units
                               Math.PI / 45.0, //Angle resolution measured in radians.
                               20, //threshold
                               30, //min Line width
                               10); //gap between lines
                //cannyEdges.Save("temp_2.jpg");
                List<Triangle2DF> triangleList = new List<Triangle2DF>();
                List<RotatedRect> boxList = new List<RotatedRect>(); //a box is a rotated rectangle
                using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                {
                    CvInvoke.FindContours(cannyEdges, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);
                    int count = contours.Size;
                    for (int i = 0; i < count; i++)
                    {
                        using (VectorOfPoint contour = contours[i])
                        using (VectorOfPoint approxContour = new VectorOfPoint())
                        {
                            CvInvoke.ApproxPolyDP(contour, approxContour, CvInvoke.ArcLength(contour, true) * 0.05, true);
                            double a = CvInvoke.ContourArea(approxContour, false);
                            //System.Diagnostics.Trace.WriteLine(string.Format("area={0}", a));
                            if (a > 50) //only consider contours with area greater than 250
                            {
                                RotatedRect rr = CvInvoke.MinAreaRect(approxContour);
                                Rectangle r = rr.MinAreaRect();
                                //CvInvoke.Polylines(diff, rr.GetVertices(), true, new MCvScalar(255, 255, 0, 0), 5);
                                System.Diagnostics.Trace.WriteLine(string.Format("rect={0} and size={1}", r, r.Width * r.Height));
                                if (r.Width * r.Height < 20000 && r.Width * r.Height > 10000 && r.X > 0 && r.Y > 0&& r.Width > 100 && r.Height > 100)
                                {
                                    //System.Diagnostics.Trace.WriteLine(string.Format("rect={0} and size={1}", r, r.Width * r.Height));
                                    ret.Add(r);
                                    //CvInvoke.Rectangle(diff, rr.MinAreaRect(), new MCvScalar(255, 255, 0, 0));
                                }
                                //diff.Save("temp_3.jpg");
                            }
                        }
                    }
                }*/
            }
            else
            {
                // 2 image are not in same size.
            }
            if (ret.Count > 2)
            {
                ret = new List<Rectangle>(MachineLearning.blue_block_filter(ret.ToArray()));
            }
            return ret.ToArray();
        }
        public static bool is_all_image_same(Bitmap []bmps, double threshold = 50.0)
        {
            bool ret = false;
            if (bmps != null && bmps.Length>0)
            {
                Bitmap b0 = bmps[0];
                bool all_same = true;
                foreach(Bitmap b in bmps)
                {
                    if (!is_same_image(b, b0, threshold))
                    {
                        all_same = false;
                        break;
                    }
                }
                ret = all_same;
            }
            return ret;
        }
        public static bool is_same_image(Bitmap f1, Bitmap f2, double threshold=30.0)
        {
            bool ret = false;
            if (f1 != null && f2 != null)
            {
                if (f1.Width == f2.Width && f1.Height == f2.Height)
                {
                    //Subtract sf = new Subtract(f1);
                    //Bitmap r = sf.Apply(f2);
                    //ret = CheckEmptyImageByML.getInstance().isImageEmpty(r);
                    double d = get_PSNR(f1, f2);
                    if (d > threshold)
                        ret = true;
                }
                else
                {
                    Program.logIt("diff_images: can not process due to the size of image is not same.");
                }
            }
            Program.logIt(string.Format("is_same_image: -- ret={0} ", ret));
            return ret;
        }
        public static Tuple<bool, float, Bitmap> diff_images(Bitmap f1, Bitmap f2)
        {
            bool retB = false;
            float retF = 0.0f;
            Bitmap retI = null;
            Program.logIt("diff_images: ++");
            if(f1!=null && f2 != null)
            {
                if(f1.Width==f2.Width&& f1.Height == f2.Height)
                {
                    Subtract sf = new Subtract(f1);
                    Bitmap r = sf.Apply(f2);
                    retI = r;
                }
                else
                {
                    Program.logIt("diff_images: can not process due to the size of image is not same.");
                }
            }
            Program.logIt(string.Format("diff_images: -- ret={0} similarity={1}", retB, retF));
            return new Tuple<bool, float, Bitmap>(retB, retF, retI);
        }
        public static Tuple<Bitmap, double, Rectangle> smate_rotate(Bitmap src)
        {
            Bitmap retB = null;
            Rectangle retR = Rectangle.Empty;
            double retAngle = 90.0;
            double angle = 0.0;
            RotateBicubic filter = new RotateBicubic(retAngle);
            Bitmap src1 = filter.Apply(src);
            //Bitmap src1 = (Bitmap)src.Clone();
            Bitmap g = Grayscale.CommonAlgorithms.BT709.Apply(src1);
            Invert it = new Invert();
            it.ApplyInPlace(g);
            //g.Save("temp_1.jpg");
            ImageStatistics stat = new ImageStatistics(g);
            Threshold t = new Threshold((int)(stat.Gray.Mean - stat.Gray.StdDev));
            t.ApplyInPlace(g);
            //g.Save("temp_2.jpg");
            stat = new ImageStatistics(g);
            DifferenceEdgeDetector edgeDetector = new DifferenceEdgeDetector();
            edgeDetector.ApplyInPlace(g);
            //g.Save("temp_3.jpg");
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
        public static Tuple<Bitmap, double, Rectangle> smate_rotate_1(Bitmap src, double pre_angle=0.0, double min_area=0.0)
        {
            Bitmap retB = null;
            double retD = 0.0;
            Rectangle retR = Rectangle.Empty;
            Image<Bgra, byte> rotated = null;
            Image<Bgra, byte> img = new Image<Bgra, byte>(src);
            //img.Save("temp_1.jpg");
            UMat uimage = new UMat();
            CvInvoke.CvtColor(img, uimage, ColorConversion.Bgr2Gray);
            UMat pyrDown = new UMat();
            CvInvoke.PyrDown(uimage, pyrDown);
            CvInvoke.PyrUp(pyrDown, uimage);
            CvInvoke.Threshold(uimage, pyrDown, 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
            uimage = pyrDown;
            double cannyThreshold = 180.0;
            double cannyThresholdLinking = 120.0;
            UMat cannyEdges = new UMat();
            CvInvoke.Canny(uimage, cannyEdges, cannyThreshold, cannyThresholdLinking);
            {
                List<double> angles = new List<double>();
                double ratio = 1.0;
                LineSegment2D[] lines = CvInvoke.HoughLinesP(
                   cannyEdges,
                   1, //Distance resolution in pixel-related units
                   Math.PI / 180.0, //Angle resolution measured in radians.
                   20, //threshold
                   100, //min Line width
                   10); //gap between lines
                for(int i=0; i < lines.Length; i++)
                {
                    double k = ((double)(lines[i].P1.Y - lines[i].P2.Y)) / ((double)(lines[i].P1.X - lines[i].P2.X));
                    double a = Math.Atan(Math.Abs(k)) * (180 / Math.PI);
                    if (a > 45.0)
                    {
                        a = 90.0 - a;
                        if (k > 0) ratio = 1.0;
                        else ratio = -1.0;
                    }
                    else
                    {
                        if (k > 0) ratio = -1.0;
                        else ratio = 1.0;
                    }
                    angles.Add(a);
                    Program.logIt(string.Format("{0}-{1}, {3} len={2}", lines[i].P1, lines[i].P2, lines[i].Length, a));
                }
                if (angles.Count > 0)
                {
                    ratio *= angles.Average();
                    retD = pre_angle + ratio;
                    rotated = img.Rotate(retD, new Bgra(0, 0, 0, 255), false);
                }
            }
            if (rotated != null)
            {
                uimage = new UMat();
                CvInvoke.CvtColor(rotated, uimage, ColorConversion.Bgr2Gray);
                pyrDown = new UMat();
                CvInvoke.PyrDown(uimage, pyrDown);
                CvInvoke.PyrUp(pyrDown, uimage);
                //MCvScalar mean = new MCvScalar();
                //MCvScalar std_dev = new MCvScalar();
                //CvInvoke.MeanStdDev(uimage, ref mean, ref std_dev);
                //CvInvoke.Threshold(uimage, pyrDown, mean.V0 + std_dev.V0, 255, ThresholdType.Binary);
                CvInvoke.Threshold(uimage, pyrDown, 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
                uimage = pyrDown;
                //uimage.Save("temp_2.jpg");
                Rectangle roi = Rectangle.Empty;
                using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                {
                    CvInvoke.FindContours(uimage, contours, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
                    int count = contours.Size;
                    for (int i = 0; i < count; i++)
                    {
                        double a1 = CvInvoke.ContourArea(contours[i], false);
                        Rectangle r = CvInvoke.BoundingRectangle(contours[i]);
                        //Program.logIt(string.Format("{0}", r));
                        if (a1 > min_area)
                        {
                            if (roi.IsEmpty) roi = r;
                            else roi = Rectangle.Union(roi, r);
                        }
                    }
                }
                Image<Bgra, byte> copped = rotated.GetSubRect(roi);
                //copped.Save("temp_3.jpg");
                retB = copped.Bitmap;
                retR = roi;
            }

            return new Tuple<Bitmap, double, Rectangle>(retB, retD, retR);
        }
        public static Tuple<bool, Rectangle, Bitmap> extrac_blue_block(Bitmap src, Size minS = default(Size))
        {
            bool retB = false;
            Rectangle retR = Rectangle.Empty;
            Bitmap retImg = null;
            Image<Bgr, Byte> img = new Image<Bgr, byte>(src);
            Mat img1 = new Mat();
            CvInvoke.GaussianBlur(img, img1, new Size(11, 11), 0);
            //img1.Save("temp_2.jpg");

            Bgr c1 = new Bgr(160, 80, 10); //new Bgr(160, 50, 30);
            Bgr c2 = new Bgr(250, 140, 50); //new Bgr(250, 200, 100);
            //Bgr c1 = new Bgr(170, 130, 70); //new Bgr(160, 50, 30);
            //Bgr c2 = new Bgr(250, 230, 160); //new Bgr(250, 200, 100);
            //Bgr c1 = new Bgr(80, 20, 10);
            //Bgr c2 = new Bgr(160, 100, 60);
            Image<Gray, Byte> g1 = (img1.ToImage<Bgr, Byte>()).InRange(c1, c2);
            //g1.Save("temp_2.jpg");
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
        public static Tuple<bool, Rectangle, Bitmap> extrac_context_menu(Bitmap b1, Bitmap b2)
        {
            bool retB = false;
            Rectangle retR = Rectangle.Empty;
            Bitmap retImg = null;
            Image<Bgra, byte> img1 = new Image<Bgra, byte>(b1);
            Image<Bgra, byte> img2 = new Image<Bgra, byte>(b2);
            Image<Bgra, byte> diff = img2.AbsDiff(img1);
            //diff.Save("temp_2.jpg");
            img1 = diff.PyrDown();
            diff = img1.PyrUp();
            //img2.Save("temp_2.jpg");
            UMat uimage = new UMat();
            CvInvoke.CvtColor(diff, uimage, ColorConversion.Bgr2Gray);
            //uimage.Save("temp_3.jpg");
            //MCvScalar mean = new MCvScalar();
            //MCvScalar std_dev = new MCvScalar();
            //CvInvoke.MeanStdDev(uimage, ref mean, ref std_dev);
            UMat bimage = new UMat();
            //CvInvoke.Threshold(uimage, bimage, mean.V0 + std_dev.V0, 255, ThresholdType.Binary);
            CvInvoke.Threshold(uimage, bimage, 0, 255, ThresholdType.Binary| ThresholdType.Otsu);
            //bimage.Save("temp_2.jpg");
            Rectangle roi = Rectangle.Empty;
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.FindContours(bimage, contours, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
                int count = contours.Size;
                for (int i = 0; i < count; i++)
                {
                    double a1 = CvInvoke.ContourArea(contours[i], false);
                    Rectangle r = CvInvoke.BoundingRectangle(contours[i]);
                    //Program.logIt(string.Format("{0}", r));
                    if (a1 > 100.0)
                    {
                        //Program.logIt(string.Format("{0}", r));
                        if (roi.IsEmpty) roi = r;
                        else roi = Rectangle.Union(roi, r);
                    }
                }
            }
            if (!roi.IsEmpty && img2.Width>roi.Width && img2.Height>roi.Height)
            {
                //Image<Bgra, byte> cropped = img2.GetSubRect(roi);
                retB = true;
                retR = roi;
                retImg = img2.GetSubRect(retR).Bitmap;
                //Program.logIt(string.Format("rect={0}", retR));
            }
            return new Tuple<bool, Rectangle, Bitmap>(retB, retR, retImg);
        }
        public static Bitmap crop_image(Bitmap src, Rectangle rect, double enlarge = 0.0)
        {
            Bitmap ret = null;
            if (src != null && !rect.IsEmpty)
            {
                Image<Bgra, Byte> i = new Image<Bgra, byte>(src);
                Rectangle r = rect;
                if (enlarge != 0.0)
                {
                    int w = (int)(enlarge * rect.Width);
                    int h = (int)(enlarge * rect.Height);
                    r.Inflate(new Size(w, h));
                }
                if (i.Width > r.Width && i.Height > r.Height)
                {
                    ret = i.GetSubRect(r).Bitmap;
                }
            }
            return ret;
        }
        public static Tuple<bool, Rectangle, Bitmap> find_focused_menu(Bitmap src)
        {
            Rectangle retR = Rectangle.Empty;
            Bitmap retB = null;
            bool ret = false;
            Image<Gray, Byte> uimage = new Image<Gray, Byte>(src);
            UMat pyrDown = new UMat();
            CvInvoke.PyrDown(uimage, pyrDown);
            CvInvoke.PyrUp(pyrDown, uimage);
            double cannyThreshold = 180.0;
            double cannyThresholdLinking = 120.0;
            UMat cannyEdges = new UMat();
            CvInvoke.Canny(uimage, cannyEdges, cannyThreshold, cannyThresholdLinking);
            //cannyEdges.Save("temp_1.jpg");
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
                            //Program.logIt(string.Format("{0}", r));
                            //using(Graphics g = Graphics.FromImage(src))
                            //{
                            //    g.DrawRectangle(new Pen(Color.Red), r);
                            //}
                            if (r.Width * r.Height > retR.Width * retR.Height)
                                retR = r;
                        }
                    }
                }
            }
            //src.Save("temp_2.jpg");
            if (!retR.IsEmpty)
            {
                ret = true;
                Image<Bgr, byte> i = new Image<Bgr, byte>(src);
                retB = (i.GetSubRect(retR)).Bitmap;
            }
            return new Tuple<bool, Rectangle, Bitmap>(ret, retR, retB);
        }
        public static Tuple<bool, Rectangle> extra_blue_block_in_settings(Bitmap src)
        {
            bool retB = false;
            Rectangle retR = Rectangle.Empty;
            Image<Bgr, Byte> b1 = new Image<Bgr, Byte>(src);
            Mat img1 = new Mat();
            CvInvoke.GaussianBlur(b1, img1, new Size(7, 7), 0);
            //img1.Save("temp_1.jpg");
            Bgr c1 = new Bgr(200, 100, 0);
            Bgr c2 = new Bgr(255, 220, 50);
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

    }
}
