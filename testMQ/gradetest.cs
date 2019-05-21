using Emgu.CV;
using Emgu.CV.ML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testMQ
{
    class gradetest
    {
        static string[] grade_level = new string[] { "A+", "A", "B", "C", "D+", "D" };

        static void Main(string[] args)
        {
            System.Configuration.Install.InstallContext _args = new System.Configuration.Install.InstallContext(null, args);
            if (_args.IsParameterTrue("debug"))
            {
                System.Console.WriteLine("Wait for debugger, press any key to continue");
                System.Console.ReadKey();
            }
            if (_args.IsParameterTrue("train"))
            {
                if (_args.Parameters.ContainsKey("file") && System.IO.File.Exists(_args.Parameters["file"]))
                {
                    //train(_args.Parameters["file"]);
                    train_knn(_args.Parameters["file"]);
                    test(@"C:\projects\local\GradeChecker\GradeChecker\bin\Debug\test_data.json");
                }
            }
            else
            {
                //train();
                test(@"C:\projects\local\GradeChecker\GradeChecker\bin\Debug\test_data.json");
            }
        }
        static void test(string fn)
        {
            //string fn = @"C:\projects\local\GradeChecker\GradeChecker\bin\Debug\test.json";

            try
            {
                var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                List<Dictionary<string, object>> datas = jss.Deserialize<List<Dictionary<string, object>>>(System.IO.File.ReadAllText(fn));
                string[] keys = testMQ.Properties.Resources.keys.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                Matrix<float> data;
                Matrix<int> response;
                load_data(datas.ToArray(), keys, out data, out response);
                using (SVM model = new SVM())
                {
                    model.Load("svm.xml");
                    for(int i = 0; i < data.Rows; i++)
                    {
                        float r = model.Predict(data.GetRow(i));
                        Dictionary<string, object> d = datas[i];
                        System.Console.WriteLine($"imei={d["imei"]}, VZW={d["VZW"]}, FD={grade_level[(int)r]}");
                    }
                }
            }
            catch (Exception) { }
        }
        static void test_knn(string fn)
        {
            //string fn = @"C:\projects\local\GradeChecker\GradeChecker\bin\Debug\test.json";

            try
            {
                var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                List<Dictionary<string, object>> datas = jss.Deserialize<List<Dictionary<string, object>>>(System.IO.File.ReadAllText(fn));
                string[] keys = testMQ.Properties.Resources.keys.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                Matrix<float> data;
                Matrix<int> response;
                load_data(datas.ToArray(), keys, out data, out response);
                using (KNearest model = new KNearest())
                {
                    model.Load("knn.xml");
                    for (int i = 0; i < data.Rows; i++)
                    {
                        float r = model.Predict(data.GetRow(i));
                        Dictionary<string, object> d = datas[i];
                        System.Console.WriteLine($"imei={d["imei"]}, VZW={d["VZW"]}, FD={grade_level[(int)r]}");
                    }
                }
            }
            catch (Exception) { }
        }
        static void train_knn(string fn)
        {
            //string fn = @"C:\projects\local\GradeChecker\GradeChecker\bin\Debug\report.json";
            try
            {
                var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                List<Dictionary<string, object>> datas = jss.Deserialize<List<Dictionary<string, object>>>(System.IO.File.ReadAllText(fn));
                string[] keys = testMQ.Properties.Resources.keys.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                Matrix<float> data;
                Matrix<int> response;
                load_data(datas.ToArray(), keys, out data, out response);
                using (KNearest knn = new KNearest())
                {

                    //SVMParams p = new SVMParams();
                    //p.KernelType = Emgu.CV.ML.MlEnum.SVM_KERNEL_TYPE.LINEAR;
                    //p.SVMType = Emgu.CV.ML.MlEnum.SVM_TYPE.C_SVC;
                    //p.C = 1;
                    //p.TermCrit = new MCvTermCriteria(100, 0.00001);
                    //TrainData td = new TrainData(data, Emgu.CV.ML.MlEnum.DataLayoutType.RowSample, response);
                    //bool ok = model.TrainAuto(td, 3);
                    bool ok = knn.Train(data, Emgu.CV.ML.MlEnum.DataLayoutType.RowSample, response);
                    if (ok)
                    {
                        knn.Save("knn.xml");
                        Matrix<float> sample;
                        load_test_data(datas.ToArray(), keys, out sample);
                        float r = knn.Predict(sample);
                    }
                }

            }
            catch (Exception) { }
        }
        static void train(string fn)
        {
            //string fn = @"C:\projects\local\GradeChecker\GradeChecker\bin\Debug\report.json";
            try
            {
                var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                List<Dictionary<string, object>> datas = jss.Deserialize<List<Dictionary<string, object>>>(System.IO.File.ReadAllText(fn));
                string[] keys = testMQ.Properties.Resources.keys.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                Matrix<float> data;
                Matrix<int> response;
                load_data(datas.ToArray(), keys, out data, out response);
                using (SVM model = new SVM())
                {
                    //model.KernelType = SVM.SvmKernelType.Linear;
                    model.SetKernel(SVM.SvmKernelType.Inter);
                    model.Type = SVM.SvmType.CSvc;
                    model.C = 1;
                    model.TermCriteria = new Emgu.CV.Structure.MCvTermCriteria(100, 0.00001);

                    //SVMParams p = new SVMParams();
                    //p.KernelType = Emgu.CV.ML.MlEnum.SVM_KERNEL_TYPE.LINEAR;
                    //p.SVMType = Emgu.CV.ML.MlEnum.SVM_TYPE.C_SVC;
                    //p.C = 1;
                    //p.TermCrit = new MCvTermCriteria(100, 0.00001);
                    TrainData td = new TrainData(data, Emgu.CV.ML.MlEnum.DataLayoutType.RowSample, response);
                    bool ok = model.TrainAuto(td, 3);
                    //bool ok = model.Train(data, Emgu.CV.ML.MlEnum.DataLayoutType.RowSample, response);
                    if (ok)
                    {
                        model.Save("svm.xml");
                        Matrix<float> sample;
                        load_test_data(datas.ToArray(), keys, out sample);
                        float r = model.Predict(sample);
                    }
                }

            }
            catch (Exception) { }
        }
        static private void load_data(Dictionary<string, object>[] records, string[] keys, out Matrix<float> data, out Matrix<int> response)
        {
            int rows = records.Length;
            int cols = keys.Length;
            //string[] labels = new string[] { "A+", "A", "B", "C", "D+", "D" };
            int i = 0;
            data = new Matrix<float>(rows, cols);
            response = new Matrix<int>(rows, 1);
            foreach (Dictionary<string, object> r in records)
            {
                string v = r["VZW"].ToString();
                int n = Array.IndexOf(grade_level, v);
                response[i, 0] = n+1;
                int j = 0;
                foreach (string k in keys)
                {
                    v = r[k].ToString();
                    if (Int32.TryParse(v, out n))
                    {
                        data[i, j++] = 1.0f * n;
                    }
                }
                i++;
            }
        }
        static private void load_test_data(Dictionary<string, object>[] records, string[] keys, out Matrix<float> data)
        {
            int cnt = records.Length;
            Dictionary<string, object> r = records[0];
            data = new Matrix<float>(1, keys.Length);
            {
                string v = r["VZW"].ToString();
                int j = 0;
                foreach (string k in keys)
                {
                    v = r[k].ToString();
                    int n;
                    if (Int32.TryParse(v, out n))
                    {
                        data[0, j++] = 1.0f * n;
                    }
                }
            }

        }
    }
}
