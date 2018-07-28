using Accord.Imaging;
using Accord.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace testMQ
{
    class ImUtility
    {
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
