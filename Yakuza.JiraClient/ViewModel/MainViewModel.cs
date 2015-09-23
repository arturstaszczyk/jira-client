using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Yakuza.JiraClient.Api;
using Yakuza.JiraClient.Controls;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Xps.Packaging;
using Telerik.Windows.Controls;
using Yakuza.JiraClient.Api.Messages.Navigation;
using Yakuza.JiraClient.Api.Messages.Actions.Authentication;
using Yakuza.JiraClient.Api.Model;
using System.Collections.Generic;
using Yakuza.JiraClient.Api.Messages.Actions;
using System;
using System.Reflection;

namespace Yakuza.JiraClient.ViewModel
{
   public class MainViewModel : GalaSoft.MvvmLight.ViewModelBase
   {
      private bool _isLoggedIn = false;

      private readonly IMessenger _messenger;
      private int _selectedDocumentPaneIndex;
      private int _selectedPropertyPaneIndex;

      public MainViewModel(IMessenger messenger)
      {
         _messenger = messenger;
         SaveXpsCommand = new RelayCommand(SaveXps, () => _isLoggedIn);

         _messenger.Register<LoggedInMessage>(this, LoadUi);
         _messenger.Register<LoggedOutMessage>(this, _ => SetIsLoggedOut());
         _messenger.Register<OpenConnectionTabMessage>(this, _ => FocusPropertyPane(ConnectionPropertyPane));
         _messenger.Register<FilteredIssuesListMessage>(this, DumpIssueList);

         DocumentPanes = new ObservableCollection<RadPane>();
         PropertyPanes = new ObservableCollection<RadPane> { ConnectionPropertyPane };
      }

      private void LoadUi(LoggedInMessage message)
      {
         DocumentPanes.Clear();
         DocumentPanes.Add(IssueListDocumentPane);
         DocumentPanes.Add(PivotDocumentPane);
         FocusDocumentPane(IssueListDocumentPane);

         PropertyPanes.Clear();
         PropertyPanes.Add(SearchPropertyPane);
         PropertyPanes.Add(PivotPropertyPane);
         PropertyPanes.Add(ConnectionPropertyPane);
         FocusPropertyPane(SearchPropertyPane);

         SetIsLoggedIn();
      }

      private void SetIsLoggedIn()
      {
         _isLoggedIn = true;
         RefreshCommands();
      }

      private void SetIsLoggedOut()
      {
         _isLoggedIn = false;
         RefreshCommands();
      }

      private void RefreshCommands()
      {
         foreach (var command in GetType().GetProperties().Where(p => p.PropertyType == typeof(RelayCommand)))
         {
            ((RelayCommand)command.GetValue(this)).RaiseCanExecuteChanged();
         }
      }

      private void SaveXps()
      {
         _messenger.Send(new GetFilteredIssuesListMessage());
      }

      private void DumpIssueList(FilteredIssuesListMessage message)
      {
         if (message.FilteredIssues == null || message.FilteredIssues.Any() == false)
         {
            _messenger.LogMessage("No issues to export.", LogLevel.Warning);
            return;
         }

         var document = CardsPrintPreview.GenerateDocument(message.FilteredIssues);
         var dlg = new Microsoft.Win32.SaveFileDialog();
         dlg.FileName = "Scrum Cards.xps";
         dlg.DefaultExt = ".xps";
         dlg.Filter = "XPS Documents (.xps)|*.xps";
         dlg.OverwritePrompt = true;

         var result = dlg.ShowDialog();

         if (result == false)
            return;

         var filename = dlg.FileName;
         if (File.Exists(filename))
            File.Delete(filename);

         using (var xpsd = new XpsDocument(filename, FileAccess.ReadWrite))
         {
            var xw = XpsDocument.CreateXpsDocumentWriter(xpsd);
            xw.Write(document);
            xpsd.Close();
         }
      }

      public RelayCommand SaveXpsCommand { get; private set; }
      public string AppTitle
      {
         get
         {
            return string.Format("Jira Client - {0}", Assembly.GetExecutingAssembly().GetName().Version.ToString(3));
         }
      }

      public int SelectedDocumentPaneIndex
      {
         get
         {
            return _selectedDocumentPaneIndex;
         }
         set
         {
            _selectedDocumentPaneIndex = value;
            RaisePropertyChanged();
            UpdatePropertiesPane();
         }
      }

      private void UpdatePropertiesPane()
      {
         switch (SelectedDocumentPaneIndex)
         {
            case 1: //PivotGrid
               FocusPropertyPane(_pivotPropertyPane);
               break;
         }
      }

      private void FocusPropertyPane(RadPane pane)
      {
         SelectedPropertyPaneIndex = PropertyPanes.IndexOf(pane);
      }

      private void FocusDocumentPane(RadPane pane)
      {
         SelectedDocumentPaneIndex = DocumentPanes.IndexOf(pane);
      }

      public int SelectedPropertyPaneIndex
      {
         get
         {
            return _selectedPropertyPaneIndex;
         }
         set
         {
            _selectedPropertyPaneIndex = value;
            RaisePropertyChanged();
         }
      }

      public ObservableCollection<RadPane> DocumentPanes { get; private set; }
      public ObservableCollection<RadPane> PropertyPanes { get; private set; }

      private RadPane _connectionPropertyPane;
      private RadPane _pivotPropertyPane;
      private RadPane _searchPropertyPane;

      private RadPane _issueListDocumentPane;
      private RadPane _pivotDocumentPane;

      private RadPane SearchPropertyPane
      {
         get
         {
            if (_searchPropertyPane == null)
               _searchPropertyPane = new RadPane { Header = "search", Content = new SearchIssues() };

            return _searchPropertyPane;
         }
      }

      private RadPane PivotPropertyPane
      {
         get
         {
            if (_pivotPropertyPane == null)
               _pivotPropertyPane = new RadPane { Header = "pivot", Content = new PivotReportingProperties() };

            return _pivotPropertyPane;
         }
      }

      private RadPane ConnectionPropertyPane
      {
         get
         {
            if (_connectionPropertyPane == null)
               _connectionPropertyPane = new RadPane { Header = "JIRA", Content = new ConnectionManager(), CanUserClose = false };

            return _connectionPropertyPane;
         }
      }

      private RadPane IssueListDocumentPane
      {
         get
         {
            if (_issueListDocumentPane == null)
               _issueListDocumentPane = new RadPane { Header = "issues", Content = new IssueListDisplay() };

            return _issueListDocumentPane;
         }
      }

      private RadPane PivotDocumentPane
      {
         get
         {
            if (_pivotDocumentPane == null)
               _pivotDocumentPane = new RadPane { Header = "pivot", Content = new PivotReportingGrid() };

            return _pivotDocumentPane;
         }
      }
   }
}