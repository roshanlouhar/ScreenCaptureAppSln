using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenCaptureApp.Utilities
{
    public static class ErrorLog
    {
        private static string LogFileName { get; set; }
        private static string ImageLocalPath { get; set; }


        static ErrorLog()
        {
            LogFileName = ConfigurationManager.AppSettings["FilePath"];
            ImageLocalPath = ConfigurationManager.AppSettings["ImageLocalPath"];
        }

        public static void ErrorLogging(Exception ex)
        {
            try
            {
                if (!Directory.Exists(ImageLocalPath))
                {
                    Directory.CreateDirectory(ImageLocalPath);
                }
                if (!File.Exists(ImageLocalPath + LogFileName))
                {
                    File.Create(ImageLocalPath + LogFileName).Dispose();
                }
                using (StreamWriter sw = File.AppendText(ImageLocalPath + LogFileName))
                {
                    sw.WriteLine("=============Error Logging ===========");
                    sw.WriteLine("===========Start============= " + DateTime.Now);
                    sw.WriteLine("Error Message: " + ex.Message);
                    sw.WriteLine("Stack Trace: " + ex.StackTrace);
                    sw.WriteLine("===========End============= " + DateTime.Now);
                    sw.WriteLine(System.Environment.NewLine);
                }
            }
            catch
            {
            }
        }

        public static void ReadError()
        {
            try
            {
                using (StreamReader sr = new StreamReader(ImageLocalPath + LogFileName))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        Console.WriteLine(line);
                    }
                }
            }
            catch
            {

            }
        }

        public static void WriteToFile(string text)
        {
            if (!Directory.Exists(ImageLocalPath))
            {
                Directory.CreateDirectory(ImageLocalPath);
            }
            if (!File.Exists(ImageLocalPath + LogFileName))
            {
                File.Create(ImageLocalPath + LogFileName).Dispose();
            }
            using (StreamWriter sw = File.AppendText(ImageLocalPath + LogFileName))
            {
                sw.WriteLine("=============Text Logging ===========");
                sw.WriteLine("===========Start============= " + DateTime.Now);
                sw.WriteLine(string.Format(text, DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")));
                sw.WriteLine("===========End============= " + DateTime.Now);
                sw.WriteLine(System.Environment.NewLine);
            }
        }
    }
}
