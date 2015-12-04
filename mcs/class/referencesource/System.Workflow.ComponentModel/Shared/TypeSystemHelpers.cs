// Copyright (c) Microsoft Corporation. All rights reserved. 
//  
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// WHETHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
// WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// THE ENTIRE RISK OF USE OR RESULTS IN CONNECTION WITH THE USE OF THIS CODE 
// AND INFORMATION REMAINS WITH THE USER. 


/*********************************************************************
 * NOTE: A copy of this file exists at: WF\Activities\Common
 * The two files must be kept in [....].  Any change made here must also
 * be made to WF\Activities\Common\TypeSystemHelpers.cs
*********************************************************************/
namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.CodeDom;
    using System.Text.RegularExpressions;
    using System.Diagnostics.CodeAnalysis;

    internal static class ParseHelpers
    {
        private static readonly Version emptyVersion = new Version(0, 0, 0, 0);
        private const string VersionTag = "version";
        private const string CultureTag = "culture";
        private const string PublicKeyTokenTag = "publickeytoken";

        private static readonly ArrayList VBKeywords = new ArrayList(new string[] { "Integer", "String", "Boolean", "Object", "Void", "Single", "Double", "Char", "DateTime", "Long", "Byte", "Short", "Single", "Double", "Decimal", "UInteger", "ULong", "SByte", "UShort" });
        private static readonly ArrayList CSKeywords = new ArrayList(new string[] { "int", "string", "bool", "object", "void", "float", "double", "char", "Date", "long", "byte", "short", "Single", "double", "decimal", "uint", "ulong", "sbyte", "ushort" });
        private static readonly string[] DotNetKeywords = new string[] { "System.Int32", "System.String", "System.Boolean", "System.Object", "System.Void", "System.Single", "System.Double", "System.Char", "System.DateTime", "System.Int64", "System.Byte", "System.Int16", "System.Single", "System.Double", "System.Decimal", "System.UInt32", "System.UInt64", "System.SByte", "System.UInt16" };

        internal enum ParseTypeNameLanguage
        {
            VB,
            CSharp,
            NetFramework
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static bool ParseTypeName(string inputTypeName, ParseTypeNameLanguage parseTypeNameLanguage, out string typeName, out string[] parameters, out string elemantDecorator)
        {
            typeName = string.Empty;
            parameters = null;
            elemantDecorator = string.Empty;

            // replace all language specific array\generic chars with the net-framework's representation
            if (parseTypeNameLanguage == ParseTypeNameLanguage.VB)
                inputTypeName = inputTypeName.Replace('(', '[').Replace(')', ']');
            else if (parseTypeNameLanguage == ParseTypeNameLanguage.CSharp)
                inputTypeName = inputTypeName.Replace('<', '[').Replace('>', ']');

            int endIndex = inputTypeName.LastIndexOfAny(new char[] { ']', '&', '*' });
            if (endIndex == -1)
            {
                // "simple" type
                typeName = inputTypeName;
            }
            else if (inputTypeName[endIndex] == ']') //array or generic
            {
                int startIndex = endIndex;
                int nestLevel = 1;
                while ((startIndex > 0) && (nestLevel > 0))
                {
                    startIndex--;
                    if (inputTypeName[startIndex] == ']')
                        nestLevel++;
                    else if (inputTypeName[startIndex] == '[')
                        nestLevel--;
                }
                if (nestLevel != 0)
                    return false;

                typeName = inputTypeName.Substring(0, startIndex) + inputTypeName.Substring(endIndex + 1);

                string bracketContent = inputTypeName.Substring(startIndex + 1, endIndex - startIndex - 1).Trim();
                if ((bracketContent == String.Empty) || (bracketContent.TrimStart()[0] == ','))
                {
                    // array
                    elemantDecorator = "[" + bracketContent + "]";
                }
                else
                {
                    // Isolate the parameters (looking for commas alone will not cover cases
                    // when parameters are multi-dim array or generics...
                    int nestingLevel = 0;
                    char[] genericParamChars = bracketContent.ToCharArray();
                    for (int loop = 0; loop < genericParamChars.Length; loop++)
                    {
                        if (genericParamChars[loop] == '[')
                            nestingLevel++;
                        else if (genericParamChars[loop] == ']')
                            nestingLevel--;
                        else if ((genericParamChars[loop] == ',') && (nestingLevel == 0))
                            genericParamChars[loop] = '$';
                    }
                    // split to get the list of generic arguments
                    parameters = new string(genericParamChars).Split(new char[] { '$' });

                    // clean the parameters
                    for (int loop = 0; loop < parameters.Length; loop++)
                    {
                        parameters[loop] = parameters[loop].Trim();

                        // remove extra brackects if exist
                        if (parameters[loop][0] == '[')
                            parameters[loop] = parameters[loop].Substring(1, parameters[loop].Length - 2);

                        // remove the "Of " keyword form VB parameters
                        if ((parseTypeNameLanguage == ParseTypeNameLanguage.VB) && (parameters[loop].StartsWith("Of ", StringComparison.OrdinalIgnoreCase)))
                            parameters[loop] = parameters[loop].Substring(3).TrimStart();
                    }
                }
            }
            else // byref, pointer
            {
                typeName = inputTypeName.Substring(0, endIndex) + inputTypeName.Substring(endIndex + 1);
                elemantDecorator = inputTypeName.Substring(endIndex, 1);
            }

            //Work around: we need to account for these langugue keywords and provide the correct type for them.
            //      A tighter way to achieve this should be found.
            if ((parseTypeNameLanguage == ParseTypeNameLanguage.CSharp) && CSKeywords.Contains(typeName))
                typeName = DotNetKeywords[CSKeywords.IndexOf(typeName)];
            else if ((parseTypeNameLanguage == ParseTypeNameLanguage.VB) && VBKeywords.Contains(typeName))
                typeName = DotNetKeywords[VBKeywords.IndexOf(typeName)];

            return true;
        }

        internal static bool AssemblyNameEquals(AssemblyName thisName, AssemblyName thatName)
        {
            // Simplest check -- the assembly name must match.
            if (thisName.Name == null || thatName.Name == null)
                return false;

            if (!thatName.Name.Equals(thisName.Name))
                return false;

            // Next, version checks.  We are comparing AGAINST thatName,
            // so if thatName has a version defined, we must match.
            Version thatVersion = thatName.Version;
            if (thatVersion != null && thatVersion != emptyVersion && thatVersion != thisName.Version)
                return false;

            // Same story for culture
            CultureInfo thatCulture = thatName.CultureInfo;
            if (thatCulture != null && !thatCulture.Equals(CultureInfo.InvariantCulture))
            {
                CultureInfo thisCulture = thisName.CultureInfo;
                if (thisCulture == null)
                    return false;

                // the requested culture must either equal, or be a parent of
                // our culture.
                do
                {
                    if (thatCulture.Equals(thisCulture))
                        break;
                    thisCulture = thisCulture.Parent;
                    if (thisCulture.Equals(CultureInfo.InvariantCulture))
                        return false;
                } while (true);
            }

            // And the same thing for the public token
            byte[] thatToken = thatName.GetPublicKeyToken();
            if (thatToken != null && thatToken.Length != 0)
            {
                byte[] thisToken = thisName.GetPublicKeyToken();
                if (thisToken == null)
                    return false;
                if (thatToken.Length != thisToken.Length)
                    return false;
                for (int i = 0; i < thatToken.Length; i++)
                {
                    if (thatToken[i] != thisToken[i])
                        return false;
                }
            }
            return true;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static bool AssemblyNameEquals(AssemblyName thisName, string thatName)
        {
            if (thisName == null || string.IsNullOrEmpty(thisName.Name))
                return false;
            if (string.IsNullOrEmpty(thatName))
                return false;

            string[] parts = thatName.Split(',');
            if (parts.Length == 0)
                return false;

            string thatAssemblyName = parts[0].Trim();
            if (!thatAssemblyName.Equals(thisName.Name))
                return false;

            if (parts.Length == 1)
                return true;

            Version thatVersion = null;
            CultureInfo thatCulture = null;
            byte[] thatToken = null;
            for (int index = 1; index < parts.Length; index++)
            {
                int indexOfEquals = parts[index].IndexOf('=');
                if (indexOfEquals != -1)
                {
                    string partName = parts[index].Substring(0, indexOfEquals).Trim().ToLowerInvariant();
                    string partValue = parts[index].Substring(indexOfEquals + 1).Trim().ToLowerInvariant();
                    if (string.IsNullOrEmpty(partValue))
                        continue;

                    switch (partName)
                    {
                        case ParseHelpers.VersionTag:
                            thatVersion = new Version(partValue);
                            break;
                        case ParseHelpers.CultureTag:
                            if (!string.Equals(partValue, "neutral", StringComparison.OrdinalIgnoreCase))
                                thatCulture = new CultureInfo(partValue);
                            break;
                        case ParseHelpers.PublicKeyTokenTag:
                            if (!string.Equals(partValue, "null", StringComparison.OrdinalIgnoreCase))
                            {
                                thatToken = new byte[partValue.Length / 2];
                                for (int i = 0; i < thatToken.Length; i++)
                                    thatToken[i] = Byte.Parse(partValue.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            if (thatVersion != null && thatVersion != emptyVersion && thatVersion != thisName.Version)
                return false;

            if (thatCulture != null && !thatCulture.Equals(CultureInfo.InvariantCulture))
            {
                CultureInfo thisCulture = thisName.CultureInfo;
                if (thisCulture == null)
                    return false;

                // the requested culture must either equal, or be a parent of
                // our culture.
                do
                {
                    if (thatCulture.Equals(thisCulture))
                        break;
                    thisCulture = thisCulture.Parent;
                    if (thisCulture.Equals(CultureInfo.InvariantCulture))
                        return false;
                } while (true);
            }

            if (thatToken != null && thatToken.Length != 0)
            {
                byte[] thisToken = thisName.GetPublicKeyToken();
                if (thisToken == null)
                    return false;
                if (thatToken.Length != thisToken.Length)
                    return false;
                for (int i = 0; i < thatToken.Length; i++)
                {
                    if (thatToken[i] != thisToken[i])
                        return false;
                }
            }

            return true;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static string FormatType(Type type, SupportedLanguages language)
        {
            string typeName = string.Empty;
            if (type.IsArray)
            {
                typeName = FormatType(type.GetElementType(), language);
                if (language == SupportedLanguages.CSharp)
                    typeName += '[';
                else
                    typeName += '(';

                typeName += new string(',', type.GetArrayRank() - 1);
                if (language == SupportedLanguages.CSharp)
                    typeName += ']';
                else
                    typeName += ')';
            }
            else
            {
                typeName = type.FullName;
                int indexOfSpecialChar = typeName.IndexOf('`');
                if (indexOfSpecialChar != -1)
                    typeName = typeName.Substring(0, indexOfSpecialChar);
                typeName = typeName.Replace('+', '.');

                if (type.ContainsGenericParameters || type.IsGenericType)
                {
                    Type[] genericArguments = type.GetGenericArguments();
                    if (language == SupportedLanguages.CSharp)
                        typeName += '<';
                    else
                        typeName += '(';

                    bool first = true;
                    foreach (Type genericArgument in genericArguments)
                    {
                        if (!first)
                            typeName += ", ";
                        else
                        {
                            if (language == SupportedLanguages.VB)
                                typeName += "Of ";
                            first = false;
                        }
                        typeName += FormatType(genericArgument, language);
                    }

                    if (language == SupportedLanguages.CSharp)
                        typeName += '>';
                    else
                        typeName += ')';
                }
            }
            return typeName;
        }

        // Helper method to format a type name (language invariant) to a specific language
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static string FormatType(string type, SupportedLanguages language)
        {
            string formattedType = string.Empty;
            string[] genericParamTypeNames = null;
            string baseTypeName = string.Empty;
            string elementDecorators = string.Empty;
            if (ParseHelpers.ParseTypeName(type, ParseHelpers.ParseTypeNameLanguage.NetFramework, out baseTypeName, out genericParamTypeNames, out elementDecorators))
            {
                if (elementDecorators.Length > 0)
                {
                    // VB uses '()' for arrays
                    if (language == SupportedLanguages.VB)
                        elementDecorators = elementDecorators.Replace('[', '(').Replace(']', ')');

                    formattedType = FormatType(baseTypeName, language) + elementDecorators;
                }
                else if (genericParamTypeNames != null && genericParamTypeNames.Length > 0)
                {
                    // add generic type
                    formattedType = FormatType(baseTypeName, language);

                    // add generic arguments
                    if (language == SupportedLanguages.CSharp)
                        formattedType += '<';
                    else
                        formattedType += '(';

                    bool first = true;
                    foreach (string genericArgument in genericParamTypeNames)
                    {
                        if (!first)
                            formattedType += ", ";
                        else
                        {
                            if (language == SupportedLanguages.VB)
                                formattedType += "Of ";
                            first = false;
                        }

                        formattedType += FormatType(genericArgument, language);
                    }

                    if (language == SupportedLanguages.CSharp)
                        formattedType += '>';
                    else
                        formattedType += ')';
                }
                else
                {
                    // non generic, non decorated type - simple cleanup
                    formattedType = baseTypeName.Replace('+', '.');

                    int indexOfSpecialChar = formattedType.IndexOf('`');
                    if (indexOfSpecialChar != -1)
                        formattedType = formattedType.Substring(0, indexOfSpecialChar);

                    indexOfSpecialChar = formattedType.IndexOf(',');
                    if (indexOfSpecialChar != -1)
                        formattedType = formattedType.Substring(0, indexOfSpecialChar);

                }
            }

            return formattedType;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static Type ParseTypeName(ITypeProvider typeProvider, SupportedLanguages language, string typeName)
        {
            Type returnType = null;

            returnType = typeProvider.GetType(typeName, false);
            if (returnType == null)
            {
                string simpleTypeName = String.Empty;
                string decoratorString = String.Empty;
                string[] parameters = null;
                if (ParseTypeName(typeName, language == SupportedLanguages.CSharp ? ParseTypeNameLanguage.CSharp : ParseTypeNameLanguage.VB, out simpleTypeName, out parameters, out decoratorString))
                {
                    returnType = typeProvider.GetType(simpleTypeName + decoratorString, false);
                }
            }

            return returnType;
        }
    }
}
