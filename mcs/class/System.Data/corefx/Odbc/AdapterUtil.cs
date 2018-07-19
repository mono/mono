// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Versioning;
using System.Text;
using System.Security.Permissions;
using System.ComponentModel;
using System.Configuration;
using System.Xml;

namespace System.Data.Common
{
	internal static partial class ADP
	{
		internal const int DecimalMaxPrecision = 29;	   
		internal const int DecimalMaxPrecision28 = 28;  // there are some cases in Odbc where we need that ...
		
		internal static readonly IntPtr PtrZero = new IntPtr(0); // IntPtr.Zero
		internal static readonly int PtrSize = IntPtr.Size;

		internal const string BeginTransaction = "BeginTransaction";
		internal const string ChangeDatabase = "ChangeDatabase";
		internal const string CommitTransaction = "CommitTransaction";
		internal const string CommandTimeout = "CommandTimeout";
		internal const string DeriveParameters = "DeriveParameters";
		internal const string ExecuteReader = "ExecuteReader";
		internal const string ExecuteNonQuery = "ExecuteNonQuery";
		internal const string ExecuteScalar = "ExecuteScalar";
		internal const string GetSchema = "GetSchema";
		internal const string GetSchemaTable = "GetSchemaTable";
		internal const string Prepare = "Prepare";
		internal const string RollbackTransaction = "RollbackTransaction";
		internal const string QuoteIdentifier = "QuoteIdentifier";
		internal const string UnquoteIdentifier = "UnquoteIdentifier";

		internal static bool NeedManualEnlistment() => false;
		internal static bool IsEmpty(string str) => string.IsNullOrEmpty(str);
		
		internal static Exception DatabaseNameTooLong()
		{
			return Argument(SR.GetString(SR.ADP_DatabaseNameTooLong));
		}

		internal static int StringLength(string inputString)
		{
			return ((null != inputString) ? inputString.Length : 0);
		}

		internal static Exception NumericToDecimalOverflow()
		{
			return InvalidCast(SR.GetString(SR.ADP_NumericToDecimalOverflow));
		}

		internal static Exception OdbcNoTypesFromProvider()
		{
			return InvalidOperation(SR.GetString(SR.ADP_OdbcNoTypesFromProvider));
		}

		internal static ArgumentException InvalidRestrictionValue(string collectionName, string restrictionName, string restrictionValue)
		{
			return ADP.Argument(SR.GetString(SR.MDF_InvalidRestrictionValue, collectionName, restrictionName, restrictionValue));
		}

		internal static Exception DataReaderNoData()
		{
			return InvalidOperation(SR.GetString(SR.ADP_DataReaderNoData));
		}

		internal static Exception ConnectionIsDisabled(Exception InnerException)
		{
			return InvalidOperation(SR.GetString(SR.ADP_ConnectionIsDisabled), InnerException);
		}
		
		internal static Exception OffsetOutOfRangeException()
		{
			return InvalidOperation(SR.GetString(SR.ADP_OffsetOutOfRangeException));
		}

		static internal InvalidOperationException QuotePrefixNotSet(string method) 
		{
			return InvalidOperation(Res.GetString(Res.ADP_QuotePrefixNotSet, method));
		}
		
		[ResourceExposure(ResourceScope.Machine)]
		[ResourceConsumption(ResourceScope.Machine)]
		internal static string GetFullPath(string filename)
		{ // MDAC 77686
			return Path.GetFullPath(filename);
		}

		internal static InvalidOperationException InvalidDataDirectory()
		{
			return ADP.InvalidOperation(SR.GetString(SR.ADP_InvalidDataDirectory));
		}

		internal static void EscapeSpecialCharacters(string unescapedString, StringBuilder escapedString)
		{
			// note special characters list is from character escapes
			// in the MSDN regular expression language elements documentation
			// added ] since escaping it seems necessary
			const string specialCharacters = ".$^{[(|)*+?\\]";

			foreach (char currentChar in unescapedString)
			{
				if (specialCharacters.IndexOf(currentChar) >= 0)
				{
					escapedString.Append("\\");
				}
				escapedString.Append(currentChar);
			}
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		internal static IntPtr IntPtrOffset(IntPtr pbase, Int32 offset)
		{
			if (4 == ADP.PtrSize)
			{
				return (IntPtr)checked(pbase.ToInt32() + offset);
			}
			Debug.Assert(8 == ADP.PtrSize, "8 != IntPtr.Size"); // MDAC 73747
			return (IntPtr)checked(pbase.ToInt64() + offset);
		}

		static internal Exception InvalidXMLBadVersion() {
			return Argument(Res.GetString(Res.ADP_InvalidXMLBadVersion));
		}
		
		static internal Exception NotAPermissionElement() {
			return Argument(Res.GetString(Res.ADP_NotAPermissionElement));
		}

		static internal Exception PermissionTypeMismatch() {
			return Argument(Res.GetString(Res.ADP_PermissionTypeMismatch));
		}

		static internal ArgumentOutOfRangeException InvalidPermissionState(PermissionState value) {
#if DEBUG
			switch(value) {
			case PermissionState.Unrestricted:
			case PermissionState.None:
				Debug.Assert(false, "valid PermissionState " + value.ToString());
				break;
			}
#endif
			return InvalidEnumerationValue(typeof(PermissionState), (int) value);
		}
		
#if !MOBILE
		static internal ConfigurationException Configuration(string message) {
			ConfigurationException e = new ConfigurationErrorsException(message);
			TraceExceptionAsReturnValue(e);
			return e;
		}
		static internal ConfigurationException Configuration(string message, XmlNode node) {
			ConfigurationException e = new ConfigurationErrorsException(message, node);
			TraceExceptionAsReturnValue(e);
			return e;
		}
#endif

		static internal ArgumentException ConfigProviderNotFound() {
			return Argument(Res.GetString(Res.ConfigProviderNotFound));
		}
		static internal InvalidOperationException ConfigProviderInvalid() {
			return InvalidOperation(Res.GetString(Res.ConfigProviderInvalid));
		}

#if !MOBILE
		static internal ConfigurationException ConfigProviderNotInstalled() {
			return Configuration(Res.GetString(Res.ConfigProviderNotInstalled));
		}
		static internal ConfigurationException ConfigProviderMissing() {
			return Configuration(Res.GetString(Res.ConfigProviderMissing));
		}

		//
		// DbProviderConfigurationHandler
		//
		static internal ConfigurationException ConfigBaseNoChildNodes(XmlNode node) { // Res.Config_base_no_child_nodes
			return Configuration(Res.GetString(Res.ConfigBaseNoChildNodes), node);
		}
		static internal ConfigurationException ConfigBaseElementsOnly(XmlNode node) { // Res.Config_base_elements_only
			return Configuration(Res.GetString(Res.ConfigBaseElementsOnly), node);
		}
		static internal ConfigurationException ConfigUnrecognizedAttributes(XmlNode node) { // Res.Config_base_unrecognized_attribute
			return Configuration(Res.GetString(Res.ConfigUnrecognizedAttributes, node.Attributes[0].Name), node);
		}
		static internal ConfigurationException ConfigUnrecognizedElement(XmlNode node) { // Res.Config_base_unrecognized_element
			return Configuration(Res.GetString(Res.ConfigUnrecognizedElement), node);
		}
		static internal ConfigurationException ConfigSectionsUnique(string sectionName) { // Res.Res.ConfigSectionsUnique
			return Configuration(Res.GetString(Res.ConfigSectionsUnique, sectionName));
		}
		static internal ConfigurationException ConfigRequiredAttributeMissing(string name, XmlNode node) { // Res.Config_base_required_attribute_missing
			return Configuration(Res.GetString(Res.ConfigRequiredAttributeMissing, name), node);
		}
		static internal ConfigurationException ConfigRequiredAttributeEmpty(string name, XmlNode node) { // Res.Config_base_required_attribute_empty
			return Configuration(Res.GetString(Res.ConfigRequiredAttributeEmpty, name), node);
		}
#endif
		static internal Exception OleDb() => new NotImplementedException("OleDb is not implemented.");
	}
}