﻿namespace Yakuza.JiraClient.Api
{
   public interface IConfiguration
   {
      string JiraSessionId { get; set; }
      string JiraUrl { get; set; }
      string Username { get; set; }
   }
}