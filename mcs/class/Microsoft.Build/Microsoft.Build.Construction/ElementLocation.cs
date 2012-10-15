//
// ProjectItemDefinitionInstance.cs
//
// Author:
//   Atsushi Enomoto (atsushi@veritas-vos-liberabit.com)
//
// Copyright (C) 2012 Xamarin Inc.
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

#if NET_4_5

using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;

namespace Microsoft.Build.Construction
{
	[Serializable]
	public abstract class ElementLocation
	{
		public abstract int Column { get; }
		public abstract string File { get; }
		public abstract int Line { get; }

		public string LocationString {
			get { return Line == 0 ? File : String.Format ("{0} ({1}{2})", File, Line, Column != 0 ? "," + Column : String.Empty); }
		}
	}
}

#endif
