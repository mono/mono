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

        internal static bool NeedManualEnlistment() => false;
        
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

        internal static ArgumentException InvalidDataType(TypeCode typecode)
        {
            return Argument(SR.GetString(SR.ADP_InvalidDataType, typecode.ToString()));
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

        internal static ArgumentException UnknownDataTypeCode(Type dataType, TypeCode typeCode)
        {
            return Argument(SR.GetString(SR.ADP_UnknownDataTypeCode, ((int)typeCode).ToString(CultureInfo.InvariantCulture), dataType.FullName));
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

        static internal ArgumentOutOfRangeException NotSupportedUserDefinedTypeSerializationFormat(Microsoft.SqlServer.Server.Format value, string method) {
            return ADP.NotSupportedEnumerationValue(typeof(Microsoft.SqlServer.Server.Format), value.ToString(), method);
        }

        static internal ArgumentOutOfRangeException InvalidUserDefinedTypeSerializationFormat(Microsoft.SqlServer.Server.Format value) {
#if DEBUG
            switch(value) {
            case Microsoft.SqlServer.Server.Format.Unknown:
            case Microsoft.SqlServer.Server.Format.Native:
            case Microsoft.SqlServer.Server.Format.UserDefined:
                Debug.Assert(false, "valid UserDefinedTypeSerializationFormat " + value.ToString());
                break;
            }
#endif
            return InvalidEnumerationValue(typeof(Microsoft.SqlServer.Server.Format), (int) value);
        }

        static internal ArgumentOutOfRangeException ArgumentOutOfRange(string message, string parameterName, object value) {
            ArgumentOutOfRangeException e = new ArgumentOutOfRangeException(parameterName, value, message);
            TraceExceptionAsReturnValue(e);
            return e;
        }
    }
}