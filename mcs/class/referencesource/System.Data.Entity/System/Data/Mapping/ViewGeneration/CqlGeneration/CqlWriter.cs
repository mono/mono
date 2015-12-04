//---------------------------------------------------------------------
// <copyright file="CqlWriter.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Text.RegularExpressions;
using System.Text;
using System.Data.Common.Utils;
using System.Data.Mapping.ViewGeneration.Utils;
using System.Data.Metadata.Edm;

namespace System.Data.Mapping.ViewGeneration.CqlGeneration
{

    // This class contains helper methods needed for generating Cql
    internal static class CqlWriter
    {

        #region Fields
        private static readonly Regex s_wordIdentifierRegex = new Regex(@"^[_A-Za-z]\w*$", RegexOptions.ECMAScript | RegexOptions.Compiled);
        #endregion

        #region Helper Methods
        // effects: Given a block name and a field in it -- returns a string
        // of form "blockName.field". Does not perform any escaping
        internal static string GetQualifiedName(string blockName, string field)
        {
            string result = StringUtil.FormatInvariant("{0}.{1}", blockName, field);
            return result;
        }

        // effects: Modifies builder to contain an escaped version of type's name as "[namespace.typename]"
        internal static void AppendEscapedTypeName(StringBuilder builder, EdmType type)
        {
            AppendEscapedName(builder, GetQualifiedName(type.NamespaceName, type.Name));
        }

        // effects: Modifies builder to contain an escaped version of "name1.name2" as "[name1].[name2]"
        internal static void AppendEscapedQualifiedName(StringBuilder builder, string name1, string name2)
        {
            AppendEscapedName(builder, name1);
            builder.Append('.');
            AppendEscapedName(builder, name2);
        }

        // effects: Modifies builder to contain an escaped version of "name"
        internal static void AppendEscapedName(StringBuilder builder, string name)
        {
            if (s_wordIdentifierRegex.IsMatch(name) && false == ExternalCalls.IsReservedKeyword(name))
            {
                // We do not need to escape the name if it is a simple name and it is not a keyword
                builder.Append(name);
            }
            else
            {
                string newName = name.Replace("]", "]]");
                builder.Append('[')
                       .Append(newName)
                       .Append(']');
            }
        }
        #endregion
    }
}
