using GalaSoft.MvvmLight.Messaging;
using Autofac;
using System.Reflection;
using Yakuza.JiraClient.Messaging;
using Yakuza.JiraClient.Api;

namespace Yakuza.JiraClient.ViewModel
{
   public class ViewModelLocator
   {
      public ViewModelLocator()
      {
         BuildIocContainer();
      }

      public MainViewModel Main
      {
         get
         {
            return IocContainer.Resolve<MainViewModel>();
         }
      }

      public LogViewModel Logging
      {
         get
         {
            return IocContainer.Resolve<LogViewModel>();
         }
      }

      public ConnectionViewModel Connection
      {
         get
         {
            return IocContainer.Resolve<ConnectionViewModel>();
         }
      }

      public SearchIssuesViewModel Search
      {
         get
         {
            return IocContainer.Resolve<SearchIssuesViewModel>();
         }
      }

      public IssueListViewModel IssueList
      {
         get
         {
            return IocContainer.Resolve<IssueListViewModel>();
         }
      }

      public PivotGridViewModel Pivot
      {
         get
         {
            return IocContainer.Resolve<PivotGridViewModel>();
         }
      }

      public static void Cleanup()
      {
         // TODO Clear the ViewModels
      }

      internal IContainer IocContainer { get; private set; }

      private void BuildIocContainer()
      {
         var builder = new ContainerBuilder();

         var clientAssembly = Assembly.Load("Jira Client");
         var ioAssembly = Assembly.Load("Yakuza.JiraClient.IO");

         builder.RegisterType<MessageBus>()
            .AsImplementedInterfaces()
            .SingleInstance();
         
         //Register ViewModels
         builder.RegisterAssemblyTypes(clientAssembly)
            .Where(t => t.Name.EndsWith("ViewModel"))
            .AsSelf()
            .SingleInstance();

         //Register and run background services
         builder.RegisterAssemblyTypes(clientAssembly)
            .InNamespace("Yakuza.JiraClient.Service")
            .AsImplementedInterfaces()
            .AsSelf()
            .SingleInstance()
            .AutoActivate();
         builder.RegisterAssemblyTypes(ioAssembly)
            .AssignableTo<IMicroservice>()
            .AsSelf()
            .SingleInstance()
            .AutoActivate();

         IocContainer = builder.Build();
      }
   }
}