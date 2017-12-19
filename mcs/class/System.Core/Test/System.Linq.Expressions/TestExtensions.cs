// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using System.Threading;
using Xunit;

namespace System.Linq.Expressions.Tests
{
    public static class CompilerTests
    {
        public static void VerifyIL(this LambdaExpression expression, string expected, bool appendInnerLambdas = false)
        {
            string actual = expression.GetIL(appendInnerLambdas);

            string nExpected = Normalize(expected);
            string nActual = Normalize(actual);

            Assert.Equal(nExpected, nActual);
        }

        private static string Normalize(string s)
        {
            Collections.Generic.IEnumerable<string> lines =
                s
                .Replace("\r\n", "\n")
                .Split(new[] { '\n' })
                .Select(line => line.Trim())
                .Where(line => line != "" && !line.StartsWith("//"));

            return string.Join("\n", lines);
        }
    }
}