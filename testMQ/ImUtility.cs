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
    }
}
