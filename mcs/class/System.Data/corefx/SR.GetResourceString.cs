using System;

internal static partial class SR 
{
	public static string GetResourceString(string resourceKey, string defaultString) => defaultString;
}

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
}