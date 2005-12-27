//
// Commons.Xml.Relaxng.RelaxngPattern.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// 2003 Atsushi Enomoto "No rights reserved."
//
// Copyright (c) 2004 Novell Inc.
// All rights reserved
//

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
using System.Collections;
using System.Collections.Specialized;

namespace Commons.Xml.Relaxng
{
	//
	// Pattern Classes
	//
	public enum RelaxngPatternType
	{
		Empty = 1,
		NotAllowed = 2,
		Text = 3,
		Choice = 4,
		Interleave = 5,
		Group = 6,
		OneOrMore = 7,
		List = 8,
		Data = 9,
		DataExcept = 10,	// only use in derivative
		Value = 11,
		Attribute = 12,
		Element = 13,
		After = 14,		// only use in derivative
		Ref = 15,		// no use in derivative
		Grammar = 16,		// no use in derivative
		ZeroOrMore = 17,	// no use in derivative
		Mixed = 18,		// no use in derivative
		Optional = 19,		// no use in derivative
//		Include = 20,		// no use in derivative
		ExternalRef = 21,	// no use in derivative
		ParentRef = 22,		// no use in derivative
	}
}

