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
	/// <summary>
	/// Summary description for CharacterRange.
	/// </summary>
	public struct CharacterRange
	{
		private int first;
		private int length;
		public CharacterRange(int First, int Length){
			first = First;
			length = Length;
		}
		public int First{
			get{
				return first;
			}
			set{
				first = value;
			}
		}
		public int Length{
			get{
				return length;
			}
			set{
				length = value;
			}
		}
	}
}
