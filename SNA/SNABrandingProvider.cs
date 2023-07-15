using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace SNA;

[Dependency(ReplaceServices = true)]
public class SNABrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "SNA";
}
