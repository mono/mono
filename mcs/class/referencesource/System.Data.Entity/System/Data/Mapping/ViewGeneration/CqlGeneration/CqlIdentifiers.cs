//---------------------------------------------------------------------
// <copyright file="CqlIdentifiers.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------


using System.Data.Common.Utils;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
namespace System.Data.Mapping.ViewGeneration.Structures
{

    // This class is responsible for ensuring unique aliases for _from0, etc
    // and block aliases T, T0, T1, etc
    internal class CqlIdentifiers : InternalBase
    {

        #region Constructor
        internal CqlIdentifiers()
        {
            m_identifiers = new Set<string>(StringComparer.Ordinal);
        }
        #endregion

        #region Fields
        private Set<string> m_identifiers;
        #endregion

        #region Methods
        // effects: Given a number, returns _from<num> if it does not clashes with
        // any identifier, else returns _from_<next>_<num> where <next> is the first number from 0
        // where there is no clash
        internal string GetFromVariable(int num)
        {
            return GetNonConflictingName("_from", num);
        }

        // effects: Given a number, returns T<num> if it does not clashes with
        // any identifier, else returns T_<next>_<num> where <next> is the first number from 0
        // where there is no clash
        internal string GetBlockAlias(int num)
        {
            return GetNonConflictingName("T", num);
        }

        // effects: Given a number, returns T if it does not clashes with
        // any identifier, else returns T_<next> where <next> is the first number from 0
        // where there is no clash
        internal string GetBlockAlias()
        {
            return GetNonConflictingName("T", -1);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        internal void AddIdentifier(string identifier)
        {
            m_identifiers.Add(identifier.ToLower(CultureInfo.InvariantCulture));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        private string GetNonConflictingName(string prefix, int number)
        {
            // Do a case sensitive search but return the string that uses the
            // original prefix
            string result = number < 0 ? prefix : StringUtil.FormatInvariant("{0}{1}", prefix, number);
            // Check if the prefix exists or not
            if (m_identifiers.Contains(result.ToLower(CultureInfo.InvariantCulture)) == false)
            {
                return result;
            }

            // Go through integers and find the first one that does not clash
            for (int count = 0; count < int.MaxValue; count++)
            {
                if (number < 0)
                {
                    result = StringUtil.FormatInvariant("{0}_{1}", prefix, count);
                }
                else
                {
                    result = StringUtil.FormatInvariant("{0}_{1}_{2}", prefix, count, number);
                }
                if (m_identifiers.Contains(result.ToLower(CultureInfo.InvariantCulture)) == false)
                {
                    return result;
                }
            }
            Debug.Fail("Found no unique _from till MaxValue?");
            return null;
        }

        internal override void ToCompactString(StringBuilder builder)
        {
            m_identifiers.ToCompactString(builder);
        }

        #endregion
    }
}
