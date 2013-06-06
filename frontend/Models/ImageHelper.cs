using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;


    public static class ImageHelper {

        public static ImageCodecInfo GetJpgEncoder()
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            ImageCodecInfo ici = null;
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.MimeType == "image/jpeg")
                {
                    ici = codec;
                }
            }

            return ici;
        }

        public static EncoderParameters GetJpgEncoderParameters(int quality=80)
        {
            EncoderParameters ep = new EncoderParameters();
            ep.Param[0] = new EncoderParameter(Encoder.Quality, (long)quality);
            return ep;
        }

        public static Image CropImage(Image Image, int Height, int Width) {
            return CropImage(Image, Height, Width, 0, 0);
        }
        public static Image CropImageFromCenter(Image Image, int Height, int Width) {
            int width = Image.Width;
            int height = Image.Height;

            int center_x = (width - Width)/2;
            int center_y = (height - Height)/2;

            if (height < Height)
                center_y = 0;

            if (width < Width)
                center_x = 0;

            return CropImage(Image, Height, Width, center_x, center_y);
        }
        public static Image CropImage(Image Image, int Height, int Width, int StartAtX, int StartAtY) {
            try {
                if (Image.Height < Height) {
                    Height = Image.Height;
                }

                if (Image.Width < Width) {
                    Width = Image.Width;
                }

                using (var bmPhoto = new Bitmap(Width, Height, PixelFormat.Format24bppRgb)) {
                    bmPhoto.SetResolution(72, 72);

                    Graphics grPhoto = Graphics.FromImage(bmPhoto) ;
                    grPhoto.SmoothingMode = SmoothingMode.AntiAlias;
                    grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    grPhoto.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    grPhoto.DrawImage(Image, new Rectangle(0, 0, Width, Height), StartAtX, StartAtY, Width, Height, GraphicsUnit.Pixel);

                    using (var mm = new MemoryStream()) {
                        bmPhoto.Save(mm, ImageFormat.Png);

                        Image outimage = Image.FromStream(mm);
                        return outimage;
                        
                    }
                    
                }
            }
            catch (Exception ex) {
                throw new Exception("Error cropping image, the error was: " + ex.Message);
            }
        }
        public static Image HardResizeImage(int Width, int Height, Image Image) {

            Image resized = null;
            resized = Width > Height ? ResizeImageWithMinimum(Height, Height, Image) : ResizeImageWithMinimum(Width, Width, Image);
            Image output = CropImageFromCenter(resized, Height, Width);
            return output;
        }
        public static Image ResizeImage(int maxWidth, int maxHeight, Image Image) {
            int width = Image.Width;
            int height = Image.Height;
            if (width > maxWidth || height > maxHeight) {
                Image.RotateFlip(RotateFlipType.Rotate180FlipX);
                Image.RotateFlip(RotateFlipType.Rotate180FlipX);

                float ratio = 0;
                if (width > height) {
                    ratio = width/(float) height;
                    width = maxWidth;
                    height = Convert.ToInt32(Math.Round(width/ratio));
                }
                else {
                    ratio = height/(float) width;
                    height = maxHeight;
                    width = Convert.ToInt32(Math.Round(height/ratio));
                }

                return Image.GetThumbnailImage(width, height, null, IntPtr.Zero);
            }

            return Image;
        }
        public static Image ResizeImageWithMinimum(int minWidth, int minHeight, Image Image)
        {
            int width = Image.Width;
            int height = Image.Height;


            if (width > minWidth || height > minHeight)
            {
                Image.RotateFlip(RotateFlipType.Rotate180FlipX);
                Image.RotateFlip(RotateFlipType.Rotate180FlipX);

                float ratio = 0;
                if (width > height)
                {
                    //height
                    ratio = width / (float)height;
                    height = minHeight;
                    width = Convert.ToInt32(Math.Round(height * ratio));
                }
                else
                {
                    //width
                    ratio = height / (float)width;
                    width = minWidth;
                    height = Convert.ToInt32(Math.Round(width * ratio));
                }

                return Image.GetThumbnailImage(width, height, null, IntPtr.Zero);
            }

            return Image;
        }

        public static Image GetResizedImage(this Image image, int Width, int Height, bool trim)
        {


            int origX, origY, newX, newY;
            int trimX = 0, trimY = 0;

            // Default to size of source image
            newX = origX = image.Width;
            newY = origY = image.Height;

            // Does image exceed maximum dimensions?
            if (origX > Width || origY > Height)
            {
                // Need to resize image
                if (trim)
                {
                    // Trim to exactly fit maximum dimensions
                    double factor = Math.Max((double)Width / (double)origX, (double)Height / (double)origY);
                    newX = (int)Math.Ceiling((double)origX * factor);
                    newY = (int)Math.Ceiling((double)origY * factor);
                    trimX = newX - Width;
                    trimY = newY - Height;
                }
                else
                {
                    // Resize (no trim) to keep within maximum dimensions
                    double factor = Math.Min((double)Width / (double)origX, (double)Height / (double)origY);
                    newX = (int)Math.Ceiling((double)origX * factor);
                    newY = (int)Math.Ceiling((double)origY * factor);
                }
            }

            // Create destination image
            Image dest = new Bitmap(newX - trimX, newY - trimY);

            Graphics graph = Graphics.FromImage(dest);
            graph.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graph.DrawImage(image, -(trimX / 2), -(trimY / 2), newX, newY);
            // Indicate success
            return dest;





        }

        public static Image Crop(this Image ThisImage, int maxHeight, int maxWidth) {
            return CropImage(ThisImage, maxHeight, maxWidth);
        }
        public static Image CropCenter(this Image ThisImage, int maxHeight, int maxWidth)
        {
            return CropImageFromCenter(ThisImage, maxHeight, maxWidth);
        }
        public static Image Crop(this Image ThisImage, int maxHeight, int maxWidth, int StartAtX, int StartAtY)
        {
            return CropImage(ThisImage, maxHeight, maxWidth, StartAtX,StartAtY);
        }
        public static Image Resize(this Image ThisImage, int Height, int Width) {
            return ResizeImage(Width, Height, ThisImage);
        }
        public static Image HardResize(this Image ThisImage, int Height, int Width) {
            return HardResizeImage(Width, Height, ThisImage);
        }

    }
