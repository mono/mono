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

#if MONO_STRICT
namespace System.Data.Linq.Sugar
#else
namespace DbLinq.Data.Linq.Sugar
#endif
{
    internal enum ExpressionPrecedence
    {
        /// <summary>
        /// x.y  f(x)  a[x]  x++  x--  new typeof  checked  unchecked
        /// </summary>
        Primary,
        /// <summary>
        /// +  -  !  ~  ++x  --x  (T)x
        /// </summary>
        Unary,
        /// <summary>
        /// *  /  %
        /// </summary>
        Multiplicative,
        /// <summary>
        /// +  -
        /// </summary>
        Additive,
        /// <summary>
        /// &lt;&lt;  >>
        /// </summary>
        Shift,
        /// <summary>
        /// &lt;  >  &lt;=  >=  is  as
        /// </summary>
        RelationalAndTypeTest,
        /// <summary>
        /// ==  !=
        /// </summary>
        Equality,
        /// <summary>
        /// &amp;
        /// </summary>
        LogicalAnd,
        /// <summary>
        /// ^
        /// </summary>
        LogicalXor,
        /// <summary>
        /// |
        /// </summary>
        LogicalOr,
        /// <summary>
        /// &amp;&amp,
        /// </summary>
        ConditionalAnd,
        /// <summary>
        /// ||
        /// </summary>
        ConditionalOr,
        /// <summary>
        /// ??
        /// </summary>
        NullCoalescing,
        /// <summary>
        /// ?:
        /// </summary>
        Conditional,
        /// <summary>
        /// =  *=  /=  %=  +=  -=  <<=  >>=  &=  ^=  |=
        /// </summary>
        Assignment,

        /// <summary>
        /// A SQL clause, FROM, WHERE, etc.
        /// </summary>
        Clause
    }
}