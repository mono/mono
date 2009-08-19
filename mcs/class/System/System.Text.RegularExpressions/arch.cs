//
// assembly:	System
// namespace:	System.Text.RegularExpressions
// file:	arch.cs
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

using System;
using System.Collections;

namespace System.Text.RegularExpressions {

	enum OpCode : ushort {
		False		= 0,	// always fails
		True,			// always succeeds

		// matching

		Position,		// zero-width position assertion
		String,			// match string literal
		Reference,		// back reference

		// character matching

		Character,		// match character exactly
		Category,		// match character from category
		NotCategory,		// match character _not_ from category
		Range,			// match character from range
		Set,			// match character from set
		In,			// match character from group of tests

		// capturing

		Open,			// open group
		Close,			// close group
		Balance,		// balance groups
		BalanceStart,           //track balance group length

		// control flow

		IfDefined,		// conditional on capture
		Sub,			// non-backtracking subexpression
		Test,			// non-backtracking lookahead/behind
		Branch,			// alternative expression
		Jump,			// unconditional goto
		Repeat,			// new repeat context
		Until,			// repeat subexpression within context
		FastRepeat,		// repeat simple subexpression
		Anchor,			// anchoring expression

		// miscellaneous
		
		Info			// pattern information
	}

	[Flags]
	enum OpFlags : ushort {
		None		= 0x000,
		Negate		= 0x100,	// succeed on mismatch
		IgnoreCase	= 0x200,	// case insensitive matching
		RightToLeft	= 0x400,	// right-to-left matching
		Lazy		= 0x800		// minimizing repeat
	}

	enum Position : ushort {
		Any,			// anywhere
		Start,			// start of string			\A
		StartOfString,		// start of string			\A
		StartOfLine,		// start of line			^
		StartOfScan,		// start of scan			\G
		End,			// end or before newline at end		\Z
		EndOfString,		// end of string			\z
		EndOfLine,		// end of line				$
		Boundary,		// word boundary			\b
		NonBoundary		// not word boundary			\B
	};
	
	// see category.cs for Category enum

	interface IMachine {
		Match Scan (Regex regex, string text, int start, int end);
		string [] Split (Regex regex, string input, int count, int startat);
		string Replace (Regex regex, string input, string replacement, int count, int startat);
		string Result (string replacement, Match match);
	}

	interface IMachineFactory {
		IMachine NewInstance ();
		IDictionary Mapping { get; set; }
		int GroupCount { get; }
		int Gap { get; set; } // Index of first group whose number differs from its index, or 1+GroupCount
		string [] NamesMapping { get; set; }
	}

	// Anchor SKIP OFFSET
	//
	// Flags:	[RightToLeft] ??
	// SKIP:	relative address of tail expression
	// OFFSET:	offset of anchor from start of pattern
	//
	// Usage:
	//
	// 	Anchor :1 OFFSET
	//		<expr>
	//		True
	// 1:	<tail>
	//
	// Notes:
	//
	// In practice, the anchoring expression is only going to be
	// Position (StartOfString, StartOfLine, StartOfScan) or String.
	// This is because the optimizer looks for position anchors at the
	// start of the expression, and if that fails it looks for the
	// longest substring. If an expression has neither a position
	// anchor or a longest substring anchor, then the anchoring expression
	// is left empty. Since an empty expression will anchor at any
	// position in any string, the entire input string will be scanned.

	// String LEN STR...
	//
	// Flags:	[RightToLeft, IgnoreCase]
	// LEN:		length of string
	// STR:		string characters

	// Branch SKIP
	//
	// SKIP:	relative address of next branch
	//
	//	Branch :1
	//		<alt expr 1>
	//		Jump :4
	// 1:	Branch :2
	//		<alt expr 2>
	//		Jump :4
	// 2:	Branch :3
	//		<alt expr 3>
	//		Jump :4
	// 3:	False
	// 4:	<tail>

	// Repeat SKIP MIN MAX
	//
	// Flags:	[Lazy]
	// SKIP:	relative address of Until instruction
	// MIN:		minimum iterations (2 slots)
	// MAX:		maximum iterations (2 slots, 0x7fffffff is infinity)
	//
	//	Repeat :1 MIN MAX
	//		<expr>
	//		Until
	// 1:	<tail>

	// FastRepeat SKIP MIN MAX
	//
	// Flags:	[Lazy]
	// SKIP:	relative address of tail expression
	// MIN:		minimum iterations (2 slots)
	// MAX:		maximum iterations (2 slots, 0x7fffffff is infinity)
	//
	//	FastRepeat :1 MIN MAX
	//		<expr>
	//		True
	// 1:	<tail>
	//
	// Notes:
	//
	// The subexpression of a FastRepeat construct must not contain any
	// complex operators. These include: Open, Close, Balance, Repeat,
	// FastRepeat, Sub, Test. In addition, the subexpression must have
	// been determined to have a fixed width.
	
	// Sub SKIP
	//
	// SKIP:	relative address of tail expression
	//
	//	Sub :1
	//		<expr>
	// 1:	<tail>
	//
	// Notes:
	//
	// The Sub operator invokes an independent subexpression. This means
	// that the subexpression will match only once and so will not
	// participate in any backtracking.

	// Test TSKIP FSKIP
	//
	// TSKIP:	relative address of true expression
	// FSKIP:	relative address of false expression
	//
	// Usage:	(?(?=test)true|false)
	//
	//	Test :1 :2
	//		<test expr>
	// 1:		<true expr>
	//		Jump
	// 2:		<false epxr>
	//	<tail>
	//
	// Usage:	(?(?=test)true)
	//
	//	Test :1 :2
	//		<test expr>
	// 1:		<true expr>
	// 2:	<tail>
	//
	// Usage:	(?=test)
	//
	//	Test :1 :2
	//		<test expr>
	// 1:		<true expr>
	//		Jump 3:
	// 2:		False
	// 3:		<tail>
	//
	// Notes:
	//
	// For negative lookaheads, just swap the values of TSKIP and
	// FSKIP. For lookbehinds, the test expression must be compiled
	// in reverse. The test expression is always executed as an
	// independent subexpression, so its behaviour is non-backtracking
	// (like a Sub clause.)

	// IfDefined SKIP GID
	//
	// SKIP:	relative address of else expression
	// GID:		number of group to check
	//
	// Usage:	(?(gid)true)
	//
	//	IfDefined :1
	//		<true expr>
	// 1:	<tail>
	//
	// Usage:	(?(gid)true|false)
	//
	//	IfDefined :1
	//		<true expr>
	//		Jump :2
	// 1:		<false expr>
	// 2:	<tail>

	// Jump SKIP
	//
	// SKIP:	relative address of target expression
	//
	//	Jump :1
	//	...
	// :1	<target expr>

	// Character CHAR
	//
	// Flags:	[Negate, IgnoreCase, RightToLeft]
	// CHAR:	exact character to match

	// Category CAT
	//
	// Flags:	[Negate, RightToLeft]
	// CAT:		category to match (see Category enum)

	// Range LO HI
	//
	// Flags:	[Negate, IgnoreCase, RightToLeft]
	// LO:		lowest character in range
	// HI:		higest character in range

	// Set LO LEN SET...
	//
	// Flags:	[Negate, IgnoreCase, RightToLeft]
	// LO:		lowest character in set
	// LEN:		number of words in set
	// SET:		bit array representing characters in set
	//
	// Notes:
	//
	// Each word in the set represents 16 characters, so the first word
	// defines membership for characters LO to LO + 15, the second for
	// LO + 16 to LO + 31, and so on up to LO + (LEN * 16 - 1). It is
	// up to the compiler to provide a compact representation for sparse
	// unicode sets. The simple way is to use Set 0 4096. Other methods
	// involve paritioning the set and placing the components into an
	// In block.

	// In SKIP
	//
	// SKIP:	relative address of tail expression
	//
	// Usage:	[expr]
	//
	//	In :1
	//		<expr>
	//		True
	// :1	<tail>
	//
	// Usage:	[^expr]
	//
	//	In :1
	//		<expr>
	//		False
	// :1	<tail>
	//
	// Notes:
	//
	// The In instruction consumes a single character, using the flags
	// of the first instruction in the subexpression to determine its
	// IgnoreCase and RightToLeft properties. The subexpression is then
	// applied to the single character as a disjunction. If any instruction
	// in the subexpression succeeds, the entire In construct succeeds
	// and matching continues with the tail.

	// Position POS
	//
	// POS:		position to match (see Position enum)

	// Open GID
	//
	// GID:		number of group to open

	// Close GID
	//
	// GID:		number of group to close
	
	// Balance GID BAL
	//
	// GID:		number of capturing group (0 if none)
	// BAL:		number of group to undefine

	// Info GROUPS MIN MAX
	//
	// GROUPS:	number of capturing groups (2 slots)
	// MIN:		minimum width of pattern (2 slots)
	// MAX:		maximum width of pattern (2 slots, 0x7fffffff means undefined)

	// False

	// True

	// Reference GID
	//
	// Flags:	[IgnoreCase, RightToLeft]
	// GID:		number of group to reference
}
