// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Xaml.MS.Impl;
using System.Collections.Generic;

namespace MS.Internal.Xaml.Parser
{
    internal abstract class XamlName
    {
        public const char PlusSign = '+';
        public const char UnderScore = '_';
        public const char Dot = '.';

        public string Name { get; protected set; }

        protected XamlName() : this(string.Empty) { }

        public XamlName(string name)
        {
            Name = name;
        }

        public XamlName(string prefix, string name)
        {
            Name = name;
            _prefix = prefix ?? string.Empty;
        }

        public abstract string ScopedName { get; }

        protected string _prefix;
        protected string _namespace = null;

        public string Prefix { get { return _prefix; } }
        public string Namespace { get { return _namespace; } }

        public static bool ContainsDot(string name)
        {
            return name.Contains(".");
        }

        public static bool IsValidXamlName(string name)
        {
            if (name.Length == 0)
            {
                return false;
            }

            if (!IsValidNameStartChar(name[0]))
            {
                return false;
            }

            for (int i = 1; i < name.Length; i++)
            {
                if (!IsValidNameChar(name[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsValidNameStartChar(char ch)
        {
            return char.IsLetter(ch) || ch == UnderScore;
        }

        public static bool IsValidNameChar(char ch)
        {
            if (IsValidNameStartChar(ch) || char.IsDigit(ch))
            {
                return true;
            }

            var unicodeCategory = char.GetUnicodeCategory(ch);
            if (unicodeCategory == UnicodeCategory.NonSpacingMark || unicodeCategory == UnicodeCategory.SpacingCombiningMark)
            {
                return true;
            }
            return false;
        }

        public static bool IsValidQualifiedNameChar(char ch)
        {
            return ch == Dot || IsValidNameChar(ch);
        }

        public static bool IsValidQualifiedNameCharPlus(char ch)
        {
            return IsValidQualifiedNameChar(ch) || ch == PlusSign;
        }
    }
}
