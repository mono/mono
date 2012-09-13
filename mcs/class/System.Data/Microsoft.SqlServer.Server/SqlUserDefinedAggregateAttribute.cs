//
// Microsoft.SqlServer.Server.SqlUserDefinedAggregateAttribute
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Umadevi S (sumadevi@novell.com)
//
// Copyright (C) Tim Coleman, 2003
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;

namespace Microsoft.SqlServer.Server {
	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
	public sealed class SqlUserDefinedAggregateAttribute : Attribute
	{
		#region Fields

		public const int MaxByteSizeValue = 8000;

		Format format;
		bool isInvariantToDuplicates;
		bool isInvariantToNulls;
		bool isInvariantToOrder;
		bool isNullIfEmpty;
		int maxByteSize;

		#endregion // Fields

		#region Constructors

		public SqlUserDefinedAggregateAttribute (Format format)
		{
			this.format = format;
			IsInvariantToDuplicates = false;
			IsInvariantToNulls = false;
			IsInvariantToOrder = false;
			IsNullIfEmpty = false;
			MaxByteSize = MaxByteSizeValue;
		}

		#endregion // Constructors

		#region Properties

		public Format Format { 
			get { return format; }
		}

		public bool IsInvariantToDuplicates {
			get { return isInvariantToDuplicates; }
			set { isInvariantToDuplicates = value; }
		}

		public bool IsInvariantToNulls {
			get { return isInvariantToNulls; }
			set { isInvariantToNulls = value; }
		}

		public bool IsInvariantToOrder {
			get { return isInvariantToOrder; }
			set { isInvariantToOrder = value; }
		}

		public bool IsNullIfEmpty {
			get { return isNullIfEmpty; }
			set { isNullIfEmpty = value; }
		}

		public int MaxByteSize {
			get { return maxByteSize; }
			set { maxByteSize = value; }
		}

		#endregion // Properties
	}
}

#endif
