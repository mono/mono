//
// System.NullConsoleDriver
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2006 Novell, Inc. (http://www.novell.com)
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
#if MONO_FEATURE_CONSOLE
using System.Runtime.InteropServices;
using System.Text;
namespace System {
	class NullConsoleDriver : IConsoleDriver
	{
		static readonly ConsoleKeyInfo EmptyConsoleKeyInfo = new ConsoleKeyInfo ('\0', 0, false, false, false);

		public ConsoleColor BackgroundColor {
			get { return ConsoleColor.Black; }
			set {
			}
		}

		public int BufferHeight {
			get { return 0; }
			set {}
		}

		public int BufferWidth {
			get { return 0; }
			set {}
		}

		public bool CapsLock {
			get { return false; }
		}

		public int CursorLeft {
			get { return 0; }
			set {}
		}

		public int CursorSize {
			get { return 0; }
			set { }
		}

		public int CursorTop {
			get { return 0; }
			set {}
		}

		public bool CursorVisible {
			get { return false; }
			set {}
		}

		public ConsoleColor ForegroundColor {
			get { return ConsoleColor.Black; }
			set {}
		}

		public bool KeyAvailable {
			get { return false; } // FIXME: throw?
		}

		public bool Initialized {
			get { return true; }
		}

		public int LargestWindowHeight {
			get { return 0; }
		}

		public int LargestWindowWidth {
			get { return 0; }
		}

		public bool NumberLock {
			get { return false; }
		}

		public string Title {
			get { return ""; }
			set {}
		}

		public bool TreatControlCAsInput {
			get { return false; }
			set {}
		}

		public int WindowHeight {
			get { return 0; }
			set {}
		}

		public int WindowLeft {
			get { return 0; }
			set {}
		}

		public int WindowTop {
			get { return 0; }
			set {}
		}

		public int WindowWidth {
			get { return 0; }
			set {}
		}

		public void Beep (int frequency, int duration)
		{
		}

		public void Clear ()
		{
		}

		public void MoveBufferArea (int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight,
					int targetLeft, int targetTop, Char sourceChar,
					ConsoleColor sourceForeColor, ConsoleColor sourceBackColor)
		{
		}

		public void Init ()
		{
		}

		public string ReadLine ()
		{
			return null;
		}

		public ConsoleKeyInfo ReadKey (bool intercept)
		{
			return EmptyConsoleKeyInfo;
		}

		public void ResetColor ()
		{
		}

		public void SetBufferSize (int width, int height)
		{
		}

		public void SetCursorPosition (int left, int top)
		{
		}

		public void SetWindowPosition (int left, int top)
		{
		}

		public void SetWindowSize (int width, int height)
		{
		}
	}
}
#endif

