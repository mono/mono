//------------------------------------------------------------------------------
// <copyright file="_ShellExpression.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net
{
    using System;

    internal struct ShellExpression
    {
        private enum ShExpTokens
        {
            Asterisk = -1,
            Question = -2,
            AugmentedDot = -3,
            AugmentedAsterisk = -4,
            AugmentedQuestion = -5,
            Start = -6,
            End = -7
        }

        private ShExpTokens[] pattern;
        private int[] match;

        // Normally would defer parsing, but we want to throw here if it's invalid.
        internal ShellExpression(string pattern)
        {
            this.pattern = null;
            this.match = null;
            GlobalLog.Print("ShellServices.ShellExpression#" + ValidationHelper.HashString(this) + "::.ctor() pattern:" + ValidationHelper.ToString(pattern));
            Parse(pattern);
        }

        /*
        // Consider removing.
        internal void SetPattern(string pattern)
        {
            GlobalLog.Print("ShellServices.ShellExpression#" + ValidationHelper.HashString(this) + "::SetPattern() pattern:" + ValidationHelper.ToString(pattern));
            Parse(pattern);
        }
        */

        internal bool IsMatch(string target)
        {
            GlobalLog.Print("ShellServices.ShellExpression#" + ValidationHelper.HashString(this) + "::IsMatch() target:" + ValidationHelper.ToString(target));
            int i = 0;
            int j = 0;
            bool reverse = false;
            bool matched = false;

            while (true)
            {
                if (!reverse)
                {
                    if (j > target.Length)
                    {
                        break;
                    }

                    switch (pattern[i])
                    {
                        case ShExpTokens.Asterisk:
                            match[i++] = j = target.Length;
                            continue;

                        case ShExpTokens.Question:
                            if (j == target.Length)
                            {
                                reverse = true;
                            }
                            else
                            {
                                match[i++] = ++j;
                            }
                            continue;

                        case ShExpTokens.AugmentedDot:
                            if (j == target.Length)
                            {
                                match[i++] = j;
                            }
                            else if (target[j] == '.')
                            {
                                match[i++] = ++j;
                            }
                            else
                            {
                                reverse = true;
                            }
                            continue;

                        case ShExpTokens.AugmentedAsterisk:
                            if (j == target.Length || target[j] == '.')
                            {
                                reverse = true;
                            }
                            else
                            {
                                match[i++] = ++j;
                            }
                            continue;

                        case ShExpTokens.AugmentedQuestion:
                            if (j == target.Length || target[j] == '.')
                            {
                                match[i++] = j;
                            }
                            else
                            {
                                match[i++] = ++j;
                            }
                            continue;

                        case ShExpTokens.Start:
                            if (j != 0)
                            {
                                break;
                            }
                            match[i++] = 0;
                            continue;

                        case ShExpTokens.End:
                            if (j == target.Length)
                            {
                                matched = true;
                                break;
                            }
                            reverse = true;
                            continue;

                        default:
                            if (j < target.Length && (int) pattern[i] == (int) char.ToLowerInvariant(target[j]))
                            {
                                match[i++] = ++j;
                            }
                            else
                            {
                                reverse = true;
                            }
                            continue;
                    }
                }
                else
                {
                    switch (pattern[--i])
                    {
                        case ShExpTokens.Asterisk:
                        case ShExpTokens.AugmentedQuestion:
                            if (match[i] != match[i - 1])
                            {
                                j = --match[i++];
                                reverse = false;
                            }
                            continue;

                        case ShExpTokens.Start:
                        case ShExpTokens.End:
                            break;

                        case ShExpTokens.Question:
                        case ShExpTokens.AugmentedDot:
                        case ShExpTokens.AugmentedAsterisk:
                        default:
                            continue;
                    }
                }
                break;
            }

            GlobalLog.Print("ShellServices.ShellExpression#" + ValidationHelper.HashString(this) + "::IsMatch() return:" + matched.ToString());
            return matched;
        }

        private void Parse(string patString)
        {
            pattern = new ShExpTokens[patString.Length + 2];  // 2 for the start, end
            match = null;
            int i = 0;

            pattern[i++] = ShExpTokens.Start;
            for (int j = 0; j < patString.Length; j++)
            {
                switch (patString[j])
                {
                    case '?':
                        pattern[i++] = ShExpTokens.Question;
                        break;

                    case '*':
                        pattern[i++] = ShExpTokens.Asterisk;
                        break;

                    case '^':
                        if (j < patString.Length - 1)
                        {
                            j++;
                        }
                        else
                        {
                            pattern = null;
                            if (Logging.On) Logging.PrintWarning(Logging.Web, SR.GetString(SR.net_log_shell_expression_pattern_format_warning, patString));
                            throw new FormatException(SR.GetString(SR.net_format_shexp, patString));
                        }
                        switch (patString[j])
                        {
                            case '.':
                                pattern[i++] = ShExpTokens.AugmentedDot;
                                break;

                            case '?':
                                pattern[i++] = ShExpTokens.AugmentedQuestion;
                                break;

                            case '*':
                                pattern[i++] = ShExpTokens.AugmentedAsterisk;
                                break;

                            default:
                                pattern = null;
                                if (Logging.On) Logging.PrintWarning(Logging.Web, SR.GetString(SR.net_log_shell_expression_pattern_format_warning, patString));
                                throw new FormatException(SR.GetString(SR.net_format_shexp, patString));
                        }
                        break;

                    default:
                        pattern[i++] = (ShExpTokens) (int) char.ToLowerInvariant(patString[j]);
                        break;
                }
            }

            pattern[i++] = ShExpTokens.End;
            match = new int[i];
        }
    }
}
