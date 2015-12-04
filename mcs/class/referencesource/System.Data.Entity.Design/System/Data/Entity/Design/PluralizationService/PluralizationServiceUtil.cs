//---------------------------------------------------------------------
// <copyright file="PluralizationServiceUtil.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Globalization;

namespace System.Data.Entity.Design.PluralizationServices
{
    internal static class PluralizationServiceUtil
    {
        internal static bool DoesWordContainSuffix(string word, IEnumerable<string> suffixes, CultureInfo culture)
        {
            return suffixes.Any(s => word.EndsWith(s, true, culture));
        }

        internal static bool TryInflectOnSuffixInWord(string word, IEnumerable<string> suffixes, Func<string, string> operationOnWord, CultureInfo culture, out string newWord)
        {
            newWord = null;

            if (PluralizationServiceUtil.DoesWordContainSuffix(word, suffixes, culture))
            {
                newWord = operationOnWord(word);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
