//
// Commons.Xml.Relaxng.RngPattern.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// 2003 Atsushi Enomoto "No rights reserved."
//
using System;
using System.Collections;
using System.Collections.Specialized;

namespace Commons.Xml.Relaxng
{
	//
	// Pattern Classes
	//
	public enum RngPatternType
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
		Include = 20,		// no use in derivative
		ExternalRef = 21,	// no use in derivative
		ParentRef = 22,		// no use in derivative
	}
}

