//
// System.Windows.Drawing.CharacterRange.cs
//
// Author:
//	Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc
//
using System;

namespace System.Drawing
{
	public struct CharacterRange
	{
		private int first;
		private int length;

		public CharacterRange (int first, int length)
		{
			this.first = first;
			this.length = length;
		}

		public int First {
			get{
				return first;
			}
			set{
				first = value;
			}
		}
		
		public int Length {
			get{
				return length;
			}
			set{
				length = value;
			}
		}
	}
}
