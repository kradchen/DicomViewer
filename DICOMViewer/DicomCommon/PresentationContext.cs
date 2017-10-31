using System;
using System.Collections.Specialized;

namespace Leadtools.DicomDemos
{
   /// <summary>
   /// Summary description for ClassInfo.
   /// </summary>
   public class PresentationContext
   {
      private string _AbstractSyntax;
      private StringCollection _TransferSyntaxList = new StringCollection();

      public string AbstractSyntax
      {
         get
         {
            return _AbstractSyntax;
         }
         set
         {
            _AbstractSyntax = value;
         }
      }

      public StringCollection TransferSyntaxList
      {
         get
         {
            return _TransferSyntaxList;
         }
      }

      public PresentationContext( )
      {
         //
         // TODO: Add constructor logic here
         //
      }
   }
}
