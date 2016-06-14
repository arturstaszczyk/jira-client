﻿using JiraAssistant.Logic.Settings;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace JiraAssistant.Logic.Services.Resources
{
   public class ResourceDownloader : BaseRestService
   {
      public ResourceDownloader(AssistantSettings configuration)
         : base(configuration)
      {
      }

      public async Task<ImageSource> DownloadPicture(string imageUri)
      {
         var request = (HttpWebRequest)WebRequest.Create(imageUri);
         if (string.IsNullOrEmpty(Configuration.SessionCookies) == false)
         {
            request.CookieContainer = new CookieContainer();
            request.CookieContainer.SetCookies(request.RequestUri, Configuration.SessionCookies);
         }

         var response = (HttpWebResponse)(await request.GetResponseAsync());

         using (Stream inputStream = response.GetResponseStream())
         using (Stream outputStream = new MemoryStream())
         {
            var buffer = new byte[4096];
            int bytesRead;
            do
            {
               bytesRead = await inputStream.ReadAsync(buffer, 0, buffer.Length);
               outputStream.Write(buffer, 0, bytesRead);
            } while (bytesRead != 0);

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = outputStream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
         }
      }
   }
}
