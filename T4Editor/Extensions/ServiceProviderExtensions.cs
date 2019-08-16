using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace T4Editor.Extensions
{
    internal static class ServiceProviderExtensions
    {
        internal static void SatisfyImportsOnce(this IServiceProvider serviceProvider, object objectToCompose)
        {
            IComponentModel compositionService = Common.Instances.CompositionService;

            if (compositionService == null)
            {
                compositionService = serviceProvider.GetService<SComponentModel, IComponentModel>();
            }

            if (compositionService != null)
            {
                compositionService.DefaultCompositionService.SatisfyImportsOnce(objectToCompose);
                if (Common.Instances.CompositionService == null) Common.Instances.CompositionService = compositionService;
            }
        }

        internal static async System.Threading.Tasks.Task SatisfyImportsOnceAsync(this IServiceProvider serviceProvider, object objectToCompose)
        {
            IComponentModel compositionService = Common.Instances.CompositionService;

            if (compositionService == null)
            {
                compositionService = await GetComponentModelAsync(serviceProvider);
            }

            if (compositionService != null)
            {
                compositionService.DefaultCompositionService.SatisfyImportsOnce(objectToCompose);
                if (Common.Instances.CompositionService == null) Common.Instances.CompositionService = compositionService;
            }
        }

        private static T GetService<T>(this IServiceProvider serviceProvider) where T : class
        {
            return serviceProvider.GetService(typeof(T)) as T;
        }

        private static TReturn GetService<TGet, TReturn>(this IServiceProvider serviceProvider)
            where TGet : class
            where TReturn : class
        {
            return serviceProvider.GetService(typeof(TGet)) as TReturn;
        }

        private static async Task<IComponentModel> GetComponentModelAsync(IServiceProvider serviceProvider)
        {
            var asyncProvider = serviceProvider as IAsyncServiceProvider;
            if (asyncProvider == null) return null;
            return await asyncProvider.GetServiceAsync<SComponentModel, IComponentModel>();
        }

        private static async Task<TReturn> GetServiceAsync<TGet, TReturn>(this IAsyncServiceProvider asyncServiceProvider)
            where TGet : class
            where TReturn : class
        {
            return await asyncServiceProvider.GetServiceAsync(typeof(TGet)) as TReturn;
        }
    }
}
