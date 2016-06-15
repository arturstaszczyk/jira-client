﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace JiraAssistant.Controls.Dialogs
{
   public partial class ColorDialog : INotifyPropertyChanged
   {
      private Color _editedColor;
      private static ObservableCollection<Color> _globalColorsHistory;

      public event PropertyChangedEventHandler PropertyChanged;

      public ColorDialog(Color editedColor)
      {
         InitializeComponent();
         EditedColor = editedColor;

         ColorEditor.HistoryCapacity = 5;

         if (_globalColorsHistory != null)
         {
            foreach (var color in _globalColorsHistory)
               ColorEditor.ColorHistory.Add(color);
         }
         _globalColorsHistory = ColorEditor.ColorHistory;

         ColorEditor.SelectedColor = EditedColor;
         ColorEditor.SelectedColorChanged += (sender, args) => EditedColor = args.Color;

         var mousePosition = Application.Current.MainWindow.PointToScreen(Mouse.GetPosition(null));
         this.Top = mousePosition.Y;
         this.Left = mousePosition.X;
      }

      public void SaveClicked(object sender, RoutedEventArgs args)
      {
         DialogResult = true;
      }

      public void CancelClicked(object sender, RoutedEventArgs args)
      {
         DialogResult = false;
      }

      public void HistoryButtonClicked(object sender, RoutedEventArgs args)
      {
         var button = (Button)sender;

         EditedColor = (Color)button.DataContext;
         ColorEditor.SelectedColor = EditedColor;
      }

      public Color EditedColor
      {
         get { return _editedColor; }
         set
         {
            _editedColor = value;
            if (PropertyChanged != null)
               PropertyChanged(this, new PropertyChangedEventArgs("EditedColor"));
         }
      }
   }
}
