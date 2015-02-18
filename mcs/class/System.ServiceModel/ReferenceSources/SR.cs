//
// Resource strings referenced by the code.
//
// Copyright 2014 Xamarin Inc
//
// Use the following script to extract strings from .NET strings.resx:
//
// var d = XDocument.Load ("Strings.resx");
// foreach (var j in d.XPathSelectElements ("/root/data")){ var v = j.XPathSelectElement ("value"); Console.WriteLine ("\tpublic const string {0}=\"{1}\";", j.Attribute ("name").Value, v.Value); }
//
using System;
partial class SR
{
	public const string BindUriTemplateToNullOrEmptyPathParam = "BindUriTemplateToNullOrEmptyPathParam";
	public const string UTAdditionalDefaultIsInvalid = "Additional Defaults IsInvalid key: {0} template: {1}";
	public const string UTBadBaseAddress = "BadBaseAddress";
	public const string UTBindByNameCalledWithEmptyKey = "BindByNameCalledWithEmptyKey";
	public const string UTBindByPositionNoVariables = "BindByPositionNoVariables";
	public const string UTBindByPositionWrongCount = "BindByPositionWrongCount";
	public const string UTBothLiteralAndNameValueCollectionKey = "BothLiteralAndNameValueCollectionKey";
	public const string UTDefaultValueToCompoundSegmentVarFromAdditionalDefaults = "DefaultValueToCompoundSegmentVarFromAdditionalDefaults";
	public const string UTDefaultValueToQueryVar = "DefaultValueToQueryVar";
	public const string UTDefaultValueToQueryVarFromAdditionalDefaults = "DefaultValueToQueryVarFromAdditionalDefaults {0} does not contain {1}";
	public const string UTDefaultValuesAreImmutable = "DefaultValuesAreImmutable";
	public const string UTInvalidDefaultPathValue = "InvalidDefaultPathValue";
	public const string UTInvalidVarDeclaration = "InvalidVarDeclaration";
	public const string UTInvalidWildcardInVariableOrLiteral = "InvalidWildcardInVariableOrLiteral";
	public const string UTNullableDefaultAtAdditionalDefaults = "NullableDefaultAtAdditionalDefaults {0} does not contain {1}";
	public const string UTNullableDefaultMustBeFollowedWithNullables = "NullableDefaultMustBeFollowedWithNullables";
	public const string UTNullableDefaultMustNotBeFollowedWithLiteral = "NullableDefaultMustNotBeFollowedWithLiteral";
	public const string UTNullableDefaultMustNotBeFollowedWithWildcard = "NullableDefaultMustNotBeFollowedWithWildcard";
	public const string UTQueryCannotEndInAmpersand = "QueryCannotEndInAmpersand";
	public const string UTQueryCannotHaveEmptyName = "QueryCannotHaveEmptyName";
	public const string UTQueryMustHaveLiteralNames = "QueryMustHaveLiteralNames";
	public const string UTQueryNamesMustBeUnique = "QueryNamesMustBeUnique";
	public const string UTStarVariableWithDefaults = "StarVariableWithDefaults";
	public const string UTStarVariableWithDefaultsFromAdditionalDefaults = "StarVariableWithDefaultsFromAdditionalDefaults";
	public const string UTVarNamesMustBeUnique = "VarNamesMustBeUnique";
	public const string UTTDuplicate = "TDuplicate";
	public const string UTInvalidFormatSegmentOrQueryPart = "InvalidFormatSegmentOrQueryPart";
	public const string UTTOtherAmbiguousQueries = "TOtherAmbiguousQueries";
	public const string UTDefaultValueToCompoundSegmentVar = "DefaultValueToCompoundSegmentVar";
	public const string UTDoesNotSupportAdjacentVarsInCompoundSegment = "DoesNotSupportAdjacentVarsInCompoundSegment";
	public const string UTCSRLookupBeforeMatch = "CSRLookupBeforeMatch";
	public const string UTQueryCannotHaveCompoundValue = "QueryCannotHaveCompoundValue";
	public const string UTTAmbiguousQueries = "UTTAmbiguousQueries";
		

	public static string GetString (string key, params object [] args)
	{
		if (args.Length == 0)
			return key;
		return String.Format (key, args);
	}
}
