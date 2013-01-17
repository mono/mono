// 
// ProofOutcome.cs
// 
// Authors:
// 	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2011 Alexander Chebaturkin
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

using System;
using System.ComponentModel;

using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static {
    static class ProofOutcome {
        /// <summary>
        /// Can be true or false.
        /// </summary>
        public static readonly FlatDomain<bool> Top = FlatDomain<bool>.TopValue;
        /// <summary>
        /// Unreachable.
        /// </summary>
        public static readonly FlatDomain<bool> Bottom = FlatDomain<bool>.BottomValue;
        /// <summary>
        /// Definitely true.
        /// </summary>
        public static readonly FlatDomain<bool> True = true;
        /// <summary>
        /// Definitely false.
        /// </summary>
        public static readonly FlatDomain<bool> False = false;

        public static FlatDomain<bool> Negate(this FlatDomain<bool> o)
        {
            if (o.IsNormal())
                return !o.IsTrue();

            return o;
        }

        public static bool IsTrue(this FlatDomain<bool> o) 
        {
            return o.IsNormal () && o.Value;
        }

        public static bool IsFalse(this FlatDomain<bool> o)
        {
            return o.IsNormal () && !o.Value;
        }
    }
}
