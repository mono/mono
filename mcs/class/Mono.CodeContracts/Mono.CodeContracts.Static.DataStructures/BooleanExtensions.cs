// 
// BooleanExtensions.cs
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

using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.DataStructures
{
    static class BooleanExtensions
    {
        /// <summary>
        /// Returns value and sets result to a resultValue.
        /// </summary>
        public static bool With<T>(this bool value, T resultValue, out T result)
        {
            result = resultValue;
            return value;
        }

        /// <summary>
        /// Returns value and sets result to a default(T).
        /// </summary>
        public static bool Without<T>(this bool value, out T result)
        {
            result = default(T);
            return value;
        }

        /// <summary>
        /// Returns ProofOutcome value based on input.
        /// </summary>
        /// <param name="condition">Condition to check.</param>
        /// <returns><see cref="ProofOutcome.True"/> if condition holds, otherwise <see cref="ProofOutcome.Top"/></returns>
        public static FlatDomain<bool> ToTrueOrTop(this bool condition)
        {
            return condition ? ProofOutcome.True : ProofOutcome.Top;
        }
    }
}