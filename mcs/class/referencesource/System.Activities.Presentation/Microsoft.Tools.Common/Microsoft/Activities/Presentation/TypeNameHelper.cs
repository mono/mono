//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace Microsoft.Activities.Presentation
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;

    internal static class TypeNameHelper
    {
        // note: does not work for nested type when fullName is true
        // eg. Namespace.DeclaringType.NestedType<T> will be displayed
        // as  Namespace.DeclaringType+NestedType<T>
        public static string GetDisplayName(Type type, bool fullName)
        {
            if (type == null)
            {
                return string.Empty;
            }

            if (type.IsGenericParameter)
            {
                return type.Name;
            }

            if (!type.IsGenericType && !type.IsArray)
            {
                if (fullName)
                {
                    return type.FullName;
                }
                else
                {
                    return type.Name;
                }
            }

            // replace `2 with <Type1, Type2>
            Regex regex = new Regex("`[0-9]+");
            GenericsMatchEvaluator evaluator = new GenericsMatchEvaluator(type.GetGenericArguments(), fullName);

            // Remove [[fullName1, ..., fullNameX]]
            string name;
            if (fullName)
            {
                name = type.FullName;
            }
            else
            {
                name = type.Name;
            }

            int start = name.IndexOf("[[", StringComparison.Ordinal);
            int end = name.LastIndexOf("]]", StringComparison.Ordinal);
            if (start > 0 && end > 0)
            {
                name = name.Substring(0, start) + name.Substring(end + 2);
            }

            return regex.Replace(name, evaluator.Evaluate);
        }

        private class GenericsMatchEvaluator
        {
            private Type[] generics = null;
            private int index;
            private bool fullName;

            public GenericsMatchEvaluator(Type[] generics, bool fullName)
            {
                this.generics = generics;
                this.index = 0;
                this.fullName = fullName;
            }

            public string Evaluate(Match match)
            {
                int numberOfParameters = int.Parse(match.Value.Substring(1), CultureInfo.InvariantCulture);

                StringBuilder sb = new StringBuilder();

                // matched "`N" is replaced by "<Type1, ..., TypeN>"
                sb.Append("<");

                for (int i = 0; i < numberOfParameters; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(", ");
                    }

                    sb.Append(TypeNameHelper.GetDisplayName(this.generics[this.index++], this.fullName));
                }

                sb.Append(">");

                return sb.ToString();
            }
        }
    }
}
