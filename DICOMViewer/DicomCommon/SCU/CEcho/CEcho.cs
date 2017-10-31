using System;
using System.Windows.Forms;
using Leadtools.Dicom;

namespace Leadtools.DicomDemos.Scu.CEcho
{
    /// <summary>
    ///     CEcho
    /// </summary>
    public class CEcho : Scu
    {
        /// <summary>
        ///     DICOM Implementation Class UID.
        /// </summary>
        public static string ImplementationClassUid;

        //private StringCollection _Files = new StringCollection();

        public bool Continue = true;
        public short PresentID;

        /// <summary>
        /// </summary>
        public CEcho()
        {
        }

        #region Secure TLS Communication

        public CEcho(string clientPEM, DicomTlsCipherSuiteType tlsCipherSuiteType,
            DicomTlsCertificateType tlsCertificateType, string privateKeyPassword)
            : base(clientPEM, tlsCipherSuiteType, tlsCertificateType, privateKeyPassword)
        {
        }

        #endregion

        ~CEcho()
        {
            if (workThread != null && workThread.IsAlive)
                workThread.Abort();
        }

        public override void Init()
        {
            base.Init();
        }


        protected override void OnReceiveCEchoResponse(byte presentationID, int messageID, string affectedClass,
            DicomCommandStatusType status)
        {
            try
            {
                var se = new StatusEventArgs();
                InvokeStatusEvent(StatusType.ReceiveCEchoResponse, status, presentationID, messageID, affectedClass);
                Continue = true;
                Event.Set();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        protected override void OnReceiveReleaseResponse()
        {
            try
            {
                InvokeStatusEvent(StatusType.ReceiveReleaseResponse, DicomExceptionCode.Success);
                Close();
                Event.Set();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public override PresentationContextCollection GetPresentationContext()
        {
            var pc = new PresentationContextCollection();
            PresentationContext p;

            try
            {
                string VerificationClass;

                VerificationClass = DicomUidType.VerificationClass;

                // Check to make sure the Presentation Context isn't already in the collection
                p = FindPresentationContext(VerificationClass, pc);
                if (p == null)
                {
                    p = new PresentationContext();

                    p.AbstractSyntax = VerificationClass;
                    pc.Add(p);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return pc;
        }

        private PresentationContext FindPresentationContext(string StorageClass, PresentationContextCollection pc)
        {
            foreach (var pCtxt in pc)
            {
                try
                {
                    if (pCtxt.AbstractSyntax == StorageClass)
                        return pCtxt;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            return null;
        }

        /// <summary>
        ///     Initiates the C-EECHO-REQ
        /// </summary>
        /// <param name="ClientAE">Calling AE Title.</param>
        /// <returns></returns>
        public DicomExceptionCode Echo(DicomServer server, string ClientAE)
        {
            DicomExceptionCode ret;

            ret = Associate(server, ClientAE, EchoProcess);
            if (ret != DicomExceptionCode.Success)
            {
                return ret;
            }

            return 0;
        }

        public void EchoProcess()
        {
            byte pid;

            if (Association == null)
            {
                Terminate();
                return;
            }

            try
            {
                pid = Association.FindAbstract(DicomUidType.VerificationClass);
                InvokeStatusEvent(StatusType.SendCEchoRequest, DicomExceptionCode.Success);
                SendCEchoRequest(pid, MessageId++, DicomUidType.VerificationClass);
            }
            catch (DicomException de)
            {
                InvokeStatusEvent(StatusType.Error, de.Code);
                Terminate();
            }

            if (!Wait())
            {
                InvokeStatusEvent(StatusType.Timeout, DicomExceptionCode.Success);
                Terminate();
                return;
            }

            if (!Continue)
                return;

            InvokeStatusEvent(StatusType.SendReleaseRequest, DicomExceptionCode.Success);

            SendReleaseRequest();

            if (!Wait())
            {
                InvokeStatusEvent(StatusType.Timeout, DicomExceptionCode.Success);
                Terminate();
            }
        }

        protected override void OnReceiveData(byte presentationID, DicomDataSet cs, DicomDataSet ds)
        {
            base.OnReceiveData(presentationID, cs, ds);

            if (null != cs)
            {
                cs.Dispose();
            }
        }
    }
}