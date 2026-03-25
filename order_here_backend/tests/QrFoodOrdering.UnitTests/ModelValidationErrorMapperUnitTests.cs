using Microsoft.AspNetCore.Mvc.ModelBinding;
using QrFoodOrdering.Api.Validation;
using QrFoodOrdering.Application.Common.Errors;
using QrFoodOrdering.Application.Common.Validation;
using QrFoodOrdering.Domain.Common;

namespace QrFoodOrdering.UnitTests;

public sealed class ModelValidationErrorMapperUnitTests
{
    [Fact]
    public void Map_with_empty_model_state_should_return_request_body_required()
    {
        var modelState = new ModelStateDictionary();

        var result = ModelValidationErrorMapper.Map(modelState);

        Assert.Equal(ApplicationErrorCodes.RequestBodyRequired, result.ErrorCode);
        Assert.Equal(RequestValidationMessages.RequestBodyRequired, result.Message);
    }

    [Fact]
    public void Map_with_non_empty_request_body_required_error_should_return_request_body_required()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError(string.Empty, "A non-empty request body is required.");

        var result = ModelValidationErrorMapper.Map(modelState);

        Assert.Equal(ApplicationErrorCodes.RequestBodyRequired, result.ErrorCode);
        Assert.Equal(RequestValidationMessages.RequestBodyRequired, result.Message);
    }

    [Fact]
    public void Map_with_table_id_json_conversion_error_should_return_table_id_invalid()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError(
            "$.tableId",
            "The JSON value could not be converted to System.Guid. Path: $.tableId"
        );

        var result = ModelValidationErrorMapper.Map(modelState);

        Assert.Equal(ApplicationErrorCodes.TableIdInvalid, result.ErrorCode);
        Assert.Equal(RequestValidationMessages.TableIdInvalid, result.Message);
    }

    [Fact]
    public void Map_with_menu_item_id_json_conversion_error_should_return_menu_item_id_invalid()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError(
            "$.items[0].menuItemId",
            "The JSON value could not be converted to System.Guid. Path: $.items[0].menuItemId"
        );

        var result = ModelValidationErrorMapper.Map(modelState);

        Assert.Equal(ApplicationErrorCodes.MenuItemIdInvalid, result.ErrorCode);
        Assert.Equal(RequestValidationMessages.MenuItemIdInvalid, result.Message);
    }

    [Fact]
    public void Map_with_quantity_json_conversion_error_should_return_invalid_quantity()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError(
            "$.quantity",
            "The JSON value could not be converted to System.Int32. Path: $.quantity"
        );

        var result = ModelValidationErrorMapper.Map(modelState);

        Assert.Equal(ApplicationErrorCodes.InvalidQuantity, result.ErrorCode);
        Assert.Equal(RequestValidationMessages.QuantityMustBeGreaterThanZero, result.Message);
    }

    [Fact]
    public void Map_with_unit_price_json_conversion_error_should_return_unit_price_invalid()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError(
            "$.unitPrice",
            "The JSON value could not be converted to System.Decimal. Path: $.unitPrice"
        );

        var result = ModelValidationErrorMapper.Map(modelState);

        Assert.Equal(ApplicationErrorCodes.UnitPriceInvalid, result.ErrorCode);
        Assert.Equal(RequestValidationMessages.UnitPriceMustBePositive, result.Message);
    }

    [Fact]
    public void Map_with_product_name_validation_error_should_return_product_name_required()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("ProductName", RequestValidationMessages.ProductNameRequired);

        var result = ModelValidationErrorMapper.Map(modelState);

        Assert.Equal(ApplicationErrorCodes.ProductNameRequired, result.ErrorCode);
        Assert.Equal(RequestValidationMessages.ProductNameRequired, result.Message);
    }

    [Fact]
    public void Map_with_create_table_code_validation_error_should_return_table_code_required()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Code", RequestValidationMessages.TableCodeRequired);

        var result = ModelValidationErrorMapper.Map(modelState);

        Assert.Equal(DomainErrorCodes.TableCodeRequired, result.ErrorCode);
        Assert.Equal(RequestValidationMessages.TableCodeRequired, result.Message);
    }

    [Fact]
    public void Map_with_generic_json_error_should_return_invalid_json()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("$", "The JSON value is not valid JSON.");

        var result = ModelValidationErrorMapper.Map(modelState);

        Assert.Equal(ApplicationErrorCodes.InvalidJson, result.ErrorCode);
        Assert.Equal(RequestValidationMessages.InvalidJson, result.Message);
    }

    [Fact]
    public void Map_with_unmapped_validation_error_should_fall_back_to_invalid_request()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("UnknownField", "Some custom validation failed.");

        var result = ModelValidationErrorMapper.Map(modelState);

        Assert.Equal(ApplicationErrorCodes.InvalidRequest, result.ErrorCode);
        Assert.Equal("Some custom validation failed.", result.Message);
    }
}
