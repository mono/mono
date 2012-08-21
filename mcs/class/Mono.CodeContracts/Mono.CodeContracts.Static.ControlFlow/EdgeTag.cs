// 
// EdgeTag.cs
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

namespace Mono.CodeContracts.Static.ControlFlow {
	[Flags]
	enum EdgeTag : uint {
		None              = 0,
		FallThroughReturn = 1,
		Branch            = 1 << 1,
		Return            = 1 << 2,
		EndSubroutine     = 1 << 3,
		True              = 1 << 4,
		False             = 1 << 5,
		FallThrough       = 1 << 6,
		Entry             = 1 << 7,
		AfterNewObj       = 1 << 8  | AfterMask,
		AfterCall         = 1 << 9  | AfterMask,
		Exit              = 1 << 10,
		Finally           = 1 << 11,
		Inherited         = 1 << 12 | InheritedMask,
		BeforeCall        = 1 << 13 | BeforeMask,
		BeforeNewObj      = 1 << 14 | BeforeMask,
		Requires          = 1 << 15,
		Assume            = 1 << 16,
		Assert            = 1 << 17,
		Invariant         = 1 << 18,
		OldManifest       = 1 << 19 | OldMask,
		Old               = 1 << 20 | OldMask,
		EndOld            = 1 << 21,

		BeforeMask        = 1 << 22,
		AfterMask         = 1 << 23,
		InheritedMask     = 1 << 24,
		ExtraMask         = 1 << 25,
		OldMask           = 1 << 26,
	}
}
