using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace QrFoodOrdering.Api.Infrastructure;

public sealed class ExcludeControllerFeatureProvider(params Type[] excludedControllers)
    : ControllerFeatureProvider
{
    private readonly HashSet<string> _excludedControllerNames = excludedControllers
        .Select(x => x.FullName)
        .OfType<string>()
        .ToHashSet(StringComparer.Ordinal);

    protected override bool IsController(TypeInfo typeInfo)
    {
        if (!base.IsController(typeInfo))
            return false;

        var fullName = typeInfo.AsType().FullName;
        return fullName is null || !_excludedControllerNames.Contains(fullName);
    }
}
