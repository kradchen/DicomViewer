using System;
using System.Windows.Forms;
using Leadtools.Dicom;

namespace DicomDemo
{
    public class UserDicomDir : DicomDir
    {
        // Private members
        private int m_nAddedDicomFilesCount;

        // Constructors
        public UserDicomDir() : base() 
        {
            m_nAddedDicomFilesCount = 0;
        }
        
        public new void Reset(string destinationFolder)
        {
            m_nAddedDicomFilesCount = 0;

            base.Reset(destinationFolder);
        }

        public new void Load(string name, DicomDataSetLoadFlags flags)
        {
            base.Load(name, flags);
        }

        public override DicomDirInsertFileCommand OnInsertFile(string fileName, DicomDataSet ds, DicomDirInsertFileStatus status, DicomExceptionCode code)
        {
            if (status == DicomDirInsertFileStatus.PreAdd)
            {
                // About to add the DICOM file
                if (ds.InformationClass == DicomClassType.BasicDirectory)
                {
                    return DicomDirInsertFileCommand.Skip;
                }
                else
                {
                }

            }
            else if (status == DicomDirInsertFileStatus.Success)
            {
                // The DICOM file has been added successfully
                m_nAddedDicomFilesCount++;
            }
            else // Failure
            {
                DialogResult dlgRes = MessageBox.Show("Invalid File Id: " + fileName + "\n Do you want to skip this file and continue?", "Error", MessageBoxButtons.YesNo);
                if (dlgRes == DialogResult.Yes)
                {
                    return DicomDirInsertFileCommand.Skip;
                }
                else
                {
                    return DicomDirInsertFileCommand.Stop;
                }
            }

            Application.DoEvents();
            return DicomDirInsertFileCommand.Continue;
        }

        /*
         * Returns the number of Dicom files added to the DicomDir
         */
        public int GetAddedDicomFilesCount()
        {
            return m_nAddedDicomFilesCount;
        }

    }
}