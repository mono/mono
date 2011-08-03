// 
// ExpandedWrapper`7.cs
//  
// Author:
//       Marek Habersack <grendel@twistedcode.net>
// 
// Copyright (c) 2011 Novell, Inc
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

using System;
using System.ComponentModel;
using System.Data.Services.Internal;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace System.Data.Services.Internal
{
	[EditorBrowsable (EditorBrowsableState.Never)]
	public sealed class ExpandedWrapper <TExpandedElement, TProperty0, TProperty1, TProperty2, TProperty3, TProperty4, TProperty5> : ExpandedWrapper <TExpandedElement>
	{
		public TProperty0 ProjectedProperty0 {
			get; set;
		}

		public TProperty1 ProjectedProperty1 {
			get; set;
		}

		public TProperty2 ProjectedProperty2 {
			get; set;
		}

		public TProperty3 ProjectedProperty3 {
			get; set;
		}

		public TProperty4 ProjectedProperty4 {
			get; set;
		}

		public TProperty5 ProjectedProperty5 {
			get; set;
		}

		protected override object InternalGetExpandedPropertyValue (int nameIndex)
		{
			throw new NotImplementedException ();
		}

		public ExpandedWrapper ()
		{
			throw new NotImplementedException ();
		}
	}
}
