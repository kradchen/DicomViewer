using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;
using Leadtools.Codecs;
using Microsoft.Win32;

namespace Leadtools.Demos
{
    public static class DemosGlobal
    {
        static DemosGlobal()
        {
            var rk = OpenSoftwareKey(@"Microsoft\InetStp");

            if (rk != null)
            {
                var major = rk.GetValue("MajorVersion");
                var minor = rk.GetValue("MinorVersion");

                rk.Close();

                if (major != null && minor != null)
                {
                    IISMajorVersionNumber = (int) major;
                    IISVersion = major + "." + minor;
                }
                else
                {
                    IISMajorVersionNumber = 0;
                }
            }
        }

        #region 没啥用的版本判断值
        public static bool IsOnXP
        {
            get { return Environment.OSVersion.Version.Major == 5; }
        }

        public static bool IsOnVista
        {
            get { return Environment.OSVersion.Version.Major >= 6; }
        }

        public static bool IsOnWindows7
        {
            get
            {
                return (Environment.OSVersion.Version.Major > 6) ||
                       (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 1);
            }
        }

        public static bool IsOnWindows2003
        {
            get { return Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor == 2; }
        }

        public static bool IsOnWindows2000
        {
            get { return Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor == 0; }
        }

        public static bool IsOnVistaOrLater
        {
            get
            {
                var OS = Environment.OSVersion;
                return (OS.Platform == PlatformID.Win32NT) && (OS.Version.Major >= 6);
            }
        }
        #endregion


        public static string ImagesFolder
        {
            get
            {
                string imagesPath;

                // Check if %PUBLIC%\Documents\LEADTOOLS Images exits first
                try
                {
                    imagesPath = Path.Combine(GetCommonDocumentsFolder(), @"LEADTOOLS Images");
                    if (Directory.Exists(imagesPath))
                    {
                        return imagesPath;
                    }
                }
                catch
                {
                }

                // Try registry next
#if LTV19_CONFIG
                imagesPath = @"Software\LEAD Technologies, Inc.\19\Images";
                var unicodeImagesPath = @"Software\LEAD Technologies, Inc.\19\UnicodeImages";
#elif LTV18_CONFIG
            imagesPath = @"Software\LEAD Technologies, Inc.\18\Images";
            string unicodeImagesPath = @"Software\LEAD Technologies, Inc.\18\UnicodeImages";
#endif
                var rk = Registry.LocalMachine.OpenSubKey(imagesPath);
                if (rk == null)
                    rk = Registry.LocalMachine.OpenSubKey(unicodeImagesPath);
                if (rk == null)
                    rk = Registry.LocalMachine.OpenSubKey(imagesPath);
                if (rk == null)
                    rk = Registry.LocalMachine.OpenSubKey(unicodeImagesPath);
                if (rk != null)
                {
                    var value = rk.GetValue(null) as string;
                    rk.Close();
                    return value;
                }

                // Finally, use the current EXE path
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }

        public static string InstallLocation
        {
            get
            {
                var location = string.Empty;
                var regKey = string.Empty;
                var englishInstallGuid = @"{1111511B-A89A-4907-A9D4-BB302F744CDB}";
                var japaneseInstallGuid = @"{1111511C-A89A-4907-A9D4-BB302F744CDB}";

                // Try English setup first
                if (Is64Process())
                    regKey = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\" + englishInstallGuid;
                else
                    regKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\" + englishInstallGuid;
                location = ReadInstallLocationFromRegistry(regKey);

                // If not English setup, try Japanese
                if (string.IsNullOrEmpty(location))
                {
                    if (Is64Process())
                        regKey = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\" +
                                 japaneseInstallGuid;
                    else
                        regKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\" + japaneseInstallGuid;
                    location = ReadInstallLocationFromRegistry(regKey);
                }

                // If both Japanese and English setup missing, use startup path.
                if (string.IsNullOrEmpty(location))
                {
                    location = Application.StartupPath;
                }
                return location;
            }
        }

        public static int IISMajorVersionNumber { get; } = -1;

        public static string IISVersion { get; } = string.Empty;


        public static bool Is64Process()
        {
            return IntPtr.Size == 8;
        }

        public static bool NeedUAC()
        {
            var system = Environment.OSVersion;

            if (system.Platform == PlatformID.Win32NT && system.Version.Major >= 6)
                return true;

            return false;
        }

        public static bool IsAdmin()
        {
            var id = WindowsIdentity.GetCurrent();
            var p = new WindowsPrincipal(id);

            var flag = p.IsInRole(WindowsBuiltInRole.Administrator);
            return flag;
        }

        public static bool MustRestartElevated()
        {
            return NeedUAC() &&
                   !IsAdmin();
        }

        public static void RestartElevated(string[] args)
        {
            var startInfo = new ProcessStartInfo();

            startInfo.UseShellExecute = true;
            startInfo.WorkingDirectory = Environment.CurrentDirectory;
            startInfo.FileName = Application.ExecutablePath;
            startInfo.Verb = "runas";
            startInfo.Arguments = string.Join(" ", args);
            try
            {
                var p = Process.Start(startInfo);
            }
            catch (Win32Exception)
            {
            }
        }

        public static void TryRestartElevated(string[] args)
        {
            foreach (var s in args)
            {
                if (string.Compare("/restartElevated", s) == 0)
                {
                    var msg = string.Format("{0}: {1}", "当前程序必须在管理员权限下运行！", Process.GetCurrentProcess().ProcessName);
                    MessageBox.Show(msg, "警告！", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            var argsNew = new string[args.Length + 1];
            Array.Copy(args, argsNew, args.Length);
            argsNew[args.Length] = "/restartElevated";

            RestartElevated(argsNew);
        }

        [SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHGetFolderPath(IntPtr hwndOwner, int nFolder, IntPtr hToken, uint dwFlags,
            [Out] StringBuilder pszPath);

        private static string GetCommonDocumentsFolder()
        {
            var SIDL_COMMON_DOCUMENTS = 0x002E;
            var sb = new StringBuilder(1024);
            SHGetFolderPath(IntPtr.Zero, SIDL_COMMON_DOCUMENTS, IntPtr.Zero, 0, sb);
            return sb.ToString();
        }

        private static string ReadInstallLocationFromRegistry(string regKey)
        {
            if (string.IsNullOrEmpty(regKey))
                return string.Empty;

            using (var key = Registry.LocalMachine.OpenSubKey(regKey))
            {
                if (key == null)
                    return string.Empty;

                var value = key.GetValue("InstallLocation");

                if (value != null)
                    return value.ToString();
            }
            return string.Empty;
        }

        private static string GetInstallLocation(string regKey)
        {
            var location = ReadInstallLocationFromRegistry(regKey);

            if (location == string.Empty)
            {
                location = Application.StartupPath;
            }

            return location;
        }


        public static RegistryKey OpenSoftwareKey(string keyName)
        {
            var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\" + keyName);

            if (key == null)
            {
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\" + keyName);
                if (key == null)
                    key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\" + keyName);
            }

            return key;
        }

        public static bool IsDotNet35Installed()
        {
            var ret = false;
            var rk = OpenSoftwareKey(@"\Microsoft\NET Framework Setup\NDP\v3.5");
            if (rk != null)
            {
                ret = true;
                rk.Close();
            }

            else
                ret = false;
            return ret;
        }

        public static bool IsDotNet4Installed()
        {
            var ret = false;
            var rk = OpenSoftwareKey(@"\Microsoft\NET Framework Setup\NDP\v4");
            if (rk != null)
            {
                ret = true;
                rk.Close();
            }

            else
                ret = false;
            return ret;
        }


        public static RectangleF GetBoundingRectangle(PointF[] pts)
        {
            if (pts.Length == 2)
                return RectangleF.FromLTRB(
                    Math.Min(pts[0].X, pts[1].X),
                    Math.Min(pts[0].Y, pts[1].Y),
                    Math.Max(pts[0].X, pts[1].X),
                    Math.Max(pts[0].Y, pts[1].Y));
            if (pts.Length == 4)
                return RectangleF.FromLTRB(
                    Math.Min(pts[0].X, Math.Min(pts[1].X, Math.Min(pts[2].X, pts[3].X))),
                    Math.Min(pts[0].Y, Math.Min(pts[1].Y, Math.Min(pts[2].Y, pts[3].Y))),
                    Math.Max(pts[0].X, Math.Max(pts[1].X, Math.Max(pts[2].X, pts[3].X))),
                    Math.Max(pts[0].Y, Math.Max(pts[1].Y, Math.Max(pts[2].Y, pts[3].Y))));
            var xMin = pts[0].X;
            var yMin = pts[0].Y;
            var xMax = xMin;
            var yMax = yMin;

            for (var i = 1; i < pts.Length; i++)
            {
                xMin = Math.Min(xMin, pts[i].X);
                yMin = Math.Min(yMin, pts[i].Y);
                xMax = Math.Max(xMax, pts[i].X);
                yMax = Math.Max(yMax, pts[i].Y);
            }

            return FixRectangle(RectangleF.FromLTRB(xMin, yMin, xMax, yMax));
        }

        public static PointF[] GetBoundingPoints(RectangleF rc)
        {
            return new[]
            {
                new PointF(rc.Left, rc.Top),
                new PointF(rc.Right, rc.Top),
                new PointF(rc.Right, rc.Bottom),
                new PointF(rc.Left, rc.Bottom)
            };
        }

        public static Rectangle GetBoundingRectangle(Point center, Size size)
        {
            return new Rectangle(
                center.X - size.Width/2,
                center.Y - size.Height/2,
                size.Width,
                size.Height);
        }

        public static RectangleF GetBoundingRectangle(PointF center, SizeF size)
        {
            return new RectangleF(
                center.X - size.Width/2.0f,
                center.Y - size.Height/2.0f,
                size.Width,
                size.Height);
        }

        public static Rectangle GetBoundingRectangle(RectangleF rc)
        {
            return FixRectangle(new Rectangle(
                (int) rc.Left,
                (int) rc.Top,
                (int) Math.Ceiling(rc.Width) + 1,
                (int) Math.Ceiling(rc.Height) + 1));
        }


        public static RectangleF FixRectangle(RectangleF rc)
        {
            if (rc.Left > rc.Right || rc.Top > rc.Bottom)
                return RectangleF.FromLTRB(
                    Math.Min(rc.Left, rc.Right),
                    Math.Min(rc.Top, rc.Bottom),
                    Math.Max(rc.Left, rc.Right),
                    Math.Max(rc.Top, rc.Bottom));
            return rc;
        }

        public static Rectangle FixRectangle(Rectangle rc)
        {
            if (rc.Left > rc.Right || rc.Top > rc.Bottom)
                return Rectangle.FromLTRB(
                    Math.Min(rc.Left, rc.Right),
                    Math.Min(rc.Top, rc.Bottom),
                    Math.Max(rc.Left, rc.Right),
                    Math.Max(rc.Top, rc.Bottom));
            return rc;
        }

        /// Convert any possible string-Value of a given enumeration
        /// type to its internal representation.
        public static object StringToEnum(Type t, string Value)
        {
            foreach (var fi in t.GetFields())
                if (fi.Name == Value)
                    return fi.GetValue(null); // We use null because
            // enumeration values
            // are static

            throw new Exception(string.Format("Can not convert {0} to {1}", Value, t));
        }

        // Trims off the % of an IPv6 address
        // Sets to lower case
        // Ipv4 addresses are returned unchanged
        // If IpV6 address is of form: ::ffff:192.168.0.xxx, then it strips off the ::ffff:
        public static string CleanIp(string ipAddress)
        {
            var prefix = "::ffff:";
            var ipParts = ipAddress.Split(new[] {'%'}, StringSplitOptions.RemoveEmptyEntries);
            if (ipParts.Length > 0)
                ipAddress = ipParts[0];

            if (ipAddress.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                ipAddress = ipAddress.Substring(prefix.Length);

            ipAddress = ipAddress.ToLower();
            return ipAddress;
        }
    }
}