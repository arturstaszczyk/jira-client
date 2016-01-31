﻿using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace JiraAssistant.Model.Ui
{
   public interface INavigationPage
   {
      Control Control { get; }
      ObservableCollection<ToolbarButton> Buttons { get; }
   }
}
