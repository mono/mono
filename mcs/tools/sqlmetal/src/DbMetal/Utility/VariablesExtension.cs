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
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DbMetal.Utility
{
    public static class VariableExtension
    {
        private static Regex Variable = new Regex(@"\$\{(?<var>[\w.]+)\}", RegexOptions.Singleline | RegexOptions.Compiled);

        private static object EvaluateMember<T>(IDictionary<string, T> variables, string memberPath, string[] extraParameters)
        {
            string[] parts = memberPath.Split('.');
            object o = null;
            if (variables.ContainsKey(parts[0]))
                o = variables[parts[0]];
            else
            {
                int index;
                if (int.TryParse(parts[0], out index))
                {
                    if (index < extraParameters.Length)
                        o = extraParameters[index];
                }
            }
            for (int memberIndex = 1; memberIndex < parts.Length; memberIndex++)
            {
                if (o == null)
                    break;
                PropertyInfo info = o.GetType().GetProperty(parts[memberIndex]);
                if (info != null)
                    o = info.GetGetMethod().Invoke(o, new object[0]);
                else
                    o = null;
            }
            return o;
        }

        public static string Evaluate<T>(this IDictionary<string, T> variables, string expression, params string[] extraParameters)
        {
            if (expression == null)
                return null;
            return Variable.Replace(expression, delegate(Match e)
                                                    {
                                                        string k = e.Result("${var}");
                                                        object o = EvaluateMember(variables, k, extraParameters);
                                                        return o != null ? o.ToString() : string.Empty;
                                                    });
        }

        public static IDictionary<string, T> LocalCopy<T>(this IDictionary<string, T> variables, params object[] localValues)
        {
            IDictionary<string, T> localCopy = new Dictionary<string, T>(variables);
            for (int valueIndex = 0; valueIndex < localValues.Length; valueIndex += 2)
            {
                localCopy[(string)localValues[valueIndex]] = (T)localValues[valueIndex + 1];
            }
            return localCopy;
        }
    }
}