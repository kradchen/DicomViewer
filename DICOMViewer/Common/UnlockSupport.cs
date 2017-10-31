using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Leadtools.Demos
{
    static class Support
    {
        public const string MedicalServerKey = "";

        public static bool SetLicense(bool silent)
        {
            try
            {
                //不读文件，改为直接写入字符串
                //var key = File.ReadAllText(Application.StartupPath+"\\full_license.key");
                string key =
                    "gcxLXptTi5bPbVDDR9E+k4CRC8BLLm0IuN383qJp6jPqoMTYamOe1yuYzHqrCmFEN5zDcumaaCTXpO9GpeGal0wjSKF8nxnu";
                var lic = File.ReadAllBytes(Application.StartupPath + "\\fl.lic");
                RasterSupport.SetLicense(lic, key);
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
            }

            if (RasterSupport.KernelExpired)
            {
                string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                /* Try the common LIC directory */
                string licenseFileRelativePath = Path.Combine(dir, "..\\..\\..\\Common\\License\\LEADTOOLS.LIC");
                string keyFileRelativePath = Path.Combine(dir, "..\\..\\..\\Common\\License\\LEADTOOLS.LIC.key");

                if (File.Exists(licenseFileRelativePath) && File.Exists(keyFileRelativePath))
                {
                    string developerKey = File.ReadAllText(keyFileRelativePath);
                    try
                    {
                        RasterSupport.SetLicense(licenseFileRelativePath, developerKey);
                    }
                    catch (Exception ex)
                    {
                        Debug.Write(ex.Message);
                    }
                }
            }

            if (RasterSupport.KernelExpired)
            {
                if (silent == false)
                {
                    string msg =
                        "Your license file is missing, invalid or expired. LEADTOOLS will not function. Please contact LEAD Sales for information on obtaining a valid license.";
                    string logmsg = string.Format("*** NOTE: {0} ***{1}", msg, Environment.NewLine);
                    Debugger.Log(0, null,
                        "*******************************************************************************" +
                        Environment.NewLine);
                    Debugger.Log(0, null, logmsg);
                    Debugger.Log(0, null,
                        "*******************************************************************************" +
                        Environment.NewLine);

                    MessageBox.Show(null, msg, "No LEADTOOLS License", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    Process.Start("https://www.leadtools.com/downloads/evaluation-form.asp?evallicenseonly=true");
                }

                return false;
            }
            return true;
        }

        public static bool SetLicense()
        {
            return SetLicense(false);
        }
    }
}