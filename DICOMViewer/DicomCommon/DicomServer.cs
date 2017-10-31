using System;
using System.Net;
using System.ComponentModel;
using Leadtools.Dicom;

namespace Leadtools.DicomDemos
{
   /// <summary>
   /// Summary description for DicomServer.
   /// </summary>
   public class DicomServer
   {
      private string _AETitle = "";
      private int _Port = 104;
      private IPAddress _Address = IPAddress.Parse("127.0.0.1");
#if LEADTOOLS_V17_OR_LATER
      private DicomNetIpTypeFlags _ipType;
#endif

      private int _Timeout = 30;

      #region Server Properties

      /// <summary>
      /// Called AE Title.
      /// </summary>
      [Category("Server")]
      [Description("Server AE Title")]
      public string AETitle
      {
         get
         {
            // TODO:  Add Storage.AETitle getter implementation
            return _AETitle;
         }
         set
         {
            _AETitle = value;
         }
      }

      /// <summary>
      /// Port of server.
      /// </summary>
      [Category("Server")]
      [Description("Server port number")]
      public int Port
      {
         get
         {
            // TODO:  Add Storage.Port getter implementation
            return _Port;
         }
         set
         {
            _Port = value;
         }
      }

      /// <summary>
      /// IPAddress of server
      /// </summary>
      [Category("Server")]
      [Description("Server internet address")]
      public System.Net.IPAddress Address
      {
         get
         {
            return _Address;
         }
         set
         {
            _Address = value;
         }
      }

#if LEADTOOLS_V17_OR_LATER
      public DicomNetIpTypeFlags IpType
      {
         get { return _ipType; }
         set { _ipType = value; }
      }
#endif

      /// <summary>
      /// About of time in milliseconds to wait for a response from
      /// the server. Assign zero to wait indefinitely.
      /// </summary>
      [Category("Server")]
      [Description("Sever timeout in milliseconds")]
      public int Timeout
      {
         get
         {
            return _Timeout;
         }
         set
         {
            _Timeout = value;
         }
      }
      #endregion

      public DicomServer( )
      {
#if LEADTOOLS_V17_OR_LATER
         _ipType = DicomNetIpTypeFlags.Ipv4;
#endif
      }
   }
}
