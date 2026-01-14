viic@IVIC-MAC qr-food-ordering-platform % dotnet new sln -n QrFoodOrdering
The template "Solution File" was created successfully.
<!-- ####################################################################################################### -->
viic@IVIC-MAC qr-food-ordering-platform % dotnet build QrFoodOrdering.sln
Restore complete (0.8s)
  QrFoodOrdering.Domain succeeded (1.3s) → src/QrFoodOrdering.Domain/bin/Debug/net9.0/QrFoodOrdering.Domain.dll
  QrFoodOrdering.Application succeeded (0.2s) → src/QrFoodOrdering.Application/bin/Debug/net9.0/QrFoodOrdering.Application.dll
  QrFoodOrdering.Infrastructure failed with 3 error(s) (0.2s)
    /Users/viic/Desktop/project c# 7day/qr-food-ordering-platform/src/QrFoodOrdering.Infrastructure/DependencyInjection.cs(1,17): error CS0234: The type or namespace name 'Extensions' does not exist in the namespace 'Microsoft' (are you missing an assembly reference?)
    /Users/viic/Desktop/project c# 7day/qr-food-ordering-platform/src/QrFoodOrdering.Infrastructure/DependencyInjection.cs(7,61): error CS0246: The type or namespace name 'IServiceCollection' could not be found (are you missing a using directive or an assembly reference?)
    /Users/viic/Desktop/project c# 7day/qr-food-ordering-platform/src/QrFoodOrdering.Infrastructure/DependencyInjection.cs(7,19): error CS0246: The type or namespace name 'IServiceCollection' could not be found (are you missing a using directive or an assembly reference?)
  QrFoodOrdering.Tests succeeded (0.4s) → tests/QrFoodOrdering.Tests/bin/Debug/net9.0/QrFoodOrdering.Tests.dll

Build failed with 3 error(s) in 2.8s
viic@IVIC-MAC qr-food-ordering-platform % 
<!-- ####################################################################################################### -->
dotnet add src/QrFoodOrdering.Api/QrFoodOrdering.Api.csproj \
  package Swashbuckle.AspNetCore
dotnet restore
dotnet build QrFoodOrdering.sln

<!-- 6.2 -->
dotnet test QrFoodOrdering.sln
result
viic@IVIC-MAC qr-food-ordering-platform % dotnet test QrFoodOrdering.sln
Restore complete (0.4s)
  QrFoodOrdering.Domain succeeded (0.0s) → src/QrFoodOrdering.Domain/bin/Debug/net9.0/QrFoodOrdering.Domain.dll
  QrFoodOrdering.Application succeeded (0.0s) → src/QrFoodOrdering.Application/bin/Debug/net9.0/QrFoodOrdering.Application.dll
  QrFoodOrdering.Tests succeeded (0.0s) → tests/QrFoodOrdering.Tests/bin/Debug/net9.0/QrFoodOrdering.Tests.dll
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.8.2+699d445a1a (64-bit .NET 9.0.5)
[xUnit.net 00:00:00.04]   Discovering: QrFoodOrdering.Tests
[xUnit.net 00:00:00.05]   Discovered:  QrFoodOrdering.Tests
[xUnit.net 00:00:00.05]   Starting:    QrFoodOrdering.Tests
[xUnit.net 00:00:00.08]   Finished:    QrFoodOrdering.Tests
  QrFoodOrdering.Tests test succeeded (0.6s)

Test summary: total: 3, failed: 0, succeeded: 3, skipped: 0, duration: 0.5s
Build succeeded in 1.2s
viic@IVIC-MAC qr-food-ordering-platform % 
<!-- ####################################################################################################### -->
<!-- 6.3 รัน API (เช็คว่า start ได้) -->
dotnet run --project src/QrFoodOrdering.Api/QrFoodOrdering.Api.csproj
<!-- result -->
viic@IVIC-MAC qr-food-ordering-platform % dotnet run --project src/QrFoodOrdering.Api/QrFoodOrdering.Api.csproj
Using launch settings from src/QrFoodOrdering.Api/Properties/launchSettings.json...
Building...
Unhandled exception. System.Reflection.ReflectionTypeLoadException: Unable to load one or more of the requested types.
Could not load type 'Microsoft.OpenApi.Any.IOpenApiAny' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
Could not load type 'Microsoft.OpenApi.Models.OpenApiDiscriminator' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
Could not load type 'Microsoft.OpenApi.Models.OpenApiExternalDocs' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
Could not load type 'Microsoft.OpenApi.Models.OpenApiReference' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
Could not load type 'Microsoft.OpenApi.Models.OpenApiSchema' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
Could not load type 'Microsoft.OpenApi.Models.OpenApiTag' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
Could not load type 'Microsoft.OpenApi.Models.OpenApiXml' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
Could not load type 'Microsoft.OpenApi.Models.OpenApiDocument' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
Could not load type 'Microsoft.OpenApi.Models.OpenApiPathItem' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
Could not load type 'Microsoft.OpenApi.Models.OpenApiRequestBody' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
Could not load type 'Microsoft.OpenApi.Models.OpenApiRequestBody' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
Could not load type 'Microsoft.OpenApi.Models.OpenApiDocument' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
Could not load type 'Microsoft.OpenApi.Models.OpenApiPaths' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
Could not load type 'Microsoft.OpenApi.Models.OpenApiOperation' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
Could not load type 'Microsoft.OpenApi.Models.OperationType' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
Could not load type 'Microsoft.OpenApi.Models.OpenApiParameter' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
Could not load type 'Microsoft.OpenApi.Models.OpenApiRequestBody' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
Could not load type 'Microsoft.OpenApi.Models.OpenApiResponse' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
Could not load type 'Microsoft.OpenApi.Models.OpenApiResponses' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
Could not load type 'Microsoft.OpenApi.Models.OpenApiSchema' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
Could not load type 'Microsoft.OpenApi.Models.OpenApiDocument' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
   at System.Reflection.RuntimeModule.GetDefinedTypes()
   at Microsoft.AspNetCore.Mvc.Controllers.ControllerFeatureProvider.PopulateFeature(IEnumerable`1 parts, ControllerFeature feature)
   at Microsoft.AspNetCore.Mvc.ApplicationParts.ApplicationPartManager.PopulateFeature[TFeature](TFeature feature)
   at Microsoft.AspNetCore.Mvc.ApplicationModels.ControllerActionDescriptorProvider.GetControllerTypes()
   at Microsoft.AspNetCore.Mvc.ApplicationModels.ControllerActionDescriptorProvider.GetDescriptors()
   at Microsoft.AspNetCore.Mvc.ApplicationModels.ControllerActionDescriptorProvider.OnProvidersExecuting(ActionDescriptorProviderContext context)
   at Microsoft.AspNetCore.Mvc.Infrastructure.DefaultActionDescriptorCollectionProvider.UpdateCollection()
   at Microsoft.AspNetCore.Mvc.Infrastructure.DefaultActionDescriptorCollectionProvider.Initialize()
   at Microsoft.AspNetCore.Mvc.Infrastructure.DefaultActionDescriptorCollectionProvider.GetChangeToken()
   at Microsoft.Extensions.Primitives.ChangeToken.ChangeTokenRegistration`1..ctor(Func`1 changeTokenProducer, Action`1 changeTokenConsumer, TState state)
   at Microsoft.Extensions.Primitives.ChangeToken.OnChange(Func`1 changeTokenProducer, Action changeTokenConsumer)
   at Microsoft.AspNetCore.Mvc.Routing.ActionEndpointDataSourceBase.Subscribe()
   at Microsoft.AspNetCore.Builder.ControllerEndpointRouteBuilderExtensions.GetOrCreateDataSource(IEndpointRouteBuilder endpoints)
   at Microsoft.AspNetCore.Builder.ControllerEndpointRouteBuilderExtensions.MapControllers(IEndpointRouteBuilder endpoints)
   at Program.<Main>$(String[] args) in /Users/viic/Desktop/project c# 7day/qr-food-ordering-platform/src/QrFoodOrdering.Api/Program.cs:line 24
System.TypeLoadException: Could not load type 'Microsoft.OpenApi.Any.IOpenApiAny' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
System.TypeLoadException: Could not load type 'Microsoft.OpenApi.Models.OpenApiDiscriminator' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
System.TypeLoadException: Could not load type 'Microsoft.OpenApi.Models.OpenApiExternalDocs' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
System.TypeLoadException: Could not load type 'Microsoft.OpenApi.Models.OpenApiReference' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
System.TypeLoadException: Could not load type 'Microsoft.OpenApi.Models.OpenApiSchema' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
System.TypeLoadException: Could not load type 'Microsoft.OpenApi.Models.OpenApiTag' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
System.TypeLoadException: Could not load type 'Microsoft.OpenApi.Models.OpenApiXml' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
System.TypeLoadException: Could not load type 'Microsoft.OpenApi.Models.OpenApiDocument' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
System.TypeLoadException: Could not load type 'Microsoft.OpenApi.Models.OpenApiPathItem' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
System.TypeLoadException: Could not load type 'Microsoft.OpenApi.Models.OpenApiRequestBody' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
System.TypeLoadException: Could not load type 'Microsoft.OpenApi.Models.OpenApiRequestBody' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
System.TypeLoadException: Could not load type 'Microsoft.OpenApi.Models.OpenApiDocument' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
System.TypeLoadException: Could not load type 'Microsoft.OpenApi.Models.OpenApiPaths' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
System.TypeLoadException: Could not load type 'Microsoft.OpenApi.Models.OpenApiOperation' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
System.TypeLoadException: Could not load type 'Microsoft.OpenApi.Models.OperationType' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
System.TypeLoadException: Could not load type 'Microsoft.OpenApi.Models.OpenApiParameter' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
System.TypeLoadException: Could not load type 'Microsoft.OpenApi.Models.OpenApiRequestBody' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
System.TypeLoadException: Could not load type 'Microsoft.OpenApi.Models.OpenApiResponse' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
System.TypeLoadException: Could not load type 'Microsoft.OpenApi.Models.OpenApiResponses' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
System.TypeLoadException: Could not load type 'Microsoft.OpenApi.Models.OpenApiSchema' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
System.TypeLoadException: Could not load type 'Microsoft.OpenApi.Models.OpenApiDocument' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
viic@IVIC-MAC qr-food-ordering-platform % 
