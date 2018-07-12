using System;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;


namespace VSPing.Models
{
    class ImageEditor
    {
        /// <summary>
        /// This class handles several different operations related to the modification of the images searched, including cropping, rotating, and resizing
        /// </summary>
        // Rotates the input image by theta degrees around center.
        public static Bitmap RotateImage(Bitmap bmpSrc, float theta) // This method handles the rotation of a local image by a specified angle
        {
            Matrix mRotate = new Matrix();
            mRotate.Translate(bmpSrc.Width / -2, bmpSrc.Height / -2, MatrixOrder.Append);
            mRotate.RotateAt(theta, new System.Drawing.Point(0, 0), MatrixOrder.Append);
            using (GraphicsPath gp = new GraphicsPath())
            {  // transform image points by rotation matrix
                gp.AddPolygon(new System.Drawing.Point[] { new System.Drawing.Point(0, 0), new System.Drawing.Point(bmpSrc.Width, 0), new System.Drawing.Point(0, bmpSrc.Height) });
                gp.Transform(mRotate);
                System.Drawing.PointF[] pts = gp.PathPoints;

                // create destination bitmap sized to contain rotated source image
                Rectangle bbox = boundingBox(bmpSrc, mRotate);
                Bitmap bmpDest = new Bitmap(bbox.Width, bbox.Height);

                using (Graphics gDest = Graphics.FromImage(bmpDest))
                {  // draw source into dest
                    Matrix mDest = new Matrix();
                    mDest.Translate(bmpDest.Width / 2, bmpDest.Height / 2, MatrixOrder.Append);
                    gDest.Transform = mDest;
                    gDest.DrawImage(bmpSrc, pts);
                    return bmpDest;
                }
            }
        }

        private static Rectangle boundingBox(Image img, Matrix matrix) // This method crops an image to a specified bounding box
        {
            GraphicsUnit gu = new GraphicsUnit();
            Rectangle rImg = Rectangle.Round(img.GetBounds(ref gu));

            // Transform the four points of the image, to get the resized bounding box.
            System.Drawing.Point topLeft = new System.Drawing.Point(rImg.Left, rImg.Top);
            System.Drawing.Point topRight = new System.Drawing.Point(rImg.Right, rImg.Top);
            System.Drawing.Point bottomRight = new System.Drawing.Point(rImg.Right, rImg.Bottom);
            System.Drawing.Point bottomLeft = new System.Drawing.Point(rImg.Left, rImg.Bottom);
            System.Drawing.Point[] points = new System.Drawing.Point[] { topLeft, topRight, bottomRight, bottomLeft };
            GraphicsPath gp = new GraphicsPath(points, new byte[] { (byte)PathPointType.Start, (byte)PathPointType.Line, (byte)PathPointType.Line, (byte)PathPointType.Line });
            gp.Transform(matrix);
            return Rectangle.Round(gp.GetBounds());
        }

        public static string RotateImage(string srcImageFile, float theta) // This method handles the location of an image loaded from the internet
        {
            if (theta == 0.0) // If there's no rotation needed, just output the original image
                return srcImageFile;

            using (var srcBitmap = new Bitmap(srcImageFile)) // Create a new rotated version of the image
            {
                var rotatedBitmap = ImageEditor.RotateImage(srcBitmap, theta);
                var retValString = Path.GetTempFileName();

                rotatedBitmap.Save(retValString, ImageFormat.Jpeg);
                rotatedBitmap.Dispose();

                return retValString;
            }
        }

        public static Bitmap ResizeImage(Image image, int width, int height) // This method handles the resizing of an image to appropriate sizes
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private static bool FixImageOrientation(Image img) // This method rotates a searched image if necessary
        {
            bool modified = false;
            try
            {               
                if (Array.IndexOf(img.PropertyIdList, 274) > -1) // Makes sure the index of the entry is valid
                {
                    var orientation = (int)img.GetPropertyItem(274).Value[0];
                    switch (orientation) 
                    {
                        case 1:
                            // No rotation required.
                            break;
                        case 2:
                            img.RotateFlip(RotateFlipType.RotateNoneFlipX);
                            modified = true;
                            break;
                        case 3:
                            img.RotateFlip(RotateFlipType.Rotate180FlipNone);
                            modified = true;
                            break;
                        case 4:
                            img.RotateFlip(RotateFlipType.Rotate180FlipX);
                            modified = true;
                            break;
                        case 5:
                            img.RotateFlip(RotateFlipType.Rotate90FlipX);
                            modified = true;
                            break;
                        case 6:
                            img.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            modified = true;
                            break;
                        case 7:
                            img.RotateFlip(RotateFlipType.Rotate270FlipX);
                            modified = true;
                            break;
                        case 8:
                            img.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            modified = true;
                            break;
                    }

                    // If orientation was corrected, this EXIF data is now invalid and should be removed.
                    if (orientation != 1)
                    {                      
                        img.RemovePropertyItem(274);
                    }                    
                }
            } catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            return modified;
        }

        public static Tuple<string, bool> ResizeIfBiggerAndFixOrientation(string srcImageFile, int targetWidthHeight = 1000) // This method checks if an image needs to be resized or rotated and does so
        {
            using (var srcBitmap = new Bitmap(srcImageFile))
            {
                int srcWidth = srcBitmap.Width;
                int srcHeight = srcBitmap.Height;

                bool imageNeedsResize = false;
                int n = srcWidth > srcHeight ? srcWidth : srcHeight;
                if (n > targetWidthHeight) { imageNeedsResize = true; } // for images smaller than target set, target is updated to the larger dimension

                bool imageRotated = ImageEditor.FixImageOrientation(srcBitmap);

                if (imageNeedsResize || imageRotated) // If the image needs to be resized or rotated, do so
                {
                    var modifiedBitmap = srcBitmap;  // modifiedBitmap is either already rotated and/or needs resizing

                    if (imageNeedsResize) // Resize the image if needed
                    {
                        // re-read dimensions
                        srcWidth = modifiedBitmap.Width;
                        srcHeight = modifiedBitmap.Height;

                        n = srcWidth > srcHeight ? srcWidth : srcWidth;

                        int dstWidth = srcWidth * targetWidthHeight / n;
                        int dstHeight = srcHeight * targetWidthHeight / n;

                        modifiedBitmap = ImageEditor.ResizeImage(modifiedBitmap, dstWidth, dstHeight);
                    }

                    var retValString = Path.GetTempFileName();
                    modifiedBitmap.Save(retValString, ImageFormat.Jpeg);
                    modifiedBitmap.Dispose();

                    return new Tuple<string,bool>(retValString, true);
                }

                // no rotation or resize required
                return new Tuple<string,bool>(srcImageFile, false);
            }
        }
    }
}