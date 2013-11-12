//
// System.ConsoleKeyInfo.cs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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
namespace System {
	[Serializable]
	public struct ConsoleKeyInfo {
		internal static ConsoleKeyInfo Empty = new ConsoleKeyInfo ('\0', 0, false, false, false);
		ConsoleKey _key;
		char _keyChar;
		ConsoleModifiers _mods;

		public ConsoleKeyInfo (char keyChar, ConsoleKey key, bool shift, bool alt, bool control)
		{
			_key = key;
			_keyChar = keyChar;
			_mods = 0;
			SetModifiers (shift, alt, control);
		}

		internal ConsoleKeyInfo (ConsoleKeyInfo other)
		{
			_key = other._key;
			_keyChar = other._keyChar;
			_mods = other._mods;
		}

		internal void SetKey (ConsoleKey key)
		{
			_key = key;
		}

		internal void SetKeyChar (char keyChar)
		{
			_keyChar = keyChar;
		}

		internal void SetModifiers (bool shift, bool alt, bool control)
		{
			_mods = (shift) ? ConsoleModifiers.Shift : 0;
			_mods |= (alt) ? ConsoleModifiers.Alt : 0;
			_mods |= (control) ? ConsoleModifiers.Control : 0;
		}

		public ConsoleKey Key 
		{
			get { return _key; }
		}

		public char KeyChar 
		{
			get { return _keyChar; }
		}

		public ConsoleModifiers Modifiers 
		{
			get { return _mods; }
		}

		public override bool Equals (object value)
		{
			if (!(value is ConsoleKeyInfo))
				return false;

			return Equals ((ConsoleKeyInfo) value);
		}

		public static bool operator == (ConsoleKeyInfo a, ConsoleKeyInfo b)
		{
			return a.Equals (b);
		}

		public static bool operator != (ConsoleKeyInfo a, ConsoleKeyInfo b)
		{
			return !a.Equals (b);
		}

		public bool Equals (ConsoleKeyInfo obj)
		{
			return _key == obj._key && _keyChar == obj._keyChar && _mods == obj._mods;
		}

		public override int GetHashCode ()
		{
			return _key.GetHashCode () ^ _keyChar.GetHashCode () ^ _mods.GetHashCode ();
		}
	}
}
