using System.Reflection;
using System.Web.Mvc;
using Dashboard;
using Dashboard.DataAccess;
using Microsoft.Practices.Unity;
using Unity.Mvc5;

namespace WebApplication.App_Start
{
    public class UnityConfig
    {
        public static void RegisterComponents(IUnityContainer container)
        {
            container.RegisterTypes(
                AllClasses.FromAssemblies(Assembly.GetAssembly(typeof(UnityConfig)),
                    Assembly.GetAssembly(typeof(Historian))),
                WithMappings.FromMatchingInterface, WithName.Default);
            
            container.RegisterType<IWorkItemRepository, WorkItemRepository>(new InjectionFactory(c => new WorkItemRepositoryFactory().CreateRepository()));
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}