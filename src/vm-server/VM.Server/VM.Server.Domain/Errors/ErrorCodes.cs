namespace VM.Server.Domain.Errors;

public static class ErrorCodes
{
    public const string InvalidDenomination = "INVALID_DENOMINATION";
    public const string InsufficientFunds = "INSUFFICIENT_FUNDS";
    public const string OutOfStock = "OUT_OF_STOCK";
    public const string ChangeUnavailable = "CHANGE_UNAVAILABLE";
    public const string ProductNotFound = "PRODUCT_NOT_FOUND";
    public const string InvalidQuantity = "INVALID_QUANTITY";
    public const string InvalidPrice = "INVALID_PRICE";
    public const string InvalidProduct = "INVALID_PRODUCT";
    public const string DuplicateProduct = "DUPLICATE_PRODUCT";
    public const string DuplicatePrice = "DUPLICATE_PRICE";
}
