//
// System.Drawing.LinkArea.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
//
// (C) 2002/3 Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Windows.Forms {
	[Serializable]
	public struct LinkArea { 

		private int start;
		private int length;

		// -----------------------
		// Public Constructor
		// -----------------------

		/// <summary>
		/// 
		/// </summary>
		///
		/// <remarks>
		///
		/// </remarks>
		
		public LinkArea (int Start, int Length)
		{
			start = Start;
			length = Length;
		}

		// -----------------------
		// Public Instance Members
		// -----------------------

		public bool IsEmpty {
			get{
				// Start can be 0, so no way to know if it is empty.
				// Docs seem to say Start must/should be set before
				// length, os if length is valid, start must also be ok.
				return length!=0;
			}
		}

		[MonoTODO]
		public override bool Equals(object o){
			return base.Equals(o) ;
		}

		[MonoTODO]
		public override int GetHashCode(){
			return base.GetHashCode() ;
		}

		public int Start {
			get{
				return start;
			}
			set{
				start = value;
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
