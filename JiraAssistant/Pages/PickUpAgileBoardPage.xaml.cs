﻿using System.Collections.ObjectModel;
using System.Windows.Controls;
using JiraAssistant.Domain.Ui;
using JiraAssistant.Logic.ContextlessViewModels;

namespace JiraAssistant.Pages
{
    public partial class PickUpAgileBoardPage : INavigationPage
   {
      public PickUpAgileBoardPage()
      {
         InitializeComponent();
      }

      public ObservableCollection<IToolbarItem> Buttons
      {
         get; private set;
      }

      public Control Control
      {
         get { return this; }
      }

      public Control StatusBarControl
      {
         get { return null; }
      }

      public void OnNavigatedFrom() { }

      public void OnNavigatedTo()
      {
         var viewModel = DataContext as AgileBoardSelectViewModel;
         viewModel.OnNavigatedTo();
      }

      public string Title { get { return "Board selection"; } }
   }
}
