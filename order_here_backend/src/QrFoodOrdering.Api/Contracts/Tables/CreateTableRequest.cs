using System.ComponentModel.DataAnnotations;
using QrFoodOrdering.Application.Common.Validation;

namespace QrFoodOrdering.Api.Contracts.Tables;

public sealed class CreateTableRequest
{
    public CreateTableRequest() { }

    public CreateTableRequest(string code)
    {
        Code = code;
    }

    [Required(AllowEmptyStrings = false, ErrorMessage = RequestValidationMessages.TableCodeRequired)]
    public string Code { get; init; } = string.Empty;
}
