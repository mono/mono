// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Resources;
using System.Reflection;

// needed for ../referencesource/System.Data/System/Data/CodeGen/datacache.cs
internal static partial class Res
{
	internal static string GetString(string name) => name;
	internal static string GetString(string name, params object[] args) => string.Format(name, args);

	internal const string CodeGen_InvalidIdentifier = "Cannot generate identifier for name '{0}'";
	internal const string CodeGen_DuplicateTableName = "There is more than one table with the same name '{0}' (even if namespace is different)";
	internal const string CodeGen_TypeCantBeNull = "Column '{0}': Type '{1}' cannot be null";
	internal const string CodeGen_NoCtor0 = "Column '{0}': Type '{1}' does not have parameterless constructor";
	internal const string CodeGen_NoCtor1 = "Column '{0}': Type '{1}' does not have constructor with string argument";

	internal const string SQLUDT_MaxByteSizeValue = "range: 0-8000";
	internal const string SqlUdt_InvalidUdtMessage = "'{0}' is an invalid user defined type, reason: {1}.";
	internal const string Sql_NullCommandText = "Command parameter must have a non null and non empty command text.";
	internal const string Sql_MismatchedMetaDataDirectionArrayLengths = "MetaData parameter array must have length equivalent to ParameterDirection array argument.";
	
	public const string ADP_InvalidXMLBadVersion = "Invalid Xml; can only parse elements of version one.";
	public const string ADP_NotAPermissionElement = "Given security element is not a permission element.";
	public const string ADP_PermissionTypeMismatch = "Type mismatch.";

	public const string ConfigProviderNotFound = "Unable to find the requested .Net Framework Data Provider.  It may not be installed.";
	public const string ConfigProviderInvalid = "The requested .Net Framework Data Provider's implementation does not have an Instance field of a System.Data.Common.DbProviderFactory derived type.";
	public const string ConfigProviderNotInstalled = "Failed to find or load the registered .Net Framework Data Provider.";
	public const string ConfigProviderMissing = "The missing .Net Framework Data Provider's assembly qualified name is required.";
	public const string ConfigBaseElementsOnly = "Only elements allowed.";
	public const string ConfigBaseNoChildNodes = "Child nodes not allowed.";
	public const string ConfigUnrecognizedAttributes = "Unrecognized attribute '{0}'.";
	public const string ConfigUnrecognizedElement = "Unrecognized element.";
	public const string ConfigSectionsUnique = "The '{0}' section can only appear once per config file.";
	public const string ConfigRequiredAttributeMissing = "Required attribute '{0}' not found.";
	public const string ConfigRequiredAttributeEmpty = "Required attribute '{0}' cannot be empty.";
	public const string ADP_QuotePrefixNotSet = "{0} requires open connection when the quote prefix has not been set.";
}

internal static partial class SR 
{
	public static string GetResourceString(string resourceKey, string defaultString) {
		Type fieldType = typeof(SR);
		FieldInfo field = fieldType.GetField(resourceKey);
		if (field != null) {
			return field.GetValue(null).ToString();
		}

		return defaultString;
	}
}