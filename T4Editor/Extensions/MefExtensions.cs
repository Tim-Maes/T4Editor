using System.ComponentModel.Composition;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;

public static class MefExtensions
{
    private static IComponentModel _compositionService;

    public static void SatisfyImportsOnce(this object o)
    {
        if (_compositionService == null)
        {
            _compositionService = ServiceProvider.GlobalProvider.GetService(typeof(SComponentModel)) as IComponentModel;
        }

        if (_compositionService != null)
        {
            _compositionService.DefaultCompositionService.SatisfyImportsOnce(o);
        }
    }
}