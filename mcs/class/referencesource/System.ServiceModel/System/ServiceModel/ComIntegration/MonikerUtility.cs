//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Text;

    internal static class MonikerUtility
    {
        internal static string Getkeyword(string moniker, out MonikerHelper.MonikerAttribute keyword)
        {
            moniker = moniker.TrimStart();
            int indexOfEqualSign = moniker.IndexOf("=", StringComparison.Ordinal);
            if (indexOfEqualSign == -1)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(SR.GetString(SR.NoEqualSignFound, moniker)));

            int indexOfComma = moniker.IndexOf(",", StringComparison.Ordinal);

            if (indexOfComma != -1 && indexOfComma < indexOfEqualSign)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(SR.GetString(SR.NoEqualSignFound, moniker)));

            string suspectedKeyword = moniker.Substring(0, indexOfEqualSign).Trim();
            suspectedKeyword = suspectedKeyword.ToLower(System.Globalization.CultureInfo.InvariantCulture);

            foreach (MonikerHelper.KeywordInfo keywordInfo in MonikerHelper.KeywordInfo.KeywordCollection)
            {
                if (suspectedKeyword == keywordInfo.Name)
                {
                    keyword = keywordInfo.Attrib;
                    moniker = moniker.Substring(indexOfEqualSign + 1).TrimStart();
                    return moniker;
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(SR.GetString(SR.UnknownMonikerKeyword, suspectedKeyword)));
        }
        internal static string GetValue(string moniker, out string val)
        {
            StringBuilder value = new StringBuilder();
            int index = 0;
            moniker = moniker.Trim();
            if (string.IsNullOrEmpty(moniker))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(SR.GetString(SR.KewordMissingValue)));
            switch (moniker[index])
            {
                case '"':
                case '\'':
                    {
                        char quoteToCompare = moniker[index];
                        index++;

                        for (; index < moniker.Length; index++)
                        {

                            if (moniker[index] == quoteToCompare)
                            {

                                if ((index < (moniker.Length - 1)) && (moniker[index + 1] == quoteToCompare))
                                {
                                    value.Append(quoteToCompare);
                                    index++;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                                value.Append(moniker[index]);

                        }
                        if (index < moniker.Length)
                        {
                            index++;
                            if (index < moniker.Length)
                            {
                                moniker = moniker.Substring(index);
                                moniker = moniker.Trim();

                                if (!String.IsNullOrEmpty(moniker))
                                {
                                    if (moniker[0] == ',')
                                    {
                                        moniker = moniker.Substring(1);
                                        moniker = moniker.Trim();
                                    }
                                    else
                                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(SR.GetString(SR.BadlyTerminatedValue, value.ToString())));
                                }


                            }
                            else
                                moniker = "";


                        }
                        else
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(SR.GetString(SR.MissingQuote, value.ToString())));

                        break;
                    }
                default:
                    {
                        for (; (index < moniker.Length) && (moniker[index] != ','); index++)
                            value.Append(moniker[index]);
                        if (index < moniker.Length)
                        {
                            index++;
                            if (index < moniker.Length)
                            {
                                moniker = moniker.Substring(index);
                                moniker = moniker.Trim();
                            }

                        }
                        else
                            moniker = "";

                        break;
                    }
            }
            val = value.ToString().Trim();
            return moniker;
        }
        internal static void Parse(string displayName, ref Dictionary<MonikerHelper.MonikerAttribute, string> propertyTable)
        {
            int indexOfMonikerData = displayName.IndexOf(":", StringComparison.Ordinal);
            if (indexOfMonikerData == -1)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(SR.GetString(SR.MonikerMissingColon)));
            string monikerParams = displayName.Substring(indexOfMonikerData + 1).Trim();
            MonikerHelper.MonikerAttribute keyword;
            string value;

            while (!string.IsNullOrEmpty(monikerParams))
            {
                monikerParams = Getkeyword(monikerParams, out keyword);
                propertyTable.TryGetValue(keyword, out value);
                if (!String.IsNullOrEmpty(value))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(SR.GetString(SR.RepeatedKeyword)));
                monikerParams = GetValue(monikerParams, out value);
                propertyTable[keyword] = value;
            }
        }
    }
}
