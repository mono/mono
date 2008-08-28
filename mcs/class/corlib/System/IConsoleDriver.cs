//
// System.IConsoleDriver
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2005 Novell, Inc. (http://www.novell.com)
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
namespace System {
	interface IConsoleDriver {
		ConsoleColor BackgroundColor { get; set; }
		int BufferHeight { get; set; }
		int BufferWidth { get; set; }
		bool CapsLock { get; }
		int CursorLeft { get; set; } 
		int CursorSize { get; set; } 
		int CursorTop { get; set; }
		bool CursorVisible { get; set; }
		ConsoleColor ForegroundColor { get; set; }
		bool KeyAvailable { get; }
		bool Initialized { get; }
		int LargestWindowHeight { get; }
		int LargestWindowWidth { get; }
		bool NumberLock { get; }
		string Title { get; set; }
		bool TreatControlCAsInput { get; set; } 
		int WindowHeight { get; set; }
		int WindowLeft { get; set; }
		int WindowTop { get; set; }
		int WindowWidth { get; set; }

		void Init ();
		void Beep (int frequency, int duration);
		void Clear ();
		void MoveBufferArea (int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight,
					int targetLeft, int targetTop, Char sourceChar,
					ConsoleColor sourceForeColor, ConsoleColor sourceBackColor);

		ConsoleKeyInfo ReadKey (bool intercept);
		void ResetColor ();
		void SetBufferSize (int width, int height);
		void SetCursorPosition (int left, int top);
		void SetWindowPosition (int left, int top);
		void SetWindowSize (int width, int height);
		string ReadLine ();
	}
}
#endif

