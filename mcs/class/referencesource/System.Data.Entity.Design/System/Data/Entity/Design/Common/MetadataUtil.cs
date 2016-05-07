//---------------------------------------------------------------------
// <copyright file="MetadataUtil.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  	 [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Data.Metadata.Edm;
using System.Xml;
using System.Data.Common;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Globalization;
using System.Linq;

namespace System.Data.Entity.Design.Common
{
    internal static class MetadataUtil
    {
        private const string s_defaultDelimiter = ", ";

        internal static bool IsStoreType(GlobalItem item)
        {
            return item.DataSpace == DataSpace.SSpace;
        }

        internal static DbProviderServices GetProviderServices(DbProviderFactory factory)
        {
            EDesignUtil.CheckArgumentNull(factory, "factory");

            // Special case SQL client so that it will work with System.Data from .NET 4.0 even without
            // a binding redirect.
            if (factory is SqlClientFactory)
            {
                return SqlProviderServices.Instance;
            }

            IServiceProvider serviceProvider = factory as IServiceProvider;
            if (serviceProvider == null)
            {
                throw MetadataUtil.ProviderIncompatible(System.Data.Entity.Design.Strings.EntityClient_DoesNotImplementIServiceProvider(
                    factory.GetType().ToString()));
            }

            DbProviderServices providerServices = serviceProvider.GetService(typeof(DbProviderServices)) as DbProviderServices;
            if (providerServices == null)
            {
                throw MetadataUtil.ProviderIncompatible(
                    System.Data.Entity.Design.Strings.EntityClient_ReturnedNullOnProviderMethod(
                        "GetService",
                        factory.GetType().ToString()));
            }
            return providerServices;
        }

        static internal ProviderIncompatibleException ProviderIncompatible(string error)
        {
            ProviderIncompatibleException e = new ProviderIncompatibleException(error);
            return e;
        }

        /// <summary>
        /// Check if all the SchemaErrors have the serverity of SchemaErrorSeverity.Warning
        /// </summary>
        /// <param name="schemaErrors"></param>
        /// <returns></returns>
        internal static bool CheckIfAllErrorsAreWarnings(IList<EdmSchemaError> schemaErrors)
        {
            int length = schemaErrors.Count;
            for (int i = 0; i < length; ++i)
            {
                EdmSchemaError error = schemaErrors[i];
                if (error.Severity != EdmSchemaErrorSeverity.Warning)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        ///   This private static method checks a string to make sure that it is not empty.
        ///   Comparing with String.Empty is not sufficient since a string with nothing
        ///   but white space isn't considered "empty" by that rationale.
        /// </summary>
        internal static bool IsNullOrEmptyOrWhiteSpace(string value)
        {
            return IsNullOrEmptyOrWhiteSpace(value, 0);
        }

        internal static bool IsNullOrEmptyOrWhiteSpace(string value, int offset)
        {
            // don't use Trim(), which will copy the string, which may be large, just to test for emptyness
            //return String.IsNullOrEmpty(value) || String.IsNullOrEmpty(value.Trim());
            if (null != value)
            {
                for (int i = offset; i < value.Length; ++i)
                {
                    if (!Char.IsWhiteSpace(value[i]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        // separate implementation from IsNullOrEmptyOrWhiteSpace(string, int) because that one will
        // pick up the jit optimization to avoid boundary checks and the this won't is unknown (most likely not)
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")] // referenced by System.Data.Entity.Design.dll
        internal static bool IsNullOrEmptyOrWhiteSpace(string value, int offset, int length)
        {
            // don't use Trim(), which will copy the string, which may be large, just to test for emptyness
            //return String.IsNullOrEmpty(value) || String.IsNullOrEmpty(value.Trim());
            if (null != value)
            {
                length = Math.Min(value.Length, length);
                for (int i = offset; i < length; ++i)
                {
                    if (!Char.IsWhiteSpace(value[i]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal static string MembersToCommaSeparatedString(IEnumerable members)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("{");
            MetadataUtil.ToCommaSeparatedString(builder, members);
            builder.Append("}");
            return builder.ToString();
        }

        internal static void ToCommaSeparatedString(StringBuilder builder, IEnumerable list)
        {
            ToSeparatedStringPrivate(builder, list, s_defaultDelimiter, string.Empty, false);
        }

        // effects: Converts the list to a list of strings, sorts its (if
        // toSort is true) and then converts to a string separated by
        // "separator" with "nullValue" used for null values.
        private static void ToSeparatedStringPrivate(StringBuilder stringBuilder, IEnumerable list, string separator,
                                                     string nullValue, bool toSort)
        {
            if (null == list)
            {
                return;
            }
            bool isFirst = true;
            // Get the list of strings first
            List<string> elementStrings = new List<string>();
            foreach (object element in list)
            {
                string str;
                // Get the element or its default null value
                if (element == null)
                {
                    str = nullValue;
                }
                else
                {
                    str = FormatInvariant("{0}", element);
                }
                elementStrings.Add(str);
            }

            if (toSort == true)
            {
                // Sort the list
                elementStrings.Sort(StringComparer.Ordinal);
            }

            // Now add the strings to the stringBuilder
            foreach (string str in elementStrings)
            {
                if (false == isFirst)
                {
                    stringBuilder.Append(separator);
                }
                stringBuilder.Append(str);
                isFirst = false;
            }
        }

        internal static string FormatInvariant(string format, params object[] args)
        {
            Debug.Assert(args.Length > 0, "Formatting utilities must be called with at least one argument");
            return String.Format(CultureInfo.InvariantCulture, format, args);
        }

        /// <summary>
        /// replace troublesome xml characters with equivalent entities
        /// </summary>
        /// <param name="text">text that make have characters troublesome in xml</param>
        /// <returns>text with troublesome characters replaced with equivalent entities</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")] // referenced by System.Data.Entity.Design.dll
        internal static string Entityize(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            text = text.Replace("&", "&amp;");
            text = text.Replace("<", "&lt;").Replace(">", "&gt;");
            return text.Replace("\'", "&apos;").Replace("\"", "&quot;");
        }

        internal static bool TrySplitExtendedMetadataPropertyName(string name, out string xmlNamespaceUri, out string attributeName)
        {
            int pos = name.LastIndexOf(':');
            if (pos < 0 || name.Length <= pos + 1)
            {
                Debug.Fail("the name is not in the form we expect");
                xmlNamespaceUri = null;
                attributeName = null;
                return false;
            }

            xmlNamespaceUri = name.Substring(0, pos);
            attributeName = name.Substring(pos + 1, (name.Length - 1) - pos);
            return true;
        }

        static private readonly Type StackOverflowType = typeof(System.StackOverflowException);
        static private readonly Type OutOfMemoryType = typeof(System.OutOfMemoryException);
        static private readonly Type ThreadAbortType = typeof(System.Threading.ThreadAbortException);
        static private readonly Type NullReferenceType = typeof(System.NullReferenceException);
        static private readonly Type AccessViolationType = typeof(System.AccessViolationException);
        static private readonly Type SecurityType = typeof(System.Security.SecurityException);

        internal static bool IsCatchableExceptionType(Exception e)
        {
            // a 'catchable' exception is defined by what it is not.
            Debug.Assert(e != null, "Unexpected null exception!");
            Type type = e.GetType();

            return ((type != StackOverflowType) &&
                     (type != OutOfMemoryType) &&
                     (type != ThreadAbortType) &&
                     (type != NullReferenceType) &&
                     (type != AccessViolationType) &&
                     !SecurityType.IsAssignableFrom(type));
        }

        /// <summary>
        /// Returns the single error message from the list of errors
        /// </summary>
        /// <param name="errors"></param>
        /// <returns></returns>
        static internal string CombineErrorMessage(IEnumerable<System.Data.Metadata.Edm.EdmSchemaError> errors)
        {
            Debug.Assert(errors != null);
            StringBuilder sb = new StringBuilder(System.Environment.NewLine);
            int count = 0;
            foreach (System.Data.Metadata.Edm.EdmSchemaError error in errors)
            {
                //Don't append a new line at the beginning of the messages
                if ((count++) != 0)
                {
                    sb.Append(System.Environment.NewLine);
                }
                sb.Append(error.ToString());

            }
            Debug.Assert(count != 0, "Empty Error List");
            return sb.ToString();
        }

        internal static void DisposeXmlReaders(IEnumerable<XmlReader> xmlReaders)
        {
            Debug.Assert(xmlReaders != null);

            foreach (XmlReader xmlReader in xmlReaders)
            {
                ((IDisposable)xmlReader).Dispose();
            }
        }

        internal static bool IsCollectionType(GlobalItem item)
        {
            return (BuiltInTypeKind.CollectionType == item.BuiltInTypeKind);
        }

        internal static bool IsComplexType(EdmType type)
        {
            return (BuiltInTypeKind.ComplexType == type.BuiltInTypeKind);
        }

        internal static bool IsPrimitiveType(EdmType type)
        {
            return (BuiltInTypeKind.PrimitiveType == type.BuiltInTypeKind);
        }

        internal static bool IsEntitySet(EntitySetBase entitySetBase)
        {
            return BuiltInTypeKind.EntitySet == entitySetBase.BuiltInTypeKind;
        }

        internal static bool IsValidKeyType(Version entityFrameworkVersion, EdmType type)
        {
            var primitiveType = type as PrimitiveType;
            if (primitiveType == null)
            {
                return false;
            }
            if (EntityFrameworkVersions.Version1 == entityFrameworkVersion)
            {
                return primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.Binary;
            }
            else
            {
                // From V2 onwards, Binary key properties are supported
                return true;
            }
        }

        /// <summary>
        /// determines if type is of EnumerationType.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool IsEnumerationType(EdmType type)
        {
            return (BuiltInTypeKind.EnumType == type.BuiltInTypeKind);
        }
    }
}
