using System;
using System.IO;

namespace Portal.Helpers
{
    public class Logger
    {
        public void log(string Message)
        {
            try
            {
                StreamWriter errsr = new StreamWriter(System.AppDomain.CurrentDomain.BaseDirectory + "\\" + DateTime.Now.ToString("yyyyMM") + "\\err" + DateTime.Now.ToString("yyyyMMdd") + ".txt", true);
                errsr.WriteLine(DateTime.Now.ToString() + " : " + Message);
                errsr.Close();
                Console.WriteLine(DateTime.Now.ToString() + " : " + Message);
            }
            catch (Exception m)
            {
                try
                {
                    if (!Directory.Exists(System.AppDomain.CurrentDomain.BaseDirectory + "\\" + DateTime.Now.ToString("yyyyMM")))
                        Directory.CreateDirectory(System.AppDomain.CurrentDomain.BaseDirectory + "\\" + DateTime.Now.ToString("yyyyMM"));
                    StreamWriter errsr = new StreamWriter(System.AppDomain.CurrentDomain.BaseDirectory + "\\" + DateTime.Now.ToString("yyyyMM") + "\\err" + DateTime.Now.ToString("yyyyMMdd") + ".txt", true);
                    errsr.WriteLine(DateTime.Now.ToString() + " : " + Message);
                    errsr.Close();
                    Console.WriteLine(DateTime.Now.ToString() + " : " + Message);
                }
                catch (Exception ex2)
                {
                    StreamWriter errsr = new StreamWriter(System.AppDomain.CurrentDomain.BaseDirectory + "\\GeneralError" + DateTime.Now.ToString("yyyyMMdd") + ".txt", true);
                    errsr.WriteLine(DateTime.Now.ToString() + "unbable to create directory / write to file -- " + ex2.Message + " : " + ex2.StackTrace);
                    errsr.Close();
                    Console.WriteLine(DateTime.Now.ToString() + "unbable to create directory / write to file -- " + ex2.Message + " : " + ex2.StackTrace);
                }
            }
        }
    }

}
