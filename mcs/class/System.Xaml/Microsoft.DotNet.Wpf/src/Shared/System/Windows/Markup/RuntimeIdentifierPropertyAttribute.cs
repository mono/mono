// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Description:
//   This attribute is placed on a class to identify the property that will
//   function as an Name for the given class

using System;
using System.Globalization;
using SRCS = System.Runtime.CompilerServices;

#if PBTCOMPILER
namespace MS.Internal.Markup
#elif WINDOWS_BASE
using MS.Internal.WindowsBase;     // FriendAccessAllowed
namespace System.Windows.Markup
#else
namespace System.Windows.Markup
#endif
{
#if !PBTCOMPILER && !TARGETTING35SP1 && !WINDOWS_BASE
    /// <summary>
    /// This attribute is placed on a class to identify the property that will
    /// function as an Name for the given class
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    [SRCS.TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public sealed class RuntimeNamePropertyAttribute: Attribute
    {
        /// <summary/>
        public RuntimeNamePropertyAttribute(string name)
        {
            _name = name;
        }

        /// <summary>
        /// The Name of the property in the class that will contain the ID of
        /// the class, this property needs to be of type string and have
        /// both get and set access
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        private string _name = null;
    }
#endif

#if !SYSTEM_XAML
    /// <summary>
    ///     The string used in RuntimeNameProperty is expected to follow certain
    /// rules.  IsValidIdentifierName checks the given string against the rules.
    /// NameValidationCallback extends to all object types and is in the right
    /// format to be used as a DependencyProperty ValidateValueCallback
    /// </summary>
    internal static class NameValidationHelper
    {
        // When a name string comes in programatically, validate it against the
        //  same rules used by the XAML parser.  In XAML scenarios this is
        //  technically redundant since the parser has already checked it against
        //  the same rules, but the parser is able to give a better error message
        //  when it happens.
#if !PBTCOMPILER
        [FriendAccessAllowed] // Built into Base, used by Core and Framework.
        internal static bool NameValidationCallback(object candidateName)
        {
            string name = candidateName as string;

            if( name != null )
            {
                // Non-null string, ask the XAML validation code for blessing.
                return IsValidIdentifierName(name);
            }
            else if( candidateName == null )
            {
                // Null string is allowed
                return true;
            }
            else
            {
                // candiateName is not a string object.
                return false;
            }
        }
#endif

        /// <summary>
        /// Validates the name to follow Naming guidelines
        /// </summary>
        /// <param name="name">string to validate</param>
#if !PBTCOMPILER
        [FriendAccessAllowed] // Built into Base, used by Core and Framework.
#endif
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
#endif
}
