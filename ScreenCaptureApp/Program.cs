using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using ScreenCaptureApp.Utilities;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Drawing.Imaging;
using System.IO;
using System.Data;
using System.Threading;
using System.Timers;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace ScreenCaptureApp
{
    static class Program
    {
        public static System.Timers.Timer timer = new System.Timers.Timer();
        private static int interval = Convert.ToInt32(ConfigurationManager.AppSettings["Interval"]);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            AppRegistery.AddOrUpdateRegistryKey();
            AppRegistery.ShowOrHideAppFolder(true);
            //AppRegistery.ShowOrHideAppFromTaskBar();
            //Service1.InsertLogInfo(); 
            Service1.OnStart();

            timer.Elapsed += new System.Timers.ElapsedEventHandler(tmrPoll_Tick);
            timer.Enabled = true;
            timer.Interval = interval * 1000;
            System.Threading.Thread.CurrentThread.Suspend();
        }

        private static void tmrPoll_Tick(object sender, ElapsedEventArgs e)
        {
            Service1.OnStart();
        }
    }


    public static class Service1
    {
        static string LocalCnString = ConfigurationManager.ConnectionStrings["LocalCnString"].ConnectionString;
        static string CloudCnString = ConfigurationManager.ConnectionStrings["LocalCloudString"].ConnectionString;
        //static string LocalServerString = ConfigurationManager.ConnectionStrings["LocalServer"].ConnectionString;
        //static string CloudServerString = ConfigurationManager.ConnectionStrings["CloudServer"].ConnectionString;
        static string ImageLocalPath = ConfigurationManager.AppSettings["ImageLocalPath"];

        static DBConnect objLocal;
        static DBConnect objCloud;
        //static DBConnect objLocalServer;
        //static DBConnect objCloudServer;

        static string MacId = GetSystemInfo();
        static bool IsInternetConnected = false;

        static Service1()
        {
            objLocal = new DBConnect(LocalCnString);
            objCloud = new DBConnect(CloudCnString);
            //objLocalServer = new DBConnect(LocalServerString);
            //objCloudServer = new DBConnect(CloudServerString);

            IsInternetConnected = CheckNet();
        }
        public static void OnStart()
        {
            try
            {
                CaptureActiveScreen();

                //CheckDatabaseExistence();
            }
            catch (Exception e)
            {
                ErrorLog.WriteToFile("Exception while starting service time.......");
                ErrorLog.ErrorLogging(e);
                //write log informations
            }
        }

        public static void OnStop()
        {
            try
            {
                if (IsInternetConnected)
                {
                    objCloud.InsertLogInfo(DateTime.Now, MacId, 0);
                }
                else
                {
                    objLocal.InsertLogInfo(DateTime.Now, MacId, 0);
                }
            }
            catch (Exception e)
            {

                ErrorLog.WriteToFile("Exception while Stoping service time.......");
                ErrorLog.ErrorLogging(e);
                //write log informations
            }
        }

        public static void CaptureActiveScreen()
        {
            try
            {
                IsInternetConnected = CheckNet();

                if (IsInternetConnected)
                {
                    var result = UploadDataOnCloud();
                }
                ScreenCapture obj = new ScreenCapture();
                Image image = obj.CaptureScreen();
                string ImageName = DateTime.Now.ToString("yyyyMMddHHmmss"); // case sensitive  
                byte[] ImageStrem = Service1.ImageToByteArray(image);

                if (IsInternetConnected)
                {
                    objCloud.InsertImage(DateTime.Now, MacId, ImageName, "0", ImageStrem);
                }
                else
                {
                    objLocal.InsertImage(DateTime.Now, MacId, ImageName, "0", ImageStrem);
                    //SaveImageLocal(ImageLocalPath, ImageName, image);
                }
            }
            catch (Exception e)
            {

                ErrorLog.WriteToFile("Exception while CaptureActiveScreen.......");
                ErrorLog.ErrorLogging(e);
                //write log informations
            }
        }

        [System.Runtime.InteropServices.DllImport("wininet.dll")]
        public extern static bool InternetGetConnectedState(out int Description, int ReservedValue);

        public static bool CheckNet()
        {
            try
            {
                int desc;
                return InternetGetConnectedState(out desc, 0);
            }
            catch (Exception e)
            {

                ErrorLog.WriteToFile("Exception while CheckNet.......");
                ErrorLog.ErrorLogging(e);
                return false;
                //write log informations
            }
        }

        public static bool CheckForInternetConnection()
        {
            try
            {
                Ping myPing = new Ping();
                String host = "google.com";
                byte[] buffer = new byte[32];
                int timeout = 3000;
                PingOptions pingOptions = new PingOptions();
                PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
                return (reply.Status == IPStatus.Success);
            }
            catch (Exception e)
            {

                ErrorLog.WriteToFile("Exception while CheckForInternetConnection.......");
                ErrorLog.ErrorLogging(e);
                return false;
                //write log informations
            }
        }

        public static void OpenConnectionServer()
        {
            try
            {

                TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 80);

                server.Start();
                Console.WriteLine("Server has started on 115.124.109.110:8080.{0}Waiting for a connection...", Environment.NewLine);

                TcpClient client = server.AcceptTcpClient();

                Console.WriteLine("A client connected.");

            }
            catch (Exception e)
            {

                ErrorLog.WriteToFile("Exception while OpenConnectionServer.......");
                ErrorLog.ErrorLogging(e);
                //write log informations
            }
        }

        public static string GetSystemInfo()
        {
            string result = string.Empty;
            try
            {
                //string MachineName = System.Environment.MachineName;
                string dnsName = System.Net.Dns.GetHostName();

                var macAddr =
                  (
                      from nic in NetworkInterface.GetAllNetworkInterfaces()
                      where nic.OperationalStatus == OperationalStatus.Up
                      select nic.GetPhysicalAddress().ToString()
                  ).FirstOrDefault();

                result = macAddr;

                #region get all system level informations.
                //string[] queryItems = { "Win32_ComputerSystem", "b.Win32_DiskDrive", "c.Win32_OperatingSystem", "d.Win32_Processor", "e.Win32_ProgramGroup", "f.Win32_SystemDevices", "g.Win32_StartupCommand" };
                //ManagementObjectSearcher searcher;
                //int i = 0;
                //ArrayList arrayListInformationCollactor = new ArrayList();
                //try
                //{
                //    searcher = new ManagementObjectSearcher("SELECT * FROM " + queryItems[0]);
                //    foreach (ManagementObject mo in searcher.Get())
                //    {
                //        i++;
                //        PropertyDataCollection searcherProperties = mo.Properties;
                //        foreach (PropertyData sp in searcherProperties)
                //        {
                //            arrayListInformationCollactor.Add(sp);
                //        }
                //    }
                //}
                //catch (Exception ex)
                //{
                //}  
                #endregion
            }
            catch (Exception e)
            {

                ErrorLog.WriteToFile("Exception while GetSystemInfo.......");
                ErrorLog.ErrorLogging(e);
                //write log informations
            }
            return result;
        }

        public static byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    imageIn.Save(ms, ImageFormat.Png);
                    return ms.ToArray();
                }
            }
            catch (Exception e)
            {

                ErrorLog.WriteToFile("Exception while ImageToByteArray.......");
                ErrorLog.ErrorLogging(e);
                return null;
                //write log informations
            }
        }

        public static Image byteArrayToImage(byte[] byteArrayIn)
        {
            try
            {
                MemoryStream ms = new MemoryStream(byteArrayIn);
                Image returnImage = Image.FromStream(ms);
                return returnImage;
            }
            catch (Exception e)
            {

                ErrorLog.WriteToFile("Exception while byteArrayToImage.......");
                ErrorLog.ErrorLogging(e);
                return null;
                //write log informations
            }
        }

        public static async Task<bool> UploadDataOnCloud()
        {
            try
            {
                string SelectQuery = "select * from tbltoposcreens";
                DataTable dtRows = objLocal.Select(SelectQuery);

                string DeleteQuery = "delete from tbltoposcreens";
                objLocal.Delete(DeleteQuery);

                List<string> Rows = new List<string>();
                for (int i = 0; i < dtRows.Rows.Count; i++)
                {
                    objCloud.InsertImage(Convert.ToDateTime(dtRows.Rows[i]["fldDateTime"]), Convert.ToString(dtRows.Rows[i]["fldMacID"]), Convert.ToString(dtRows.Rows[i]["fldScreenshot"]), Convert.ToString(dtRows.Rows[i]["fldScreenShotProcessedYesNo"]), (byte[])dtRows.Rows[0]["fldScreenBlob"]);
                    await Task.Delay(TimeSpan.FromSeconds(3));
                }

                return true;
            }
            catch (Exception e)
            {
                ErrorLog.WriteToFile("Exception while byteArrayToImage.......");
                ErrorLog.ErrorLogging(e);
                return false;
                //write log informations
            }
        }

        public static bool SaveImageLocal(string ImageLocalPath, string ImageName, Image Screen)
        {
            bool result = false;
            try
            {
                if (!Directory.Exists(ImageLocalPath))
                {
                    Directory.CreateDirectory(ImageLocalPath);
                }
                Screen.Save(ImageLocalPath + ImageName + ".png", ImageFormat.Png);
            }
            catch (Exception e)
            {
                ErrorLog.WriteToFile("Exception while SaveImageLocal.......");
                ErrorLog.ErrorLogging(e);
                return false;
                //write log informations
            }
            return result;
        }

        public static void InsertLogInfo()
        {
            try
            {
                ErrorLog.WriteToFile("Service started time.......");
                if (IsInternetConnected)
                {
                    objCloud.InsertLogInfo(DateTime.Now, MacId, 1);
                }
                else
                {
                    objLocal.InsertLogInfo(DateTime.Now, MacId, 1);
                }
            }
            catch (Exception e)
            {
                ErrorLog.WriteToFile("Exception while starting service time.......");
                ErrorLog.ErrorLogging(e);
                ErrorLog.WriteToFile("Service started time.......");
                if (IsInternetConnected)
                {
                    objCloud.InsertLogInfo(DateTime.Now, MacId, 0);
                }
                else
                {
                    objLocal.InsertLogInfo(DateTime.Now, MacId, 0);
                }
                //write log informations
            }
        }

        public static void CheckDatabaseExistence()
        {
            try
            {
                if (IsInternetConnected)
                {
                    //objCloudServer.CreateCloudDatabaseProgramatically(CloudServerString);
                }
                //objLocalServer.CreateLocalDatabaseProgramatically(LocalServerString);
            }
            catch (Exception e)
            {
                ErrorLog.WriteToFile("Exception while CheckDatabaseExistence.......");
                ErrorLog.ErrorLogging(e);
                //write log informations
            }
        }

    }
    public static class AppRegistery
    {
        // The path to the key where Windows looks for startup applications
        static RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        static bool IsRegistered = true;
        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        public static bool AddOrUpdateRegistryKey()
        {
            bool result = false;
            try
            {
                var value = registryKey.GetValue("ScreenCaptureService");
                if (IsRegistered)
                {
                    if (value == null)
                    {
                        registryKey.SetValue("ScreenCaptureService", Application.ExecutablePath);
                    }
                    else
                    {
                        registryKey.SetValue("ScreenCaptureService", Application.ExecutablePath);
                    }
                    result = true;
                }
                else
                {
                    registryKey.DeleteValue("ScreenCaptureService");
                    result = false;
                }
            }
            catch (Exception e)
            {
                ErrorLog.WriteToFile("Exception while ShowOrHideAppFolder.......");
                ErrorLog.ErrorLogging(e);
                //write log informations
            }
            return result;
        }

        public static bool ShowOrHideAppFolder(bool value)
        {
            bool result = false;
            try
            {
                if (value)
                {
                    DirectoryInfo Folder = new DirectoryInfo(Application.StartupPath);
                    Folder.Attributes = FileAttributes.Hidden;
                }
                else
                {
                    DirectoryInfo Folder = new DirectoryInfo(Application.StartupPath);
                    Folder.Attributes = FileAttributes.Normal;
                }
                result = true;
            }
            catch (Exception e)
            {
                ErrorLog.WriteToFile("Exception ShowOrHideAppFolder.......");
                ErrorLog.ErrorLogging(e);
                //write log informations
            }
            return result;
        }

        public static bool ShowOrHideAppFromTaskBar()
        {
            bool result = false;
            try
            {
                IntPtr hWnd;
                Process[] processRunning = Process.GetProcesses();
                foreach (Process pr in processRunning)
                {
                    if (pr.ProcessName == "ScreenCaptureApp")
                    {
                        hWnd = pr.MainWindowHandle;
                        ShowWindow(hWnd, SW_HIDE);
                    }
                }
                result = true;
            }
            catch (Exception e)
            {
                ErrorLog.WriteToFile("Exception while ShowOrHideAppFromTaskBar.......");
                ErrorLog.ErrorLogging(e);
                //write log informations
            }
            return result;
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }

}
