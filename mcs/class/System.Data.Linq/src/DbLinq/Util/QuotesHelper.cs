#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DbLinq.Util
{
    /// <summary>
    /// maintainer: Anton Andreev
    /// </summary>
#if MONO_STRICT
    internal
#else
    public
#endif 
    static class QuotesHelper
    {
        public static string AddQuotesToSequence(string idColName, string sequenceName)
        {
            if (idColName != idColName.ToLower() && !sequenceName.StartsWith("\""))//toncho11: quotes are added due to issue http://code.google.com/p/dblinq2007/issues/detail?id=27}
            {
                sequenceName = "\"" + sequenceName.Replace(".", "\".\"") + "\"";
            }
            return sequenceName;
        }

        /// <summary>
        /// Enquotes a given string, if not already enquoted
        /// </summary>
        /// <param name="name">The string to enquote</param>
        /// <param name="startQuote">The start quote</param>
        /// <param name="endQuote">The end quote</param>
        /// <returns></returns>
        public static string Enquote(string name, char startQuote, char endQuote)
        {
            if (name.Length > 0 && name[0] != startQuote)
                name = startQuote + name;
            if (name.Length > 0 && name[name.Length - 1] != endQuote)
                name = name + endQuote;
            return name;
        }

        public static string Enquote(string name, char quote)
        {
            return Enquote(name, quote, quote);
        }
    }
}
