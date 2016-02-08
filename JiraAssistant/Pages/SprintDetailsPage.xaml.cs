﻿using GalaSoft.MvvmLight.Command;
using JiraAssistant.Model.Jira;
using System.Collections.Generic;
using System.Windows.Input;
using System;
using JiraAssistant.Services;

namespace JiraAssistant.Pages
{
   public partial class SprintDetailsPage : BaseNavigationPage
   {
      private readonly IEnumerable<JiraIssue> _issues;
      private readonly INavigator _navigator;

      public SprintDetailsPage(RawAgileSprint sprint, IEnumerable<JiraIssue> issues, INavigator navigator)
      {
         InitializeComponent();

         Sprint = sprint;
         _issues = issues;
         _navigator = navigator;

         ScrumCardsCommand = new RelayCommand(OpenScrumCards);
         BurnDownCommand = new RelayCommand(OpenBurnDownChart);
         EngagementCommand = new RelayCommand(OpenEngagementChart);

         DataContext = this;
      }

      private void OpenEngagementChart()
      {
         _navigator.NavigateTo(new EngagementChart(_issues));
      }

      private void OpenBurnDownChart()
      {
         _navigator.NavigateTo(new BurnDownChart(Sprint, _issues));
      }

      private void OpenScrumCards()
      {
         _navigator.NavigateTo(new ScrumCardsPrintPreview(_issues));
      }

      public ICommand ScrumCardsCommand { get; private set; }
      public ICommand BurnDownCommand { get; private set; }
      public ICommand EngagementCommand { get; private set; }
      public RawAgileSprint Sprint { get; private set; }
   }
}
