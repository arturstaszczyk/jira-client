﻿using JiraAssistant.Model.Jira;
using System.Collections.Generic;

namespace JiraAssistant.Model.NavigationMessages
{
   public class OpenBurnDownMessage
   {
      public OpenBurnDownMessage(RawAgileSprint sprint, IList<JiraIssue> issues)
      {
         Issues = issues;
         Sprint = sprint;
      }
      
      public IList<JiraIssue> Issues { get; private set; }
      public RawAgileSprint Sprint { get; private set; }
   }
}
