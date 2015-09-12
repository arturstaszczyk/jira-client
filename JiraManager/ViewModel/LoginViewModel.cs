using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using JiraManager.Api;
using JiraManager.Messages.Actions;
using JiraManager.Messages.Actions.Authentication;
using JiraManager.Service;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace JiraManager.ViewModel
{
   public class LoginViewModel : ViewModelBase
   {
      private readonly Configuration _configuration;
      private readonly IMessenger _messenger;
      private readonly IJiraOperations _operations;
      private bool _isBusy = false;
      private bool _isConnected;
      private bool _isDisconnected;

      public LoginViewModel(IMessenger messenger, Configuration configuration, IJiraOperations operations)
      {
         _messenger = messenger;
         _configuration = configuration;
         _operations = operations;

         var checkLoginTimer = new DispatcherTimer();
         checkLoginTimer.Interval = TimeSpan.FromMilliseconds(100);
         checkLoginTimer.Tick += async (s, a) =>
         {
            checkLoginTimer.IsEnabled = false;
            var sessionInfo = await _operations.CheckSession();
            if (sessionInfo.IsLoggedIn)
            {
               _messenger.Send(new LoggedInMessage());
               _configuration.IsLoggedIn = true;
               IsConnected = true;
               _messenger.LogMessage("Logged in using existing security token.");
            }
            else
            {
               _messenger.Send(new LoggedOutMessage());
               _configuration.IsLoggedIn = false;
               IsConnected = false;
            }
         };
         checkLoginTimer.IsEnabled = true;
         _messenger.Register<ConnectionIsBroken>(this, OnConnectionBroken);

         LoginCommand = new RelayCommand<PasswordBox>(async password => await Login(((PasswordBox)password).Password), p => _isBusy == false);
         LogoutCommand = new RelayCommand(async () => await Logout(), () => _isBusy == false);

         IsConnected = false;
      }

      private void OnConnectionBroken(ConnectionIsBroken message)
      {
         _messenger.LogMessage("Connection is broken. Security token might have been invalidated.");
         if (IsConnected)
         {
            _messenger.Send(new LoggedOutMessage());
         }
         IsConnected = false;
      }

      private async Task Login(string password)
      {
         _isBusy = true;
         LoginCommand.RaiseCanExecuteChanged();

         try
         {
            _messenger.LogMessage("Trying to log in JIRA: " + JiraUrl);
            var result = await _operations.TryToLogin(Username, password);
            if (result.WasSuccessful)
            {
               IsConnected = true;
               _messenger.Send(new LoggedInMessage());
               _messenger.LogMessage("Logged in successfully!", LogLevel.Info);
            }
            else
            {
               IsConnected = false;
               _messenger.Send(new LoggedOutMessage());
               _messenger.LogMessage("Failed to log in! Reason: " + result.ErrorMessage, LogLevel.Warning);
            }
         }
         catch (Exception e)
         {
            _messenger.LogMessage("Stack Trace: " + Environment.NewLine + e.StackTrace, LogLevel.Debug);
            _messenger.LogMessage("Unhandled exception occured during logging in: " + e.Message, LogLevel.Critical);
         }

         _isBusy = false;
         LoginCommand.RaiseCanExecuteChanged();
      }

      private async Task Logout()
      {
         _isBusy = true;
         LogoutCommand.RaiseCanExecuteChanged();

         try
         {
            _messenger.LogMessage("Logging out...");
            await _operations.Logout();
            IsConnected = false;
            _messenger.Send(new LoggedOutMessage());
            _messenger.LogMessage("Logged out successfully", LogLevel.Info);

         }
         catch (Exception e)
         {
            _messenger.LogMessage("Stack Trace: " + Environment.NewLine + e.StackTrace, LogLevel.Debug);
            _messenger.LogMessage("Unhandled exception occured during logging out: " + e.Message, LogLevel.Critical);
         }

         _isBusy = false;
         LogoutCommand.RaiseCanExecuteChanged();
      }
      
      public string JiraUrl
      {
         get { return _configuration.JiraUrl; }
         set
         {
            _configuration.JiraUrl = value;
            RaisePropertyChanged();
         }
      }

      public string Username
      {
         get { return _configuration.Username; }
         set
         {
            _configuration.Username = value;
            RaisePropertyChanged();
         }
      }

      public bool IsConnected
      {
         get
         {
            return _isConnected;
         }
         set
         {
            _isConnected = value;
            _isDisconnected = !value;

            RaisePropertyChanged();
            RaisePropertyChanged(() => IsDisconnected);
         }
      }

      public bool IsDisconnected
      {
         get
         {
            return _isDisconnected;
         }
         set
         {
            _isDisconnected = value;
            _isConnected = !value;

            RaisePropertyChanged();
            RaisePropertyChanged(() => IsConnected);
         }
      }

      public RelayCommand<PasswordBox> LoginCommand { get; private set; }
      public RelayCommand LogoutCommand { get; private set; }
   }
}