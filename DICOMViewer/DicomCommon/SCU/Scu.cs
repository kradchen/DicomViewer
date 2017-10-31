using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Reflection;

using Leadtools;
using Leadtools.Dicom;
using Leadtools.Demos;

namespace Leadtools.DicomDemos.Scu
{
   public delegate void SCUProcessFunc( );

   internal class AssociateInfo
   {
      public Scu scu;
      public DicomServer dcmServer;
      public string ClientAE;
      public SCUProcessFunc process;

      public void DoAssociate( )
      {
         DicomAssociate associate;

         DicomExceptionCode ret = DicomExceptionCode.Success;

         ret = scu.Connect(dcmServer);
         if(ret != DicomExceptionCode.Success)
         {
            return;
         }

         associate = scu.BuildAssociation(dcmServer.AETitle, ClientAE);
         if(associate == null)
         {
            scu.InvokeStatusEvent(StatusType.Error, DicomExceptionCode.Parameter);
            scu.Terminate();
            return;
         }

         scu.InvokeStatusEvent(StatusType.SendAssociateRequest, DicomExceptionCode.Success);
         try
         {
            if(scu.IsConnected())
            {
               scu.SendAssociateRequest(associate);
            }
            else
            {
               scu.InvokeStatusEvent(StatusType.Error, DicomExceptionCode.NetConnectionAborted);
               scu.Terminate();
               return;
            }
         }
         catch(DicomException de)
         {
            scu.InvokeStatusEvent(StatusType.Error, de.Code);
            scu.Terminate();
            return;
         }

         if(!scu.Wait())
         {
            //
            // Connection timed out
            //
            scu.InvokeStatusEvent(StatusType.Timeout, DicomExceptionCode.Success);
            scu.Terminate();
         }         
         if (!scu.Rejected && scu.IsAssociated())
            process();
      }
   }

   /// <summary>
   /// Summary description for Scu.
   /// </summary>
   public class Scu : Base
   {
      private DicomServer dcmServer;
      private AssociateInfo workInfo;
      public Thread workThread;
      private short _MessageId = 1;

      private string _ProtocolVersion;
      private int _presentationContextType;

      public int PresentationContextType
      {
         get { return _presentationContextType; }
         set { _presentationContextType = value; }
      }

      private bool _Rejected = false;

      public bool Rejected
      {
         get
         {
            return _Rejected;
         }
      }

      public string ProtocolVersion
      {
         get
         {
            return _ProtocolVersion;
         }
         set
         {
            _ProtocolVersion = value;
         }
      }

      /// <summary>
      /// Dicom message id.
      /// </summary>
      public short MessageId
      {
         get
         {
            return _MessageId;
         }
         set
         {
            _MessageId = value;
         }
      }

      public Scu( )
      {
      }

      #region Secure TLS Communication
      public Scu(string clientPEM, DicomTlsCipherSuiteType tlsCipherSuiteType, DicomTlsCertificateType tlsCertificateType, string privateKeyPassword)
         : base(clientPEM, tlsCipherSuiteType, tlsCertificateType, privateKeyPassword)
      {

      }
      #endregion

      ~Scu( )
      {
         if(workThread != null && workThread.IsAlive)
            workThread.Abort();
      }

      public override void Init( )
      {
         base.Init();
      }

      protected override void OnConnect(DicomExceptionCode error)
      {
         if(error != DicomExceptionCode.Success)
         {
            InvokeStatusEvent(StatusType.ConnectFailed, error);

            // Calling terminate here causes a problem, because it disposes of the Net
            // The DicomNet object is disposed of in the OnClose event
            // 
            // This occurs in two places:
            // 1. InvokeStatusEvent->store_Status->ClosedForced which generates an OnClose
            // 2. Terminate->CloseForced which genrates an OnClose
            //
            // So do not call Terminate
            // Terminate();
            return;
         }
         StatusEventArgs e = new StatusEventArgs();

         e._Type = StatusType.ConnectSucceeded;
         e._PeerIP = IPAddress.Parse(PeerAddress);
         e._PeerPort = PeerPort;
         InvokeStatusEvent(e);
         if (!IsSecureTLS)
            Event.Set();
      }

      protected override void OnSecureLinkReady(DicomExceptionCode error)
      {
         if (error != DicomExceptionCode.Success)
         {
            InvokeStatusEvent(StatusType.ConnectFailed, error);
            Terminate();
            return;
         }

         StatusEventArgs e = new StatusEventArgs();
         e._Type = StatusType.SecureLinkReady;
         e._PeerIP = IPAddress.Parse(PeerAddress);
         e._PeerPort = PeerPort;
         InvokeStatusEvent(e);

         Event.Set();
      }

      protected override void OnReceiveAssociateAccept(DicomAssociate association)
      {
         InvokeStatusEvent(StatusType.ReceiveAssociateAccept, 0, association.Calling,
                           association.Called);
         Event.Set();
      }

      protected override void OnReceiveAssociateReject(DicomAssociateRejectResultType result, DicomAssociateRejectSourceType source, DicomAssociateRejectReasonType reason)
      {
         InvokeStatusEvent(StatusType.ReceiveAssociateReject, result, source, reason);
         _Rejected = true;
         Close();
         StatusEventArgs se = new StatusEventArgs();
         se._Type = StatusType.ConnectionClosed;
         InvokeStatusEvent(se);
         Event.Set();
      }

      /// <summary>
      /// Waits for a dicom communication to complete.
      /// </summary>
      /// <returns></returns>
      public bool Wait( )
      {
         WaitReturn ret;

         ret = Utils.WaitForComplete(dcmServer.Timeout * 1000, Event, ResetTimeoutEvent);
         return (ret == WaitReturn.Complete);

      }

      /// <summary>
      /// Connects to a dicom server.
      /// </summary>
      /// <param name="server">Dicom server to connect to.</param>
      /// <returns></returns>
      public DicomExceptionCode Connect(DicomServer server)
      {
         dcmServer = server;

         try
         {
            _Rejected = false;
#if LEADTOOLS_V17_OR_LATER
            Connect(null, 0, server.Address.ToString(), server.Port , server.IpType);
#else
            Connect(null, 0, server.Address.ToString(), server.Port);
#endif
            if(!Wait())
            {
               //
               // Connection timed out
               //
               return DicomExceptionCode.NetTimeout;
            }
         }
         catch(DicomException e)
         {
            return e.Code;
         }

         return DicomExceptionCode.Success;
      }

      public virtual PresentationContextCollection GetPresentationContext( )
      {
         return new PresentationContextCollection();
      }

      public DicomAssociate BuildAssociation(string CalledTitle, string CallingTitle)
      {
         PresentationContextCollection contexts = GetPresentationContext();
         DicomAssociate association = new DicomAssociate(true);

         association.Called = CalledTitle;
         association.Calling = CallingTitle;
         association.MaxLength = 46726;
         association.ImplementClass = ImplementationClass;

         byte pid = 1;
         bool addedImplicitVRLittleEndian = false;
         if (PresentationContextType == 0)
         {
            // One presentation context contains all transfer syntaxes
            foreach (PresentationContext pc in contexts)
            {
               association.AddPresentationContext(pid, 0, pc.AbstractSyntax);
               addedImplicitVRLittleEndian = false;
               foreach (string transfersyntax in pc.TransferSyntaxList)
               {
                  association.AddTransfer(pid, transfersyntax);
                  if (transfersyntax == DicomUidType.ImplicitVRLittleEndian)
                     addedImplicitVRLittleEndian = true;
               }
               if (!addedImplicitVRLittleEndian)
                  association.AddTransfer(pid, DicomUidType.ImplicitVRLittleEndian);
               pid += 2;
            }
         }
         else
         {
            // Separate presentation context for each transfer syntax
            foreach (PresentationContext pc in contexts)
            {
               addedImplicitVRLittleEndian = false;
               foreach (string transfersyntax in pc.TransferSyntaxList)
               {
                  association.AddPresentationContext(pid, 0, pc.AbstractSyntax);
                  association.AddTransfer(pid, transfersyntax);
                  if (transfersyntax == DicomUidType.ImplicitVRLittleEndian)
                     addedImplicitVRLittleEndian = true;
                  pid += 2;
               }
               if (!addedImplicitVRLittleEndian)
               {
                  association.AddPresentationContext(pid, 0, pc.AbstractSyntax);
                  association.AddTransfer(pid, DicomUidType.ImplicitVRLittleEndian);
                  pid += 2;
               }
            }
         }




         return association;
      }

      /// <summary>
      /// Send an associate request to a dicom scp.
      /// </summary>
      /// <param name="server">Dicom server.</param>
      /// <param name="CallingTitle">Calling ae title.</param>
      /// <returns>DICOM_SUCCESS if successful, error otherwise.</returns>
      public DicomExceptionCode Associate(DicomServer server, string CallingTitle, SCUProcessFunc process)
      {
         //
         // Terminate any existing communication
         //
         Terminate();

         workInfo = new AssociateInfo();
         workInfo.scu = this;
         workInfo.dcmServer = server;
         workInfo.ClientAE = CallingTitle;
         workInfo.process = process;

         workThread = new Thread(new ThreadStart(workInfo.DoAssociate));
         workThread.Start();

         return DicomExceptionCode.Success;
      }

      /// <summary>
      /// Terminates the dicom request.
      /// </summary>
      public void Terminate( )
      {
         try
         {
         if(IsConnected())
         {
            StatusEventArgs se = new StatusEventArgs();

            if(IsAssociated())
            {
               SendAbort(DicomAbortSourceType.User, 0);
            }
            CloseForced(true);
            se._Type = StatusType.ProcessTerminated;
            OnStatus(se);
         }

         //
         // Terminate the store thread
         //
         if(workThread != null && workThread.IsAlive)
            workThread.Abort();
      }
         catch (Exception )
         {
         }
         finally
         {
         }

      }

      protected override void OnClose(DicomExceptionCode error, DicomNet net)
      {
         base.OnClose(error, net);
      }
   }
}
