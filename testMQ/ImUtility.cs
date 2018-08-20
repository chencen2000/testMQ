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

namespace testMQ
{
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
        public static Rectangle[] detect_blue_rectangle(Image<Bgr, Byte> img1, Image<Bgr, Byte> img2)
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
                MCvScalar m1 = new MCvScalar();
                MCvScalar m2 = new MCvScalar();
                CvInvoke.MeanStdDev(uimage, ref m1, ref m2);
                Image<Gray, Byte> t = uimage.ToImage<Gray, Byte>().ThresholdBinary(new Gray(m1.V0 + m2.V0), new Gray(255));
                uimage = t.ToUMat();
                //uimage.Save("temp_1.jpg");
                using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                {
                    //Matrix<int> hierarchy = new Matrix<int>(1, contours.Size);
                    Mat hierarchy = new Mat();
                    CvInvoke.FindContours(uimage, contours, hierarchy, RetrType.External, ChainApproxMethod.ChainApproxNone);
                    int count = contours.Size;
                    for (int i = 0; i < count; i++)
                    {
                        RotatedRect rr = CvInvoke.MinAreaRect(contours[i]);
                        Rectangle r = rr.MinAreaRect();
                        //System.Diagnostics.Trace.WriteLine(string.Format("rect={0}", r));
                        if (r.Width > 50 && r.Height > 50 && r.X >= 0 && r.Y >= 0)
                        {

                            //System.Diagnostics.Trace.WriteLine(string.Format("[{1}]: rect={0}", r, i));
                            ret.Add(r);
                            //CvInvoke.Rectangle(diff, rr.MinAreaRect(), new MCvScalar(255, 255, 0, 0));
                            /*
                            using (VectorOfPoint contour = contours[i])
                            using (VectorOfPoint approxContour = new VectorOfPoint())
                            {
                                CvInvoke.ApproxPolyDP(contour, approxContour, CvInvoke.ArcLength(contour, true) * 0.05, false);
                            }
                            */
                        }
                    }
                }
                //diff.Save("temp_3.jpg");
            }
            if (ret.Count > 2)
            {
                ret = new List<Rectangle>(MachineLearning.blue_block_filter(ret.ToArray()));
            }
            foreach(var r in ret)
                System.Diagnostics.Trace.WriteLine(string.Format("rect={0}", r));
            return ret.ToArray();
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

        public static bool is_same_image(Bitmap f1, Bitmap f2)
        {
            bool ret = false;
            if (f1 != null && f2 != null)
            {
                if (f1.Width == f2.Width && f1.Height == f2.Height)
                {
                    Subtract sf = new Subtract(f1);
                    Bitmap r = sf.Apply(f2);
                    ret = CheckEmptyImageByML.getInstance().isImageEmpty(r);
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
    }
}
