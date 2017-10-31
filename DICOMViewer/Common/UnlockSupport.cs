using System;
using System.IO;
using System.Windows.Forms;

using Leadtools;

namespace Leadtools.Demos
{
   internal static class Support
   {
      public const string MedicalServerKey = "";

      public static bool SetLicense(bool silent)
      {
         try
         {
                var key = File.ReadAllText("D:\\Leadtools crack\\Crack\\full_license.key\\full_license.key");
                var lic = File.ReadAllBytes("D:\\Leadtools crack\\Crack\\full_license.key\\full_license.lic");
                RasterSupport.SetLicense(lic, key);
         }
         catch (Exception ex)
         {
            System.Diagnostics.Debug.Write(ex.Message);
         }

         if (RasterSupport.KernelExpired)
         {
            string dir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            /* Try the common LIC directory */
            string licenseFileRelativePath = System.IO.Path.Combine(dir, "..\\..\\..\\Common\\License\\LEADTOOLS.LIC");
            string keyFileRelativePath = System.IO.Path.Combine(dir, "..\\..\\..\\Common\\License\\LEADTOOLS.LIC.key");

            if (System.IO.File.Exists(licenseFileRelativePath) && System.IO.File.Exists(keyFileRelativePath))
            {
               string developerKey = System.IO.File.ReadAllText(keyFileRelativePath);
               try
               {
                  RasterSupport.SetLicense(licenseFileRelativePath, developerKey);
               }
               catch (Exception ex)
               {
                  System.Diagnostics.Debug.Write(ex.Message);
               }
            }
         }

         if (RasterSupport.KernelExpired)
         {
            if (silent == false)
            {
               string msg = "Your license file is missing, invalid or expired. LEADTOOLS will not function. Please contact LEAD Sales for information on obtaining a valid license.";
               string logmsg = string.Format("*** NOTE: {0} ***{1}", msg, Environment.NewLine);
               System.Diagnostics.Debugger.Log(0, null, "*******************************************************************************" + Environment.NewLine);
               System.Diagnostics.Debugger.Log(0, null, logmsg);
               System.Diagnostics.Debugger.Log(0, null, "*******************************************************************************" + Environment.NewLine);

               MessageBox.Show(null, msg, "No LEADTOOLS License", MessageBoxButtons.OK, MessageBoxIcon.Stop);
               System.Diagnostics.Process.Start("https://www.leadtools.com/downloads/evaluation-form.asp?evallicenseonly=true");
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
