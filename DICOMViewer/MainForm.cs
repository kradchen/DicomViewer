﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net;
using System.Text;
using System.Windows.Forms;
using DICOMViewer.Config;
using Leadtools;
using Leadtools.Dicom;
using Leadtools.DicomDemos;
using Leadtools.DicomDemos.Scu.CFind;

namespace DICOMViewer
{
    public partial class MainForm : Form
    {
        CFind cfind;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.FormClosing += MainForm_FormClosing;
            cfind = new CFind
            {
                ImplementationClass = ConstDefineValues.CONFIGURATION_IMPLEMENTATIONCLASS,
                ProtocolVersion = ConstDefineValues.CONFIGURATION_PROTOCOLVERSION,
                ImplementationVersionName = ConstDefineValues.CONFIGURATION_IMPLEMENTATIONVERSIONNAME
            };
            cfind.Status += cfind_Status;
            cfind.FindComplete += cfind_FindComplete;
            cfind.MoveComplete += cfind_MoveComplete;

            DicomServer server = new DicomServer
            {
                AETitle = "LEAD_SERVER",
                Address = IPAddress.Parse(Convert.ToString("192.168.1.137")),
                Port = 104,
                Timeout = 30,
                IpType = DicomNetIpTypeFlags.Ipv4
            };
            CFindQuery dcmQuery = new CFindQuery();
            cfind.Find(server, FindType.Study, dcmQuery, "Client1");
        }
        public delegate void ShowDataSetTextdelegate(DicomDataSet ds);
        private void ShowDataSetText(DicomDataSet ds)
        {
            ListViewItem item;
            string tagValue;

            if (InvokeRequired)
            {
                Invoke(new ShowDataSetTextdelegate(ShowDataSetText), ds);
            }
            else
            {
                this.richTextBox1.Text += ds.ToString();
            }
        }

        #region StudyListView相关方法
        public delegate void StartUpdateDelegate(ListView lv);
        private void StartUpdate(ListView lv)
        {
            if (InvokeRequired)
            {
                Invoke(new StartUpdateDelegate(StartUpdate), lv);
            }
            else
            {
                lv.Items.Clear();
                lv.BeginUpdate();
            }
        }

        public delegate void EndUpdateDelegate(ListView lv);
        private void EndUpdate(ListView lv)
        {
            if (InvokeRequired)
            {
                Invoke(new EndUpdateDelegate(EndUpdate), lv);
            }
            else
            {
                lv.EndUpdate();
            }
        }

        public delegate void AddStudyItemDelegate(DicomDataSet ds);
        private void AddStudyItem(DicomDataSet ds)
        {
            ListViewItem item;
            string tagValue;

            if (InvokeRequired)
            {
                Invoke(new AddStudyItemDelegate(AddStudyItem), ds);
            }
            else
            {
                tagValue = Utils.GetStringValue(ds, DemoDicomTags.PatientID);
                item = listViewStudies.Items.Add(tagValue); 

                tagValue = Utils.GetStringValue(ds, DemoDicomTags.AccessionNumber);
                item.SubItems.Add(tagValue);

                tagValue = Utils.GetStringValue(ds, DemoDicomTags.PatientName);
                item.SubItems.Add(tagValue);

                tagValue = Utils.GetStringValue(ds, DemoDicomTags.PatientSex);
                item.SubItems.Add(tagValue);

                tagValue = Utils.GetStringValue(ds, DemoDicomTags.PatientBirthDate);
                item.SubItems.Add(tagValue);

                tagValue = Utils.GetStringValue(ds, DemoDicomTags.StudyDate);
                item.SubItems.Add(tagValue);

                tagValue = Utils.GetStringValue(ds, DemoDicomTags.ReferringPhysicianName);
                item.SubItems.Add(tagValue);

                tagValue = Utils.GetStringValue(ds, DemoDicomTags.StudyDescription);
                item.SubItems.Add(tagValue);

                item.Tag = Utils.GetStringValue(ds, DemoDicomTags.StudyInstanceUID);
            }
        }


        #endregion

        private void cfind_Status(object sender, StatusEventArgs e)
        {
            string message = "Unknown Error";
            String action = "";
            bool done = false;

            if (e.Type == StatusType.Error)
            {
                action = "Error";
                message = "Error occurred.\r\n";
                message += "\tError code is:\t" + e.Error.ToString();
            }
            else
            {
                switch (e.Type)
                {
                    case StatusType.ConnectFailed:
                        action = "Connect";
                        message = "Operation failed.";
                        done = true;
                        break;
                    case StatusType.ConnectSucceeded:
                        action = "Connect";
                        message = "Operation succeeded.\r\n";
                        message += "\tPeer Address:\t" + e.PeerIP.ToString() + "\r\n";
                        message += "\tPeer Port:\t\t" + e.PeerPort.ToString();
                        break;
                    case StatusType.SendAssociateRequest:
                        action = "Associate Request";
                        message = "Request sent.";
                        break;
                    case StatusType.ReceiveAssociateAccept:
                        action = "Associcated Accept";
                        message = "Received.\r\n";
                        message += "\tCalling AE:\t" + e.CallingAE + "\r\n";
                        message += "\tCalled AE:\t" + e.CalledAE;
                        break;
                    case StatusType.ReceiveAssociateRequest:
                        action = "Associcated Request";
                        message = "Received.\r\n";
                        message += "\tCalling AE:\t" + e.CallingAE + "\r\n";
                        message += "\tCalled AE:\t" + e.CalledAE;
                        break;
                    case StatusType.ReceiveAssociateReject:
                        action = "Associate Reject";
                        message = "Received Associate Reject!";
                        message += "\r\n\tSource: " + e.Source.ToString();
                        message += "\r\n\tResult: " + e.Result.ToString();
                        message += "\r\n\tReason: " + e.Reason.ToString();
                        done = true;
                        break;
                    case StatusType.AbstractSyntaxNotSupported:
                        action = "Error";
                        message = "Abstract Syntax NOT supported!";
                        done = true;
                        break;
                    case StatusType.SendCFindRequest:
                        action = "C-FIND";
                        message = "Sending request";
                        break;
                    case StatusType.ReceiveCFindResponse:
                        action = "C-FIND";
                        if (e.Error == DicomExceptionCode.Success)
                        {
                            message = "Operation completed successfully.";
                        }
                        else
                        {
                            if (e.Status == DicomCommandStatusType.Pending)
                            {
                                message = "Additional operations pending.";
                            }
                            else
                            {
                                message = "Error in response Status code is: " + e.Status.ToString();
                            }
                        }
                        break;
                    case StatusType.ConnectionClosed:
                        action = "Connect";
                        message = "Network Connection closed!";
                        done = true;
                        break;
                    case StatusType.ProcessTerminated:
                        action = "";
                        message = "Process has been terminated!";
                        done = true;
                        break;
                    case StatusType.SendReleaseRequest:
                        action = "Release Request";
                        message = "Request sent.";
                        break;
                    case StatusType.ReceiveReleaseResponse:
                        action = "Release Response";
                        message = "Response received.";
                        done = true;
                        break;
                    case StatusType.SendCMoveRequest:
                        action = "C-MOVE";
                        message = "Sending request";
                        break;
                    case StatusType.ReceiveCMoveResponse:
                        action = "C-MOVE";
                        message = "Received response\r\n";
                        message += "\tStatus: " + e.Status.ToString() + "\r\n";
                        message += "\tNumber Completed: " + e.NumberCompleted.ToString() + "\r\n";
                        message += "\tNumber Remaining: " + e.NumberRemaining.ToString();
                        break;
                    case StatusType.SendCStoreResponse:
                        action = "C-STORE";
                        message = "Sending response";
                        break;
                    case StatusType.ReceiveCStoreRequest:
                        action = "C-STORE";
                        message = "Received request";
                        break;
                    case StatusType.Timeout:
                        message = "Communication timeout. Process will be terminated.";
                        done = true;
                        break;
                }
            }
        }

        private void cfind_FindComplete(object sender, FindCompleteEventArgs e)
        {
            switch (e.Type)
            {
                case FindType.Study:
                    StartUpdate(listViewStudies);
                    foreach (DicomDataSet ds in e.Datasets)
                    {
                        AddStudyItem(ds);
                    }
                    EndUpdate(listViewStudies);
                    break;
                case FindType.StudySeries:
                    foreach (DicomDataSet ds in e.Datasets)
                    {
                        Console.Out.WriteLine(ds.ToString());
                    }
                    break;
            }
        }

        private void cfind_MoveComplete(object sender, MoveCompleteEventArgs e)
        {
            //            if (InvokeRequired)
            //            {
            //                Invoke(new MoveCompleteEventHandler(cfind_MoveComplete), sender, e);
            //            }
            //            else
            //            {
            foreach (DicomDataSet ds in e.Datasets)
            {
                DicomElement element;

                try
                {
                    element = ds.FindFirstElement(null, DemoDicomTags.PixelData, true);
                    if (element == null)
                        continue;

                    for (int i = 0; i < ds.GetImageCount(element); i++)
                    {
                        RasterImage image;
                        DicomImageInformation info = ds.GetImageInformation(element, i);

                        image = ds.GetImage(element, i, 0, info.IsGray ? RasterByteOrder.Gray : RasterByteOrder.Rgb,
                            DicomGetImageFlags.AutoApplyModalityLut |
                            DicomGetImageFlags.AutoApplyVoiLut |
                            DicomGetImageFlags.AllowRangeExpansion);
                        if (image != null)
                        {
                        }
                    }
                }
                catch (DicomException de)
                {
                    StatusEventArgs eventArg = new StatusEventArgs();

                    eventArg._Error = de.Code;
                    eventArg._Type = StatusType.Error;
                    cfind_Status(new object(), eventArg);
                }
            }
            //            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
//            Application.Exit();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            
            if (cfind != null)
            {
                cfind.Terminate();
                cfind.CloseForced(true);
            }

        }

        private void listViewStudies_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listViewStudies.SelectedItems.Count == 0)
                return;

            //imageList.Items.Clear();

            string patientID, studyInstance, seriesInstance;

            patientID = listViewStudies.SelectedItems[0].SubItems[1].Text;
            studyInstance = listViewStudies.SelectedItems[0].Tag as string;
            //seriesInstance = listViewSeries.SelectedItems[0].Tag as string;
        }
    }
}