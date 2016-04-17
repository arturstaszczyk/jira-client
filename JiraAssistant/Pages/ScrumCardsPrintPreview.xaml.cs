﻿using GalaSoft.MvvmLight.Command;
using JiraAssistant.Model.Jira;
using System.Collections.Generic;
using System.Linq;
using System;
using JiraAssistant.Dialogs;
using JiraAssistant.Model.Ui;
using System.Windows.Media.Imaging;
using System.Windows.Xps.Packaging;
using System.IO;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Markup;
using JiraAssistant.Controls;
using Autofac;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using JiraAssistant.Services.Jira;

namespace JiraAssistant.Pages
{
   public partial class ScrumCardsPrintPreview : BaseNavigationPage
   {
      private const int Rows = 5;
      private const int Columns = 3;
      private int _rightPageIndex;
      private readonly IContainer _iocContainer;
      private readonly string[] _defaultIssueTypes = { "user story", "bug", "story bug", "story" };
      private int _cardsCount;

      public ScrumCardsPrintPreview(IEnumerable<JiraIssue> issues, IContainer iocContainer)
      {
         InitializeComponent();

         _iocContainer = iocContainer;

         Pages = new ObservableCollection<PrintPreviewPage>();

         Issues = issues;

         AllCardsCount = issues.Count();
         StatusBarControl = new ScrumCardsPrintPreviewStatusBar();
         ExportCardsCommand = new RelayCommand(ExportCards);
         PreviousPageCommand = new RelayCommand(() => RightPageIndex -= 2, () => RightPageIndex > 1);
         NextPageCommand = new RelayCommand(() => RightPageIndex += 2, () => RightPageIndex < Pages.Count - 1);

         GetIssueTypes();

         Buttons.Add(new ToolbarButton
         {
            Tooltip = "Export cards...",
            Command = ExportCardsCommand,
            Icon = new BitmapImage(new Uri(@"pack://application:,,,/;component/Assets/Icons/ExportIcon.png"))
         });
         Buttons.Add(new ToolbarControl
         {
            Control = new ScrumCardsPrintPreviewPageIndicator { DataContext = this }
         });
         Buttons.Add(new ToolbarControl
         {
            Control = new ScrumCardsPrintPreviewIssueTypeFilter { DataContext = this }
         });

         DataContext = this;
      }

      private void GeneratePreviewPages()
      {
         Pages.Clear();
         var issuesLeft = Issues;

         if (AvailableIssueTypes != null)
            issuesLeft = issuesLeft.Where(i => AvailableIssueTypes.Where(t => t.IsSelected).Select(t => t.IssueType.Name).Contains(i.BuiltInFields.IssueType.Name));

         AllCardsCount = issuesLeft.Count();
         while (issuesLeft.Any())
         {
            var issuesForPage =
               issuesLeft.Take(Rows * Columns)
                         .Select((issue, index) => new JiraIssuePrintPreviewModel
                         {
                            Issue = issue,
                            Column = index % Columns,
                            Row = index / Columns
                         });
            issuesLeft = issuesLeft.Skip(Rows * Columns);

            Pages.Add(new PrintPreviewPage(issuesForPage));
         }
      }

      private async void GetIssueTypes()
      {
         var jiraApi = _iocContainer.Resolve<IJiraApi>();
         var issueTypes = (await jiraApi.Server.GetIssueTypes())
            .Select(i => new SelectableIssueType(i)
            {
               IsSelected = _defaultIssueTypes.Contains(i.Name.ToLower())
            }).ToList();

         foreach (var issueType in issueTypes)
            issueType.PropertyChanged += (sender, args) => GeneratePreviewPages();

         AvailableIssueTypes = issueTypes;
         RaisePropertyChanged("AvailableIssueTypes");

         GeneratePreviewPages();
      }

      private void ExportCards()
      {
         var dlg = new Microsoft.Win32.SaveFileDialog();
         dlg.FileName = "Scrum Cards.xps";
         dlg.DefaultExt = ".xps";
         dlg.Filter = "XPS Documents (.xps)|*.xps";
         dlg.OverwritePrompt = true;

         var result = dlg.ShowDialog();

         if (result == false)
            return;

         var document = GenerateDocument();
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

      private FixedDocument GenerateDocument()
      {
         var document = new FixedDocument();
         var pageSize = new Size(8.5 * 96.0, 11.0 * 96.0);

         foreach (var page in Pages)
         {
            var pageContent = new PageContent();
            var fixedPage = new FixedPage();
            var pagePreview = new ScrumCardsPageControl { DataContext = page };
            pagePreview.Height = pageSize.Height - 10;
            pagePreview.Width = pageSize.Width - 10;
            pagePreview.Margin = new Thickness(5);
            pagePreview.UpdateLayout();

            fixedPage.Children.Add(pagePreview);
            ((IAddChild) pageContent).AddChild(fixedPage);
            document.Pages.Add(pageContent);
         }

         return document;
      }

      public RelayCommand ExportCardsCommand { get; private set; }

      public ObservableCollection<PrintPreviewPage> Pages { get; private set; }

      public int RightPageIndex
      {
         get { return _rightPageIndex; }
         set
         {
            _rightPageIndex = value * 2 - 1;
            RaisePropertyChanged();
            PreviousPageCommand.RaiseCanExecuteChanged();
            NextPageCommand.RaiseCanExecuteChanged();
         }
      }

      public IEnumerable<SelectableIssueType> AvailableIssueTypes { get; private set; }

      public int AllCardsCount
      {
         get { return _cardsCount; }
         set
         {
            _cardsCount = value;
            RaisePropertyChanged();
         }
      }
      public IEnumerable<JiraIssue> Issues { get; private set; }
      public RelayCommand PreviousPageCommand { get; private set; }
      public RelayCommand NextPageCommand { get; private set; }
      public override string Title { get { return "Scrum cards"; } }
   }

   public class PrintPreviewPage
   {
      public PrintPreviewPage(IEnumerable<JiraIssuePrintPreviewModel> issuesForPage)
      {
         Issues = issuesForPage;
         SetCardColorCommand = new RelayCommand<JiraIssuePrintPreviewModel>(SetCardColor);
      }

      private void SetCardColor(JiraIssuePrintPreviewModel printPreview)
      {
         var dialog = new ColorDialog(printPreview.CategoryColor);

         if (dialog.ShowDialog() == false)
            return;

         printPreview.CategoryColor = dialog.EditedColor;
      }

      public IEnumerable<JiraIssuePrintPreviewModel> Issues { get; private set; }
      public RelayCommand<JiraIssuePrintPreviewModel> SetCardColorCommand { get; private set; }
   }

   public class SelectableIssueType : ViewModelBase
   {
      public SelectableIssueType(RawIssueType issueType)
      {
         IssueType = issueType;
      }

      private bool _isSelected;

      public bool IsSelected
      {
         get { return _isSelected; }
         set
         {
            _isSelected = value;
            RaisePropertyChanged();
         }
      }

      public RawIssueType IssueType { get; private set; }
   }
}
