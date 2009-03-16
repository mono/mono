//
// System.TermInfoBooleans
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2009 Novell, Inc (http://www.novell.com)
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
#if NET_2_0 || BOOTSTRAP_NET_2_0

// These values are taken from 'man 5 terminfo' and /usr/include/term.h.
// They are the indexes for the boolean capabilities in a terminfo file.
namespace System {
	class ControlCharacters {
		public const int Intr = 0;
		public const int Quit = 1;
		public const int Erase = 2;
		public const int Kill = 3;
		public const int EOF = 4;
		public const int Time = 5;
		public const int Min = 6;
		public const int SWTC = 7;
		public const int Start = 8;
		public const int Stop = 9;
		public const int Susp = 10;
		public const int EOL = 11;
		public const int Reprint = 12;
		public const int Discard = 13;
		public const int WErase = 14;
		public const int LNext = 15;
		public const int EOL2 = 16;
	}
}
#endif

