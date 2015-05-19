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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Jackson Harper (jackson@ximian.com)
//
//

using System;
using System.Resources;

namespace System.Windows.Forms {

	internal class KeyboardLayouts {

		private KeyboardLayout [] keyboard_layouts;
		public int [][] vkey_table;
		public short [][] scan_table;

		public void LoadLayouts ()
		{
			ResourceManager	rm;
			rm = new ResourceManager ("keyboards", System.Reflection.Assembly.GetExecutingAssembly());
			keyboard_layouts = (KeyboardLayout []) rm.GetObject ("keyboard_table");

			vkey_table = (int [][]) rm.GetObject ("vkey_table");
			scan_table = (short [][]) rm.GetObject ("scan_table");
		}

		public KeyboardLayout [] Layouts {
			get {
				if (keyboard_layouts == null)
					LoadLayouts ();
				return keyboard_layouts;
			}
		}
	}



	[Serializable]
#if GENERATING_RESOURCES
	[CLSCompliant(false)]
	public
#else 
	internal
#endif
		class KeyboardLayout {

		public int Lcid;
		public string Name;
		public ScanTableIndex ScanIndex;
		public VKeyTableIndex VKeyIndex;
		public uint [][] Keys;

		public KeyboardLayout (int lcid, string name, ScanTableIndex scan_index,
				VKeyTableIndex vkey_index, uint [][] keys)
		{
			Lcid = lcid;
			Name = name;
			ScanIndex = scan_index;
			VKeyIndex = vkey_index;
			Keys = keys;
		}

		public KeyboardLayout (int lcid, string name, int scan_index,
				int vkey_index, uint [][] keys) : this (lcid, name, (ScanTableIndex) scan_index,
						(VKeyTableIndex) vkey_index, keys)
		{
		}
	}

#if GENERATING_RESOURCES
	public
#else 
	internal
#endif
	 enum VKeyTableIndex {
		Qwerty,
		Qwertz,
		Dvorak,
		Qwertz105,
		Azerty,
		QwertyV2,
		AbntQwerty,
		QwertyJp106,
		Vnc
	}

#if GENERATING_RESOURCES
	public
#else 
	internal
#endif
	 enum ScanTableIndex {
		Qwerty,
		Dvorak,
		AbntQwerty,
		QwertyJp106,
		Vnc
	}

}

