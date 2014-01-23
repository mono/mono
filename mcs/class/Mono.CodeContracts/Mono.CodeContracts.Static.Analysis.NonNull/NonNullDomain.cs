// 
// Domain.cs
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
using System.IO;

using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Analysis.NonNull {
	struct NonNullDomain<V> where V : IEquatable<V> {
		public static readonly NonNullDomain<V> BottomValue = new NonNullDomain<V> (SetDomain<V>.BottomValue, SetDomain<V>.BottomValue);

		public SetDomain<V> NonNulls;
		public SetDomain<V> Nulls;

		public NonNullDomain(SetDomain<V> nonNulls, SetDomain<V> nulls)
		{
			this.NonNulls = nonNulls;
			this.Nulls = nulls;
		}

		public bool IsNonNull(V v)
		{
			return this.NonNulls.Contains (v);
		}

		public bool IsNull(V v)
		{
			return this.Nulls.Contains (v);
		}

        public override string ToString()
        {
            var sw = new StringWriter();

            sw.WriteLine("Nulls:");
                sw.WriteLine("<");
                this.Nulls.Dump (sw);
                sw.WriteLine(">");

            sw.WriteLine("Non-Nulls:");
                sw.WriteLine("<");
                this.NonNulls.Dump(sw);
                sw.WriteLine(">");

            return sw.ToString ();
        }
	}
}