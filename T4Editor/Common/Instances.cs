using Microsoft.VisualStudio.ComponentModelHost;
using System;

namespace T4Editor.Common
{
    internal static class Instances
    {
        public static IServiceProvider ServiceProvider { get; set; }
        public static IComponentModel CompositionService { get; set; }
    }
}
