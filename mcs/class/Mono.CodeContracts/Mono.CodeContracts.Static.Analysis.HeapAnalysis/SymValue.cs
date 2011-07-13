// 
// SymValue.cs
// 
// Authors:
//	Alexander Chebaturkin (chebaturkin@gmail.com)
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

namespace Mono.CodeContracts.Static.Analysis.HeapAnalysis {
	sealed class SymValue : IEquatable<SymValue>, IComparable<SymValue>, IComparable {
		private static int _globalIdGenerator;

		public readonly int UniqueId;
		public readonly int GlobalId;

		public SymValue (int uniqueId)
		{
			this.UniqueId = uniqueId;
			this.GlobalId = ++_globalIdGenerator;
		}

		#region IComparable Members
		public int CompareTo (object obj)
		{
			var that = obj as SymValue;
			if (that == null)
				return 1;

			return CompareTo (that);
		}
		#endregion

		#region Implementation of IEquatable<SymValue>
		public bool Equals (SymValue other)
		{
			return this == other;
		}
		#endregion

		#region IComparable<SymValue> Members
		public int CompareTo (SymValue other)
		{
			return this.UniqueId - other.UniqueId;
		}
		#endregion

		public static int GetUniqueKey (SymValue sv)
		{
			return sv == null ? 0 : sv.GlobalId;
		}

		public override string ToString ()
		{
			return string.Format ("sv{0} ({1})", this.UniqueId, this.GlobalId);
		}
	}
}
