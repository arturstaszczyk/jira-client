﻿using System.IO;
using System.Net;
using System.Windows.Media.Imaging;
using Yakuza.JiraClient.Api;
using Yakuza.JiraClient.Api.Messages.IO.Jira;
using Yakuza.JiraClient.Messaging.Api;

namespace Yakuza.JiraClient.IO.Jira
{
   public class ResourceDownloaderMicroservice : RestMicroserviceBase,
      IMicroService,
      IHandleMessage<DownloadPictureMessage>
   {
      public ResourceDownloaderMicroservice(IMessageBus messageBus, IConfiguration configuration)
         : base(configuration, messageBus)
      {
         _messageBus.Register(this);
      }

      public void Handle(DownloadPictureMessage message)
      {
         var request = (HttpWebRequest)WebRequest.Create(message.PictureUrl);
         if (string.IsNullOrEmpty(_configuration.JiraSessionId) == false)
         {
            request.CookieContainer = new CookieContainer();
            request.CookieContainer.Add(new Cookie("JSESSIONID", _configuration.JiraSessionId, "/", request.RequestUri.Host));
         }

         var response = (HttpWebResponse)request.GetResponse();

         using (Stream inputStream = response.GetResponseStream())
         using (Stream outputStream = new MemoryStream())
         {
            var buffer = new byte[4096];
            int bytesRead;
            do
            {
               bytesRead = inputStream.Read(buffer, 0, buffer.Length);
               outputStream.Write(buffer, 0, bytesRead);
            } while (bytesRead != 0);

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = outputStream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            _messageBus.Send(new DownloadPictureResponse(bitmapImage));
         }
      }
   }
}
