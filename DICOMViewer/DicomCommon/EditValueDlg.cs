using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using Leadtools.Dicom;

namespace Leadtools.DicomDemos
{
   /// <summary>
   /// Summary description for EditValueDlg.
   /// </summary>
   public class EditValueDlg : System.Windows.Forms.Form
   {
      /// <summary>
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.Container components = null;
      private System.Windows.Forms.Label labelVR;
      private System.Windows.Forms.Label labelFormat;
      private System.Windows.Forms.Label label1;
      public System.Windows.Forms.ListBox listBoxValues;
      private System.Windows.Forms.TextBox textBoxValue;
      private System.Windows.Forms.Button buttonBefore;
      private System.Windows.Forms.Button buttonAfter;
      private System.Windows.Forms.Button buttonDelete;
      private System.Windows.Forms.Button buttonModify;
      private System.Windows.Forms.Button buttonOK;
      private System.Windows.Forms.Button buttonCancel;

      private DicomDataSet ds;
      private DicomElement element;

      public EditValueDlg(DicomDataSet ds, DicomElement element)
      {
         //
         // Required for Windows Form Designer support
         //
         InitializeComponent();

         this.ds = ds;
         this.element = element;
      }

      /// <summary>
      /// Clean up any resources being used.
      /// </summary>
      protected override void Dispose(bool disposing)
      {
         if (disposing)
         {
            if (components != null)
            {
               components.Dispose();
            }
         }
         base.Dispose(disposing);
      }

      #region Windows Form Designer generated code
      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
          this.labelVR = new System.Windows.Forms.Label();
          this.labelFormat = new System.Windows.Forms.Label();
          this.label1 = new System.Windows.Forms.Label();
          this.listBoxValues = new System.Windows.Forms.ListBox();
          this.textBoxValue = new System.Windows.Forms.TextBox();
          this.buttonBefore = new System.Windows.Forms.Button();
          this.buttonAfter = new System.Windows.Forms.Button();
          this.buttonDelete = new System.Windows.Forms.Button();
          this.buttonModify = new System.Windows.Forms.Button();
          this.buttonOK = new System.Windows.Forms.Button();
          this.buttonCancel = new System.Windows.Forms.Button();
          this.SuspendLayout();
          // 
          // labelVR
          // 
          this.labelVR.Location = new System.Drawing.Point(8, 8);
          this.labelVR.Name = "labelVR";
          this.labelVR.Size = new System.Drawing.Size(392, 16);
          this.labelVR.TabIndex = 0;
          this.labelVR.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
          // 
          // labelFormat
          // 
          this.labelFormat.Location = new System.Drawing.Point(8, 24);
          this.labelFormat.Name = "labelFormat";
          this.labelFormat.Size = new System.Drawing.Size(392, 48);
          this.labelFormat.TabIndex = 1;
          this.labelFormat.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
          // 
          // label1
          // 
          this.label1.Location = new System.Drawing.Point(8, 72);
          this.label1.Name = "label1";
          this.label1.Size = new System.Drawing.Size(100, 16);
          this.label1.TabIndex = 2;
          this.label1.Text = "Values:";
          // 
          // listBoxValues
          // 
          this.listBoxValues.Location = new System.Drawing.Point(8, 88);
          this.listBoxValues.Name = "listBoxValues";
          this.listBoxValues.Size = new System.Drawing.Size(296, 238);
          this.listBoxValues.TabIndex = 3;
          this.listBoxValues.SelectedIndexChanged += new System.EventHandler(this.listBoxValues_SelectedIndexChanged);
          // 
          // textBoxValue
          // 
          this.textBoxValue.Location = new System.Drawing.Point(8, 336);
          this.textBoxValue.Name = "textBoxValue";
          this.textBoxValue.Size = new System.Drawing.Size(296, 20);
          this.textBoxValue.TabIndex = 4;
          // 
          // buttonBefore
          // 
          this.buttonBefore.Location = new System.Drawing.Point(312, 88);
          this.buttonBefore.Name = "buttonBefore";
          this.buttonBefore.Size = new System.Drawing.Size(80, 23);
          this.buttonBefore.TabIndex = 5;
          this.buttonBefore.Text = "Insert Before";
          this.buttonBefore.Click += new System.EventHandler(this.buttonBefore_Click);
          // 
          // buttonAfter
          // 
          this.buttonAfter.Location = new System.Drawing.Point(312, 120);
          this.buttonAfter.Name = "buttonAfter";
          this.buttonAfter.Size = new System.Drawing.Size(80, 23);
          this.buttonAfter.TabIndex = 6;
          this.buttonAfter.Text = "Insert After";
          this.buttonAfter.Click += new System.EventHandler(this.buttonAfter_Click);
          // 
          // buttonDelete
          // 
          this.buttonDelete.Enabled = false;
          this.buttonDelete.Location = new System.Drawing.Point(312, 152);
          this.buttonDelete.Name = "buttonDelete";
          this.buttonDelete.Size = new System.Drawing.Size(80, 23);
          this.buttonDelete.TabIndex = 7;
          this.buttonDelete.Text = "Delete";
          this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
          // 
          // buttonModify
          // 
          this.buttonModify.Enabled = false;
          this.buttonModify.Location = new System.Drawing.Point(312, 184);
          this.buttonModify.Name = "buttonModify";
          this.buttonModify.Size = new System.Drawing.Size(80, 23);
          this.buttonModify.TabIndex = 8;
          this.buttonModify.Text = "Modify";
          this.buttonModify.Click += new System.EventHandler(this.buttonModify_Click);
          // 
          // buttonOK
          // 
          this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
          this.buttonOK.Location = new System.Drawing.Point(312, 303);
          this.buttonOK.Name = "buttonOK";
          this.buttonOK.Size = new System.Drawing.Size(80, 23);
          this.buttonOK.TabIndex = 9;
          this.buttonOK.Text = "&OK";
          // 
          // buttonCancel
          // 
          this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
          this.buttonCancel.Location = new System.Drawing.Point(312, 333);
          this.buttonCancel.Name = "buttonCancel";
          this.buttonCancel.Size = new System.Drawing.Size(80, 23);
          this.buttonCancel.TabIndex = 10;
          this.buttonCancel.Text = "&Cancel";
          // 
          // EditValueDlg
          // 
          this.AcceptButton = this.buttonOK;
          this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
          this.CancelButton = this.buttonCancel;
          this.ClientSize = new System.Drawing.Size(402, 367);
          this.Controls.Add(this.buttonCancel);
          this.Controls.Add(this.buttonOK);
          this.Controls.Add(this.buttonModify);
          this.Controls.Add(this.buttonDelete);
          this.Controls.Add(this.buttonAfter);
          this.Controls.Add(this.buttonBefore);
          this.Controls.Add(this.textBoxValue);
          this.Controls.Add(this.listBoxValues);
          this.Controls.Add(this.label1);
          this.Controls.Add(this.labelFormat);
          this.Controls.Add(this.labelVR);
          this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
          this.MaximizeBox = false;
          this.MinimizeBox = false;
          this.Name = "EditValueDlg";
          this.ShowInTaskbar = false;
          this.Text = "Edit Value";
          this.Load += new System.EventHandler(this.EditValueDlg_Load);
          this.ResumeLayout(false);
          this.PerformLayout();

      }
      #endregion

      private void EditValueDlg_Load(object sender, System.EventArgs e)
      {
         Init();
      }

      private void Init()
      {
         string strInfo;
         string[] values = null;
         DicomVR vr;
         DicomTag tag;

         strInfo = ds.GetConvertValue(element);
         if (strInfo != null)
            values = strInfo.Split('\\');
         if ((values != null) && (values.GetLength(0) > 0))
         {
            foreach (string value in values)
            {
               listBoxValues.Items.Add(value);
            }

            if (listBoxValues.Items.Count > 0)
               listBoxValues.SelectedIndex = 0;
         }

         vr = DicomVRTable.Instance.Find(element.VR);
         tag = DicomTagTable.Instance.Find(element.Tag);

         strInfo = "Value Representation: " + vr.Name;
         labelVR.Text = strInfo;

         switch (element.VR)
         {
            case DicomVRType.OB:
            case DicomVRType.UN:
               strInfo = "Hexadecimal";
               break;

            case DicomVRType.SS:
            case DicomVRType.US:
            case DicomVRType.OW:
            case DicomVRType.SL:
            case DicomVRType.IS:
            case DicomVRType.UL:
               strInfo = "Integer";
               break;

            case DicomVRType.AT:
               strInfo = "Group:Element\r\n(Group and Element should be hexadecimal words)";
               break;

            case DicomVRType.FD:
            case DicomVRType.FL:
            case DicomVRType.DS:
               strInfo = "Float";
               break;

            case DicomVRType.CS:
            case DicomVRType.SH:
            case DicomVRType.LO:
            case DicomVRType.AE:
            case DicomVRType.LT:
            case DicomVRType.ST:
            case DicomVRType.UI:
            case DicomVRType.UT:
            case DicomVRType.PN:
               strInfo = "String";
               break;

            case DicomVRType.AS:
               strInfo = "Number Reference\r\n(Reference = 'days' or 'weeks' or 'months' or 'years')";
               break;

            case DicomVRType.DA:
               strInfo = "MM/DD/YYYY\r\n(MM=Month, DD=Day, YYYY=Year)";
               break;

            case DicomVRType.DT:
               strInfo = "CC MM/DD/YYYY HH:MM:SS.FFFFFF&OOOO\r\n(CC=Centry, MM=Month, DD=Day, YYYY=Year)\r\n(HH=Hours, MM=Minutes,SS=Seconds, ";
               strInfo += "FFFFFF=Fractional Second, OOOO=Offset from Coordinated Universial Time)";
               break;

            case DicomVRType.TM:
               strInfo = "HH:MM:SS.FFFF\r\n(HH=Hours, MM=Minutes, SS=Seconds, FFFF=Fractional Second)";
               break;

            default:
               strInfo = "";
               break;

         }
         if (tag== null)
         {
            buttonBefore.Enabled = false;
            buttonAfter.Enabled = false;
            buttonDelete.Enabled = false;
            buttonModify.Enabled = false;
            buttonOK.Enabled = false;
            textBoxValue.Enabled = false;
            buttonCancel.Enabled = true;
         }
         else
         {
            buttonBefore.Enabled = tag.MaxVM >= 1 || tag.MaxVM == -1;
            buttonAfter.Enabled = tag.MaxVM >= 1 || tag.MaxVM == -1;
         }
         labelFormat.Text = strInfo;
      }

      private void buttonBefore_Click(object sender, System.EventArgs e)
      {
         if (textBoxValue.Text.Length > 0)
         {
            if (listBoxValues.SelectedIndex == -1)
               listBoxValues.Items.Add(textBoxValue.Text);
            else
               listBoxValues.Items.Insert(listBoxValues.SelectedIndex, textBoxValue.Text);
         }
      }

      private void buttonAfter_Click(object sender, System.EventArgs e)
      {
         if (textBoxValue.Text.Length > 0)
         {
            if (listBoxValues.SelectedIndex == -1)
               listBoxValues.Items.Add(textBoxValue.Text);
            else
               listBoxValues.Items.Insert(listBoxValues.SelectedIndex + 1, textBoxValue.Text);
         }
      }

      private void buttonDelete_Click(object sender, System.EventArgs e)
      {
         try
         {
            if (listBoxValues.Items.Count > 0 && (listBoxValues.SelectedIndex != -1))
            {
               listBoxValues.Items.RemoveAt(listBoxValues.SelectedIndex);
            }
         }
         catch { }
      }

      private void buttonModify_Click(object sender, System.EventArgs e)
      {
         if (textBoxValue.Enabled == false)
         {
            MessageBox.Show("The value for this element can’t be edited.");
            return;
         }
         if (textBoxValue.Text.Length > 0)
            listBoxValues.Items[listBoxValues.SelectedIndex] = textBoxValue.Text;
      }

      private void listBoxValues_SelectedIndexChanged(object sender, System.EventArgs e)
      {
         if (listBoxValues.SelectedItem != null)
         {
            textBoxValue.Text = listBoxValues.SelectedItem.ToString();
         }
         else
            textBoxValue.Text = "";

        buttonModify.Enabled = (listBoxValues.SelectedItem != null);
        buttonDelete.Enabled = (listBoxValues.SelectedItem != null);
      }
   }
}
