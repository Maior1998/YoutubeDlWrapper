using DevExpress.Mvvm.UI.Native;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace YoutubeDLWrapperView
{
    public static class HelperFuncs
    {
        public static async Task<BitmapImage> LoadWebP(string url)
        {
            HttpWebRequest request = WebRequest.CreateHttp(url);
            var response = request.GetResponse();
            var str = response.GetResponseStream();
            byte[] buffer = str.CopyAllBytes();
            Imazen.WebP.SimpleDecoder decoder = new Imazen.WebP.SimpleDecoder();
            System.Drawing.Bitmap bitmap = decoder.DecodeFromBytes(buffer, buffer.Length);
            BitmapImage bitmapImage = new BitmapImage();

            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Position = 0;

                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
            }

            return bitmapImage;
        }
    }
}
