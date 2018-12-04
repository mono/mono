// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Used to store mapping information for names occuring 
// within the logical tree section.

using System;
using System.Globalization;


namespace System.Xaml
{
    /// <summary>
    ///     The string used in RuntimeNameProperty is expected to follow certain
    /// rules.  IsValidIdentifierName checks the given string against the rules.
    /// NameValidationCallback extends to all object types and is in the right
    /// format to be used as a DependencyProperty ValidateValueCallback
    /// </summary>
    internal static class NameValidationHelper
    {
        /// <summary>
        /// Validates the name to follow Naming guidelines
        /// </summary>
        /// <param name="name">string to validate</param>
        internal static bool IsValidIdentifierName(string name)
        {
            // Grammar:
            // <identifier> ::= <identifier_start> ( <identifier_start> | <identifier_extend> )*
            // <identifier_start> ::= [{Lu}{Ll}{Lt}{Lo}{Nl}('_')]
            // <identifier_extend> ::= [{Mn}{Mc}{Lm}{Nd}]
            UnicodeCategory uc;
            for (int i = 0; i < name.Length; i++)
            {
                uc = Char.GetUnicodeCategory(name[i]);
                bool idStart = (uc == UnicodeCategory.UppercaseLetter || // (Lu)
                             uc == UnicodeCategory.LowercaseLetter || // (Ll)
                             uc == UnicodeCategory.TitlecaseLetter || // (Lt)
                             uc == UnicodeCategory.OtherLetter || // (Lo)
                             uc == UnicodeCategory.LetterNumber || // (Nl)
                             name[i] == '_');
                bool idExtend = (uc == UnicodeCategory.NonSpacingMark || // (Mn)
                              uc == UnicodeCategory.SpacingCombiningMark || // (Mc)
                              uc == UnicodeCategory.ModifierLetter || // (Lm)
                              uc == UnicodeCategory.DecimalDigitNumber); // (Nd)
                if (i == 0)
                {
                    if (!idStart)
                    {
                        return false;
                    }
                }
                else if (!(idStart || idExtend))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
