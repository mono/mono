// 
// PolynomialTests.cs
// 
// Authors:
// 	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2012 Alexander Chebaturkin
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//  
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 

using Mono.CodeContracts.Static.Analysis.Numerical;
using Mono.CodeContracts.Static.DataStructures;

using NUnit.Framework;

namespace Test
{
    [TestFixture]
    public class PolynomialTests
    {
        [Test]
        public void Renaming ()
        {
            var _5xy = Monomial<string>.From (Rational.For (5L), new[] { "x", "y" });
            var _4y = Monomial<string>.From (Rational.For (5L), new[] { "y" });
            var _1x = Monomial<string>.From (Rational.For (1L), new[] { "x"});
            var poly = new Polynomial<string, int> (
                ExpressionOperator.GreaterThan, new[] { _5xy }, new[] { _4y, _1x });
            Polynomial<string, int> poly1;
            var result = poly.TryToCanonicalForm (out poly1);
        }
    }
}