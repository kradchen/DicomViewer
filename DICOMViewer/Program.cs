using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Leadtools;
using Leadtools.Demos;
using Leadtools.DicomDemos;

namespace DICOMViewer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            //设置License
            if (!Support.SetLicense())
                return;
            //leadtools验证liccense
            if (RasterSupport.IsLocked(RasterSupportType.DicomCommunication))
            {
                MessageBox.Show(String.Format("{0} Support is locked!", RasterSupportType.DicomCommunication.ToString()), "Warning");
                return;
            }
            //对Uac进行判断，如果需要提升权限进行运行，已通过manifest设置requireAdministrator绕过
            if (DemosGlobal.MustRestartElevated())
            {
                DemosGlobal.TryRestartElevated(args);
                return;
            }
            //启动leadtools引擎，DicomEngine.Startup();
            Utils.EngineStartup();
            //DicomNet.Startup();
            Utils.DicomNetStartup();

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(true);
                //运行主界面
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Utils.EngineShutdown();
                Utils.DicomNetShutdown();
            }
        }
    }
}
