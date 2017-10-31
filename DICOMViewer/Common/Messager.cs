using System;
using System.Windows.Forms;

namespace Leadtools.Demos
{
   public static class Messager
   {
      private static string _fileOpenErrorMessage;
      private static string _fileSaveErrorMessage;

      static Messager()
      {

            _fileOpenErrorMessage = "Error opening file:";
            _fileSaveErrorMessage = "Error saving file:";
      }

      private static string _caption;

      public static string Caption
      {
         get
         {
            return _caption;
         }

         set
         {
            _caption = value;
         }
      }

      public static void ShowError(IWin32Window owner, Exception ex)
      {
         string message = ex.Message;
         message = CheckIfSupportError(ex, false);
         Show(owner, message, MessageBoxIcon.Exclamation, MessageBoxButtons.OK);
      }

      public static void ShowError(IWin32Window owner, string message)
      {
         Show(owner, message, MessageBoxIcon.Exclamation, MessageBoxButtons.OK);
      }

      public static void ShowWarning(IWin32Window owner, Exception ex)
      {
         string message = ex.Message;
         message = CheckIfSupportError(ex, false);
         Show(owner, message, MessageBoxIcon.Exclamation, MessageBoxButtons.OK);
      }

      public static void ShowWarning(IWin32Window owner, string message)
      {
         Show(owner, message, MessageBoxIcon.Exclamation, MessageBoxButtons.OK);
      }

      public static DialogResult ShowQuestion(IWin32Window owner, string message, MessageBoxButtons buttons)
      {
         return Show(owner, message, MessageBoxIcon.Question, buttons);
      }

      public static DialogResult ShowQuestion(IWin32Window owner, string message, MessageBoxIcon icon, MessageBoxButtons buttons)
      {
         return Show(owner, message, icon, buttons);
      }

      public static void ShowInformation(IWin32Window owner, string message)
      {
         Show(owner, message, MessageBoxIcon.Information, MessageBoxButtons.OK);
      }

      public static void Show(IWin32Window owner, Exception ex, MessageBoxIcon icon)
      {
         string message = ex.Message;
         message = CheckIfSupportError(ex,false);
         Show(owner, message, icon, MessageBoxButtons.OK);
      }

      public static void ShowFileOpenError(IWin32Window owner, string fileName, Exception ex)
      {
         string message = ex.Message;
         message = CheckIfSupportError(ex,false);
         if(fileName != null && fileName != string.Empty)
         {
            if(ex != null)
               Show(owner, string.Format("{0}{2}{1}{2}{2}{3}", _fileOpenErrorMessage, fileName, Environment.NewLine, message), MessageBoxIcon.Exclamation, MessageBoxButtons.OK);
            else
               Show(owner, string.Format("{0}{1}", _fileOpenErrorMessage, fileName), MessageBoxIcon.Exclamation, MessageBoxButtons.OK);
         }
         else
         {
            if(ex != null)
               Show(owner, string.Format("{0}{1}{1}{2}", _fileOpenErrorMessage, Environment.NewLine, message), MessageBoxIcon.Exclamation, MessageBoxButtons.OK);
            else
               Show(owner, _fileOpenErrorMessage, MessageBoxIcon.Exclamation, MessageBoxButtons.OK);
         }
      }

      public static void ShowFileSaveError(IWin32Window owner, string fileName, Exception ex)
      {
         string message = ex.Message;
         message = CheckIfSupportError(ex, false);
         if(fileName != null && fileName != string.Empty)
         {
            if(ex != null)
               ShowError(owner, string.Format("{0}{2}{1}{2}{2}{3}", _fileSaveErrorMessage, fileName, Environment.NewLine, message));
            else
               ShowError(owner, string.Format("{0}{1}", _fileSaveErrorMessage, fileName));
         }
         else
         {
            if(ex != null)
               ShowError(owner, string.Format("{0}{1}{1}{2}", _fileSaveErrorMessage, Environment.NewLine, message));
            else
               ShowError(owner, _fileSaveErrorMessage);
         }
      }

      public static string CheckIfSupportError(Exception ex, bool ignoreIfNotSupportError)
      {
         string message = ex.Message;

         if (ignoreIfNotSupportError)
            return message;

         RasterException rex = ex as RasterException;
         if (rex != null)
         {
            if (rex.Code == RasterExceptionCode.FeatureNotSupported | rex.Code == RasterExceptionCode.DicomNotEnabled |
               rex.Code == RasterExceptionCode.DocumentNotEnabled | rex.Code == RasterExceptionCode.MedicalNotEnabled |
               /*rex.Code == RasterExceptionCode.ExtGrayNotEnabled | */
               rex.Code == RasterExceptionCode.ProNotEnabled |
               rex.Code == RasterExceptionCode.LzwLocked | rex.Code == RasterExceptionCode.JbigNotEnabled |
               rex.Code == RasterExceptionCode.Jbig2Locked | rex.Code == RasterExceptionCode.J2kLocked |
               rex.Code == RasterExceptionCode.PdfNotEnabled | rex.Code == RasterExceptionCode.CmwLocked |
               rex.Code == RasterExceptionCode.AbcLocked | rex.Code == RasterExceptionCode.MedicalNetNotEnabled |
               rex.Code == RasterExceptionCode.NitfLocked | rex.Code == RasterExceptionCode.JpipLocked |
               rex.Code == RasterExceptionCode.FormsLocked | rex.Code == RasterExceptionCode.DocumentWritersNotEnabled |
               rex.Code == RasterExceptionCode.MediaWriterNotEnabled | rex.Code == RasterExceptionCode.DocumentWritersPdfNotEnabled |
               rex.Code == RasterExceptionCode.LeadPrinterNotEnabled | rex.Code == RasterExceptionCode.LeadPrinterServerNotEnabled |
               rex.Code == RasterExceptionCode.LeadPrinterNetworkNotEnabled | rex.Code == RasterExceptionCode.AppStoreNotEnabled |
               rex.Code == RasterExceptionCode.BasicNotEnabled | rex.Code == RasterExceptionCode.NoServerLicense)
            {
               message = string.Format("Your runtime license does not include support for this functionality.\n* {0}\nPlease contact sales@leadtools.com to discuss options to include this feature set in your runtime license.", rex.Message);
            }
         }
         return message;
      }

      public static DialogResult Show(IWin32Window owner, string message, MessageBoxIcon icon, MessageBoxButtons buttons)
      {
         return MessageBox.Show(owner, message, Caption, buttons, icon);
      }
   }
}
