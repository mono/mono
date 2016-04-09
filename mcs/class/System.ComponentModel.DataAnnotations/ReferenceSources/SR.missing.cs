namespace System.ComponentModel.DataAnnotations.Resources
{
static partial class DataAnnotationsResources
{
	public const string AssociatedMetadataTypeTypeDescriptor_MetadataTypeContainsUnknownProperties = "The associated metadata type for type '{0}' contains the following unknown properties or fields: {1}. Please make sure that the names of these members match the names of the properties on the main type.";
	public const string AttributeStore_Unknown_Property = "The type '{0}' does not contain a public property named '{1}'.";
	public const string Common_PropertyNotFound = "The property {0}.{1} could not be found.";
	public const string CompareAttribute_UnknownProperty = "Could not find a property named {0}.";
	public const string CreditCardAttribute_Invalid = "The {0} field is not a valid credit card number.";
	public const string CustomValidationAttribute_Method_Must_Return_ValidationResult = "The CustomValidationAttribute method '{0}' in type '{1}' must return System.ComponentModel.DataAnnotations.ValidationResult.  Use System.ComponentModel.DataAnnotations.ValidationResult.Success to represent success.";
	public const string CustomValidationAttribute_Method_Not_Found = "The CustomValidationAttribute method '{0}' does not exist in type '{1}' or is not public and static.";
	public const string CustomValidationAttribute_Method_Required = "The CustomValidationAttribute.Method was not specified.";
	public const string CustomValidationAttribute_Method_Signature = "The CustomValidationAttribute method '{0}' in type '{1}' must match the expected signature: public static ValidationResult {0}(object value, ValidationContext context).  The value can be strongly typed.  The ValidationContext parameter is optional.";
	public const string CustomValidationAttribute_Type_Conversion_Failed = "Could not convert the value of type '{0}' to '{1}' as expected by method {2}.{3}.";
	public const string CustomValidationAttribute_Type_Must_Be_Public = "The custom validation type '{0}' must be public.";
	public const string CustomValidationAttribute_ValidationError = "{0} is not valid.";
	public const string CustomValidationAttribute_ValidatorType_Required = "The CustomValidationAttribute.ValidatorType was not specified.";
	public const string DataTypeAttribute_EmptyDataTypeString = "The custom DataType string cannot be null or empty.";
	public const string DisplayAttribute_PropertyNotSet = "The {0} property has not been set.  Use the {1} method to get the value.";
	public const string EmailAddressAttribute_Invalid = "The {0} field is not a valid e-mail address.";
	public const string EnumDataTypeAttribute_TypeCannotBeNull = "The type provided for EnumDataTypeAttribute cannot be null.";
	public const string EnumDataTypeAttribute_TypeNeedsToBeAnEnum = "The type '{0}' needs to represent an enumeration type.";
	public const string FileExtensionsAttribute_Invalid = "The {0} field only accepts files with the following extensions: {1}";
	public const string LocalizableString_LocalizationFailed = "Cannot retrieve property '{0}' because localization failed.  Type '{1}' is not public or does not contain a public static string property with the name '{2}'.";
	public const string MetadataTypeAttribute_TypeCannotBeNull = "MetadataClassType cannot be null.";
	public const string PhoneAttribute_Invalid = "The {0} field is not a valid phone number.";
	public const string RangeAttribute_ArbitraryTypeNotIComparable = "The type {0} must implement {1}.";
	public const string RangeAttribute_MinGreaterThanMax = "The maximum value '{0}' must be greater than or equal to the minimum value '{1}'.";
	public const string RangeAttribute_Must_Set_Min_And_Max = "The minimum and maximum values must be set.";
	public const string RangeAttribute_Must_Set_Operand_Type = "The OperandType must be set when strings are used for minimum and maximum values.";
	public const string RangeAttribute_ValidationError = "The field {0} must be between {1} and {2}.";
	public const string RegularExpressionAttribute_Empty_Pattern = "The pattern must be set to a valid regular expression.";
	public const string StringLengthAttribute_InvalidMaxLength = "The maximum length must be a nonnegative integer.";
	public const string StringLengthAttribute_ValidationError = "The field {0} must be a string with a maximum length of {1}.";
	public const string StringLengthAttribute_ValidationErrorIncludingMinimum = "The field {0} must be a string with a minimum length of {2} and a maximum length of {1}.";
	public const string UIHintImplementation_ControlParameterKeyIsNotAString = "The key parameter at position {0} with value '{1}' is not a string. Every key control parameter must be a string.";
	public const string UIHintImplementation_ControlParameterKeyIsNull = "The key parameter at position {0} is null. Every key control parameter must be a string.";
	public const string UIHintImplementation_ControlParameterKeyOccursMoreThanOnce = "The key parameter at position {0} with value '{1}' occurs more than once.";
	public const string UIHintImplementation_NeedEvenNumberOfControlParameters = "The number of control parameters must be even.";
	public const string UrlAttribute_Invalid = "The {0} field is not a valid fully-qualified http, https, or ftp URL.";
	public const string ValidationAttribute_Cannot_Set_ErrorMessage_And_Resource = "Either ErrorMessageString or ErrorMessageResourceName must be set, but not both.";
	public const string ValidationAttribute_IsValid_NotImplemented = "IsValid(object value) has not been implemented by this class.  The preferred entry point is GetValidationResult() and classes should override IsValid(object value, ValidationContext context).";
	public const string ValidationAttribute_NeedBothResourceTypeAndResourceName = "Both ErrorMessageResourceType and ErrorMessageResourceName need to be set on this attribute.";
	public const string ValidationAttribute_ResourcePropertyNotStringType = "The property '{0}' on resource type '{1}' is not a string type.";
	public const string ValidationAttribute_ResourceTypeDoesNotHaveProperty = "The resource type '{0}' does not have an accessible static property named '{1}'.";
	public const string ValidationAttribute_ValidationError = "The field {0} is invalid.";
	public const string ValidationContextServiceContainer_ItemAlreadyExists = "A service of type '{0}' already exists in the container.";
	public const string Validator_InstanceMustMatchValidationContextInstance = "The instance provided must match the ObjectInstance on the ValidationContext supplied.";
	public const string Validator_Property_Value_Wrong_Type = "The value for property '{0}' must be of type '{1}'.";
}
}
