using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Leadtools;
using Leadtools.Controls;
using Leadtools.Dicom;
using Leadtools.DicomDemos;

namespace DICOMViewer.Utility
{
    class ImageConverter
    {
        void Convert(DicomDataSet ds)
        {
            DicomElement element;
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
                //ds.
                    
            }

        }

        void ConvertJpeg2000(DicomDataSet ds)
        {
            DicomJpeg2000Options options = ds.DefaultJpeg2000Options;

            Console.WriteLine("JPEG 2000 Options:");
            Console.WriteLine("DicomJpeg2000Options.UseColorTransform            is : {0}", options.UseColorTransform);
            Console.WriteLine("DicomJpeg2000Options.DerivedQuantization          is : {0}", options.DerivedQuantization);
            Console.WriteLine("DicomJpeg2000Options.TargetFileSize               is : {0}", options.TargetFileSize);
            Console.WriteLine("DicomJpeg2000Options.ImageAreaHorizontalOffset    is : {0}", options.ImageAreaHorizontalOffset);
            Console.WriteLine("DicomJpeg2000Options.ImageAreaVerticalOffset      is : {0}", options.ImageAreaVerticalOffset);
            Console.WriteLine("DicomJpeg2000Options.ReferenceTileWidth           is : {0}", options.ReferenceTileWidth);
            Console.WriteLine("DicomJpeg2000Options.ReferenceTileHeight          is : {0}", options.ReferenceTileHeight);
            Console.WriteLine("DicomJpeg2000Options.TileHorizontalOffset         is : {0}", options.TileHorizontalOffset);
            Console.WriteLine("DicomJpeg2000Options.TileVerticalOffset           is : {0}", options.TileVerticalOffset);
            Console.WriteLine("DicomJpeg2000Options.DecompositionLevels          is : {0}", options.DecompositionLevels);
            Console.WriteLine("DicomJpeg2000Options.ProgressingOrder             is : {0}", options.ProgressingOrder);
            Console.WriteLine("DicomJpeg2000Options.CodeBlockWidth               is : {0}", options.CodeBlockWidth);
            Console.WriteLine("DicomJpeg2000Options.CodeBlockHeight              is : {0}", options.CodeBlockHeight);
            Console.WriteLine("DicomJpeg2000Options.UseSopMarker                 is : {0}", options.UseSopMarker);
            Console.WriteLine("DicomJpeg2000Options.UseEphMarker                 is : {0}", options.UseEphMarker);
            Console.WriteLine("DicomJpeg2000Options.RegionOfInterest             is : {0}", options.RegionOfInterest);
            Console.WriteLine("DicomJpeg2000Options.UseRegionOfInterest          is : {0}", options.UseRegionOfInterest);
            Console.WriteLine("DicomJpeg2000Options.RegionOfInterestWeight       is : {0}", options.RegionOfInterestWeight);
            Console.WriteLine("DicomJpeg2000Options.RegionOfInterestRectangle    is : {0}", options.RegionOfInterestRectangle);

            options.CompressionControl = DicomJpeg2000CompressionControl.Ratio;
            options.CompressionRatio = 50;

            Console.WriteLine("Changed CompressionControl to DicomJpeg2000CompressionControl.Ratio and DicomJpeg2000CompressionControl.CompressionRatio to 50");
            //https://www.leadtools.com/help/leadtools/v19/dh/di/dicomdataset-setimage.html
            ds.Jpeg2000Options = options;
            ds.ChangeTransferSyntax(DicomUidType.JPEG2000, 2, ChangeTransferSyntaxFlags.None);
        }
    }
}
