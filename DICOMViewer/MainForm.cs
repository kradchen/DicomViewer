using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using DicomDemo;
using DICOMViewer.Config;
using Leadtools.Dicom;
using Leadtools.DicomDemos;
using Leadtools.DicomDemos.Scu.CFind;

namespace DICOMViewer
{
    public partial class MainForm : Form
    {
        public delegate void AddSeriesItemDelegate(DicomDataSet ds);
        private CFind cfind;
        private readonly string AETitle = "Client1";
        private string patientId;
        private readonly int Port = 1000;
        private UserDicomDir dicomDir;
        private Queue<string> queue = new Queue<string>();
        private string seriesInstance;
        private readonly DicomServer server;
        private string studyInstance;
        private string workPath= "D:\\MyWork\\C#\\DicomViewer\\DICOMViewer\\bin\\Debug\\dicomfile\\1.2.840.1.19439.0.108707908.20171027094139.1910.10000.4416679";
        private Thread th;

        public MainForm()
        {
            InitializeComponent();
            FormClosing += MainForm_FormClosing;
            cfind = new CFind
            {
                ImplementationClass = ConstDefineValues.CONFIGURATION_IMPLEMENTATIONCLASS,
                ProtocolVersion = ConstDefineValues.CONFIGURATION_PROTOCOLVERSION,
                ImplementationVersionName = ConstDefineValues.CONFIGURATION_IMPLEMENTATIONVERSIONNAME
            };
            cfind.Status += cfind_Status;
            cfind.FindComplete += cfind_FindComplete;
            cfind.MoveComplete += cfind_MoveComplete;

            server = new DicomServer
            {
                AETitle = "LEAD_SERVER",
                Address = IPAddress.Parse(Convert.ToString("192.168.3.29")),
                Port = 104,
                Timeout = 30,
                IpType = DicomNetIpTypeFlags.Ipv4
            };
            DoCreateDicomDirectory();
            Application.Exit();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

            CFindQuery dcmQuery = new CFindQuery();
            cfind.Find(server, FindType.Study, dcmQuery, AETitle);
        }

        private void cfind_Status(object sender, StatusEventArgs e)
        {
            var message = "Unknown Error";
            var action = "";
            var done = false;

            if (e.Type == StatusType.Error)
            {
                action = "Error";
                message = "Error occurred.\r\n";
                message += "\tError code is:\t" + e.Error;
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
                        message += "\tPeer Address:\t" + e.PeerIP + "\r\n";
                        message += "\tPeer Port:\t\t" + e.PeerPort;
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
                        message += "\r\n\tSource: " + e.Source;
                        message += "\r\n\tResult: " + e.Result;
                        message += "\r\n\tReason: " + e.Reason;
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
                                message = "Error in response Status code is: " + e.Status;
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
                        message += "\tStatus: " + e.Status + "\r\n";
                        message += "\tNumber Completed: " + e.NumberCompleted + "\r\n";
                        message += "\tNumber Remaining: " + e.NumberRemaining;
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
                    Directory.CreateDirectory(workPath);
                    foreach (DicomDataSet ds in e.Datasets)
                    {
                        var SeriesInstanceUID = Utils.GetStringValue(ds, DemoDicomTags.SeriesInstanceUID);
                        queue.Enqueue(SeriesInstanceUID);
                    }
                    if (th == null || (th != null && !th.IsAlive))
                    {
                        th = new Thread(() =>
                        {
                            cfind.workThread.Join();
                            if (queue.Count <= 0) return;
                            SendMoveSeries();
                        });
                    }
                    th.Start();
                    break;
            }
        }

        private void cfind_MoveComplete(object sender, MoveCompleteEventArgs e)
        {
            
            for (var i = 0; i < e.Datasets.Count; i++)
            {
                Console.Out.WriteLine("begin ds save");
                DicomDataSet ds = e.Datasets[i];
                ds.Save(workPath+"\\" + i + ".dcm",
                    DicomDataSetSaveFlags.None);
                Console.Out.WriteLine("end save");
            }
            //获取完成！
            if (queue.Count <= 0)
            {
                DoCreateDicomDirectory();
                //this.Cursor = Cursors.Default;
                MessageBox.Show("下载完成！");
            }
            if (th == null || (th != null && !th.IsAlive))
            {
                th = new Thread(() =>
                {
                    Console.Out.WriteLine("th start!");
                    cfind.workThread.Join();
                    if (queue.Count > 0)
                    {
                        Console.Out.WriteLine("send move!");
                        SendMoveSeries();
                    }
                });
            }
            th.Start();
        }

        private void SendMoveSeries()
        {
            if (queue.Count <= 0) return;
            seriesInstance = queue.Dequeue();
            cfind.MoveSeries(server, AETitle, patientId, studyInstance, seriesInstance, Port);
        }

        private void DoCreateDicomDirectory()
        {
            try
            {
                dicomDir = new UserDicomDir();
                // Reset the DICOM Directory and set the destination folder where
                // the DICOMDIR file wiUserDicomDirll be saved
                dicomDir.Reset(workPath);

                // If it is desired to change the values of the Implementation Class
                // UID (0002,0012) and the Implementation Version Name (0002,0013)...
                DicomElement element;
                element = dicomDir.DataSet.FindFirstElement(null, DemoDicomTags.ImplementationClassUID, false);
                if (element != null)
                {
                    dicomDir.DataSet.SetStringValue(element, "1.2.840.114257.0.1", DicomCharacterSetType.Default); // Must be a UID
                }
                element = dicomDir.DataSet.FindFirstElement(null, DemoDicomTags.ImplementationVersionName, false);
                if (element != null)
                {
                    dicomDir.DataSet.SetStringValue(element, "LEADTOOLS 15", DicomCharacterSetType.Default); // Must be a UID
                }

                // Set options
                DicomDirOptions options = dicomDir.Options;
                options.IncludeSubfolders = true;
                options.InsertIconImageSequence = false;
                options.RejectInvalidFileId = false;
                dicomDir.Options = options;

                // Add the DICOM files to the DICOM Directory.
                // This is the function that does it all!
                // You can always give the user feedback about the progress inside 
                // this function by overriding the function DicomDir.OnInsertFile.   
                dicomDir.InsertFile(null);
                dicomDir.Save();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #region 窗口关闭相关
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
        #endregion

        private void AddSeriesItem(DicomDataSet ds)
        {
            if (InvokeRequired)
            {
                Invoke(new AddSeriesItemDelegate(AddSeriesItem), ds);
            }
            else
            {
                ListViewItem item = new ListViewItem();
                item.ImageIndex = listViewSeries.Items.Count;
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("序列码：{0}", Utils.GetStringValue(ds, DemoDicomTags.SeriesNumber));

                sb.AppendLine();
                sb.AppendFormat("检查类型：", Utils.GetStringValue(ds, DemoDicomTags.Modality));
                sb.AppendLine();
                sb.AppendFormat("相关实例数量：", Utils.GetStringValue(ds, DemoDicomTags.NumberOfSeriesRelatedInstances));
                item.Text = sb.ToString();
                item.Tag = Utils.GetStringValue(ds, DemoDicomTags.SeriesInstanceUID);
                listViewSeries.Items.Add(item);
            }
        }

        /// <summary>
        /// studies列表双击事件，触发获取Study相关所有Series下所有Image的操作！
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listViewStudies_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listViewStudies.SelectedItems.Count == 0)
                return;
            patientId = listViewStudies.SelectedItems[0].SubItems[0].Text;
            studyInstance = listViewStudies.SelectedItems[0].Tag as string;
            if (!string.IsNullOrEmpty(studyInstance))
            {
                CFindQuery query = new CFindQuery();
                query.StudyInstanceUid = studyInstance;
                query.PatientID = listViewStudies.SelectedItems[0].SubItems[0].Text;
                cfind.Find(server, FindType.StudySeries, query, AETitle);
            }
            //创建存放dcm文件的文件夹
            workPath = Application.StartupPath + "\\dicomfile\\"+studyInstance;
            //Cursor = Cursors.WaitCursor;
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
    }
}