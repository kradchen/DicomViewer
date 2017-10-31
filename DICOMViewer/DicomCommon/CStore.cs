using System;
using System.IO;
using System.Net;
using System.Threading;
using System.ComponentModel;
using System.Collections.Specialized;

using Leadtools;
using Leadtools.Dicom;
using Leadtools.DicomDemos;
using Leadtools.DicomDemos.Scu;

namespace Leadtools.DicomDemos.Scu.CStore
{
   /// <summary>
   /// 
   /// </summary>
   public class ProgressFilesEventArgs : EventArgs
   {
      internal FileInfo _File;

      /// <summary>
      /// The current file that is been worked on.
      /// </summary>
      public FileInfo File
      {
         get
         {
            return _File;
         }
      }
   }

   public delegate void ProgressFilesEventHandler(Object sender, ProgressFilesEventArgs e);

   /// <summary>
   /// Summary description for Class1.
   /// </summary>
   public class CStore : Scu
   {
      private StringCollection _Files = new StringCollection();


      private DicomImageCompressionType _Compression;
      public bool Continue = true;
      public short PresentID;
      string strCompression = "";

      /// <summary>
      /// DICOM Implementation Class UID.
      /// </summary>
      public static string ImplementationClassUid;

      /// <summary>
      /// 
      /// </summary>
      public CStore( )
      {
      }
      #region Secure TLS Communication
      public CStore(string clientPEM, DicomTlsCipherSuiteType tlsCipherSuiteType, DicomTlsCertificateType tlsCertificateType, string privateKeyPassword)
         : base(clientPEM, tlsCipherSuiteType, tlsCertificateType, privateKeyPassword)
      {

      }
      #endregion

      ~CStore( )
      {
         if(workThread != null && workThread.IsAlive)
            workThread.Abort();
      }

      # region Events

      public event ProgressFilesEventHandler ProgressFiles;

      public virtual void OnProgressFiles(ProgressFilesEventArgs e)
      {
         if(ProgressFiles != null)
         {
            // Invokes teh delegates
            ProgressFiles(this, e);
         }
      }

      # endregion

      /// <summary>
      /// Collections of files to send with C-STORE-REQ
      /// </summary>
      public StringCollection Files
      {
         get
         {
            return _Files;
         }
      }

      /// <summary>
      /// Compression
      /// </summary>
      [Category("Compression")]
      public DicomImageCompressionType Compression
      {
         get
         {
            return _Compression;
         }
         set
         {
            _Compression = value;
         }
      }

      public override void Init( )
      {
         base.Init();
      }

      protected override void OnReceiveCStoreResponse(byte presentationID, int messageID, string affectedClass, string instance, DicomCommandStatusType status)
      {
         InvokeStatusEvent(StatusType.ReceiveCStoreResponse, status);
         Continue = true;
         Event.Set();
      }

      protected override void OnReceiveReleaseResponse( )
      {
         InvokeStatusEvent(StatusType.ReceiveReleaseResponse, DicomExceptionCode.Success);
         Close();
         Event.Set();
      }

      protected override void OnReceiveData(byte presentationID, DicomDataSet cs, DicomDataSet ds)
      {
         base.OnReceiveData(presentationID, cs, ds);
         
         if ( null != cs )
         {
            cs.Dispose ( ) ;
         }
      }

      public override PresentationContextCollection GetPresentationContext( )
      {
         PresentationContextCollection pc = new PresentationContextCollection();
         PresentationContext p;
         string TransferSyntax = "";

         if(Compression != DicomImageCompressionType.None)
         {
            switch(Compression)
            {
               case DicomImageCompressionType.J2kLossy:
                  strCompression = DicomUidType.JPEG2000;
                  break;
               case DicomImageCompressionType.J2kLossless:
                  strCompression = DicomUidType.JPEG2000LosslessOnly;
                  break;
               case DicomImageCompressionType.JpegLossy:
                  strCompression = DicomUidType.JPEGBaseline1;
                  break;
               case DicomImageCompressionType.JpegLossless:
                  strCompression = DicomUidType.JPEGLosslessNonhier14;
                  break;
            }
         }

         foreach(string file in Files)
         {
            string StorageClass;
            
            using ( DicomDataSet dcmDS = new DicomDataSet() )
            {
               TransferSyntax = strCompression;
               try
               {
                  dcmDS.Load(file, DicomDataSetLoadFlags.None);
               }
               catch(DicomException de)
               {
                  InvokeStatusEvent(StatusType.Error, de.Code);
                  Terminate();
                  break;
               }


               StorageClass = Utils.GetStringValue(dcmDS, (long)DemoDicomTags.MediaStorageSOPClassUID);
               StorageClass = StorageClass.Trim();
               if(StorageClass.Length == 0)
               {
                  StorageClass = Utils.GetStringValue(dcmDS, (long)DemoDicomTags.SOPClassUID);
                  StorageClass = StorageClass.Trim();
               }
               if(StorageClass.Length == 0)
                  StorageClass = "1.1.1.1";

               p = FindPresentationContext(StorageClass, pc);
               if(p == null)
               {
                  p = new PresentationContext();

                  p.AbstractSyntax = StorageClass;
                  pc.Add(p);
               }

               if(TransferSyntax.Length == 0)
               {
                  TransferSyntax = Utils.GetStringValue(dcmDS, (long)DemoDicomTags.TransferSyntaxUID);
                  TransferSyntax = TransferSyntax.Trim();
               }

               if(p.TransferSyntaxList.IndexOf(TransferSyntax) == -1)
                  p.TransferSyntaxList.Add(TransferSyntax);
            }
         }
         
         return pc;
      }

      private PresentationContext FindPresentationContext(string StorageClass, PresentationContextCollection pc)
      {
         foreach(PresentationContext pCtxt in pc)
         {
            if(pCtxt.AbstractSyntax == StorageClass)
               return pCtxt;
         }
         return null;
      }


      /// <summary>
      /// Initiates the C-STORE-REQ
      /// </summary>
      /// <param name="ClientAE">Calling AE Title.</param>		
      /// <returns></returns>
      public DicomExceptionCode Store(DicomServer server, String ClientAE)
      {
         DicomExceptionCode ret;


         ret = Associate(server, ClientAE, new SCUProcessFunc(StoreProcess));
         if(ret != DicomExceptionCode.Success)
         {
            //MessageBox.Show("Error during association: ",ret.ToString());
            return ret;
         }

         return 0;
      }

      // Returns the presentation ID of the association that contains 
      // 1. the storage class (sStorageClass)
      // 2. the transfer syntax (sSearchTransferSyntax)
      //
      // sSearchTransferSyntax: if String.Empty then match ANY valid transfer syntax
      // Returns 0 if not found
      public byte SearchAssociation(string sStorageClass, string sSearchTransferSyntax)
      {
         // Check if the sent abstract syntax is accepted
         byte pid = Association.FindAbstract(sStorageClass);
         while (pid != 0)
         {
            if (Association.GetResult(pid) == DicomAssociateAcceptResultType.Success)
            {
               // Get the accepted transfer syntax
               string transferSyntax = Association.GetTransfer(pid, 0);
               if (sSearchTransferSyntax == string.Empty)
                  return pid;
               if (transferSyntax == sSearchTransferSyntax)
                  return pid;
            }
            pid = Association.FindNextAbstract(pid, sStorageClass);
         }
         return 0;
      }

      public void StoreProcess( )
      {
         foreach(string file in Files)
         {
            string StorageClass;
            string StorageInstance;
            byte pid=0;
            string sOriginalTransferSyntax = DicomUidType.ImplicitVRLittleEndian;
            string sNewTransferSyntax = DicomUidType.ImplicitVRLittleEndian;
            string sSearchTransferSyntax = DicomUidType.ImplicitVRLittleEndian;

            ProgressFilesEventArgs pfe = new ProgressFilesEventArgs();

            if(Association == null)
            {
               Terminate();
               return;
            }

            pfe._File = new FileInfo(file);
            OnProgressFiles(pfe);

            bool bStoreFile = true;
            try
            {
               using ( DicomDataSet dcmDS = new DicomDataSet() )
               {
                  dcmDS.Load(file, DicomDataSetLoadFlags.None);


                  StorageClass = Utils.GetStringValue(dcmDS, DemoDicomTags.MediaStorageSOPClassUID);
                  StorageClass = StorageClass.Trim();

                  sOriginalTransferSyntax = Utils.GetStringValue(dcmDS, DemoDicomTags.TransferSyntaxUID);
                  if(StorageClass.Length == 0)
                  {
                     StorageClass = Utils.GetStringValue(dcmDS, DemoDicomTags.SOPClassUID);
                     StorageClass = StorageClass.Trim();
                  }
                  if(StorageClass.Length == 0)
                     StorageClass = "1.1.1.1";

                  StorageInstance = Utils.GetStringValue(dcmDS, DemoDicomTags.SOPInstanceUID);
                  StorageInstance = StorageInstance.Trim();
                  if(StorageInstance.Length == 0)
                  {
                     StorageInstance = "998.998.1.1.19950214.94000.1.102";
                  }

                  switch (Compression)
                  {
                     case DicomImageCompressionType.None:
                        // For this demo, DicomImageCompressionType.None actually means "Do Not Recompress"
                        // Search the association for the files current transfer syntax
                        // If not found, change to Little Endian
                        sSearchTransferSyntax = sOriginalTransferSyntax;
                        break;
                     case DicomImageCompressionType.J2kLossy:
                        sSearchTransferSyntax = DicomUidType.JPEG2000;
                        break;
                     case DicomImageCompressionType.J2kLossless:
                        sSearchTransferSyntax = DicomUidType.JPEG2000LosslessOnly;
                        break;
                     case DicomImageCompressionType.JpegLossy:
                        sSearchTransferSyntax = DicomUidType.JPEGBaseline1;
                        break;
                     case DicomImageCompressionType.JpegLossless:
                        sSearchTransferSyntax = DicomUidType.JPEGLosslessNonhier14;
                        break;
                  }

                  if ((pid = SearchAssociation(StorageClass, sSearchTransferSyntax)) != 0)
                  {
                     sNewTransferSyntax = sOriginalTransferSyntax;
                  }
                  else if ((pid = SearchAssociation(StorageClass, String.Empty)) != 0)
                  {
                     // Search for default transfer syntax
                     sNewTransferSyntax = DicomUidType.ImplicitVRLittleEndian;
                  }
                  else
                  {
                     bStoreFile = false;
                  }

                  if (bStoreFile)
                  {
                     // Change the transfer syntax to the accepted one
                     sNewTransferSyntax = sNewTransferSyntax.Trim();
                     if (sNewTransferSyntax.Length > 0)
                     {
                        dcmDS.ChangeTransferSyntax(sNewTransferSyntax, 2, ChangeTransferSyntaxFlags.None);
                     }

                     InvokeStatusEvent(StatusType.SendCStoreRequest, DicomExceptionCode.Success);
                     SendCStoreRequest(pid, MessageId++, StorageClass,
                                       StorageInstance, DicomCommandPriorityType.Medium,
                                       "", 0, dcmDS);
                  }
               }
            }
            catch(DicomException de)
            {
               InvokeStatusEvent(StatusType.Error, de.Code);
               Terminate();
               break;
            }

            if (bStoreFile)
            {
               if (!Wait())
               {
                  InvokeStatusEvent(StatusType.Timeout, DicomExceptionCode.Success);
                  Terminate();
                  return;
               }

               if (!Continue)
                  return;
            }
         }

         InvokeStatusEvent(StatusType.SendReleaseRequest, DicomExceptionCode.Success);
         SendReleaseRequest();

         if(!Wait())
         {
            InvokeStatusEvent(StatusType.Timeout, DicomExceptionCode.Success);
            Terminate();
            return;
         }
      }
   }
}
