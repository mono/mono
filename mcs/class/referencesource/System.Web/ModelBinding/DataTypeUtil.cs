namespace System.Web.ModelBinding {
    using System;
    using System.ComponentModel.DataAnnotations;

    internal static class DataTypeUtil {
        internal static readonly string CurrencyTypeName = DataType.Currency.ToString();
        internal static readonly string DateTypeName = DataType.Date.ToString();
        internal static readonly string DateTimeTypeName = DataType.DateTime.ToString();
        internal static readonly string DurationTypeName = DataType.Duration.ToString();
        internal static readonly string EmailAddressTypeName = DataType.EmailAddress.ToString();
        internal static readonly string HtmlTypeName = DataType.Html.ToString();
        internal static readonly string ImageUrlTypeName = DataType.ImageUrl.ToString();
        internal static readonly string MultiLineTextTypeName = DataType.MultilineText.ToString();
        internal static readonly string PasswordTypeName = DataType.Password.ToString();
        internal static readonly string PhoneNumberTypeName = DataType.PhoneNumber.ToString();
        internal static readonly string TextTypeName = DataType.Text.ToString();
        internal static readonly string TimeTypeName = DataType.Time.ToString();
        internal static readonly string UrlTypeName = DataType.Url.ToString();
        internal static readonly string CreditCardTypeName = DataType.CreditCard.ToString();
        internal static readonly string PostalCodeTypeName = DataType.PostalCode.ToString();
        internal static readonly string UploadTypeName = DataType.Upload.ToString();

        // This is a faster version of GetDataTypeName(). It internally calls ToString() on the enum
        // value, which can be quite slow because of value verification.
        internal static string ToDataTypeName(this DataTypeAttribute attribute, Func<DataTypeAttribute, Boolean> isDataType = null) {
            if (isDataType == null) {
                isDataType = t => t.GetType().Equals(typeof(DataTypeAttribute));
            }

            // GetDataTypeName is virtual, so this is only safe if they haven't derived from DataTypeAttribute.
            // However, if they derive from DataTypeAttribute, they can help their own perf by overriding GetDataTypeName
            // and returning an appropriate string without invoking the ToString() on the enum.
            if (isDataType(attribute)) {
                switch (attribute.DataType) {
                    case DataType.Currency:
                        return CurrencyTypeName;
                    case DataType.Date:
                        return DateTypeName;
                    case DataType.DateTime:
                        return DateTimeTypeName;
                    case DataType.Duration:
                        return DurationTypeName;
                    case DataType.EmailAddress:
                        return EmailAddressTypeName;
                    case DataType.Html:
                        return HtmlTypeName;
                    case DataType.ImageUrl:
                        return ImageUrlTypeName;
                    case DataType.MultilineText:
                        return MultiLineTextTypeName;
                    case DataType.Password:
                        return PasswordTypeName;
                    case DataType.PhoneNumber:
                        return PhoneNumberTypeName;
                    case DataType.Text:
                        return TextTypeName;
                    case DataType.Time:
                        return TimeTypeName;
                    case DataType.Url:
                        return UrlTypeName;
                    case DataType.CreditCard:
                        return CreditCardTypeName;
                    case DataType.PostalCode:
                        return PostalCodeTypeName;
                    case DataType.Upload:
                        return UploadTypeName;
                }
            }

            return attribute.GetDataTypeName();
        }
    }
}
