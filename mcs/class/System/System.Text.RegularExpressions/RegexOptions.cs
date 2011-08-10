//
// assembly:	System
// namespace:	System.Text.RegularExpressions
// file:	regex.cs
//
// author:	Dan Lewis (dlewis@gmx.co.uk)
// 		(c) 2002

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

namespace System.Text.RegularExpressions {

	[Flags]
	public enum RegexOptions {
		None				= 0x000,
		IgnoreCase			= 0x001,
		Multiline			= 0x002,
		ExplicitCapture			= 0x004,
#if MOBILE || !NET_2_1
		Compiled			= 0x008,
#endif
		Singleline			= 0x010,
		IgnorePatternWhitespace		= 0x020,
		RightToLeft			= 0x040,
		ECMAScript			= 0x100,
		CultureInvariant		= 0x200 
	}
}
