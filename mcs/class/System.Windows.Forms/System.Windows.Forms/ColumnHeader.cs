//
// System.Windows.Forms.ColumnHeader.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//	Dennis Hayes (dennish@raytek.com)
//
// (C) Ximian, Inc., 2002
//

using System.ComponentModel;

namespace System.Windows.Forms
{
	/// <summary>
	/// Displays a single column header in a ListView control.
	///
	/// ToDo note:
	///  - no methods are implemented
	/// </summary>

	[MonoTODO]
	public class ColumnHeader : Component, ICloneable
	{
		// private fields
		string text;
		HorizontalAlignment textAlign;
		
		/// --- constructor ---
		[MonoTODO]
		public ColumnHeader() : base () {
			text = null;
			textAlign = HorizontalAlignment.Left;
		}
		
		
		
		// --- Properties ---
		[MonoTODO]
		public int Index {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public ListView ListView {
			get { throw new NotImplementedException (); }
		}
		
		public string Text {
			get { return text; }
			set { text=value; }
		}
		
		public HorizontalAlignment TextAlign {
			get { return textAlign; }
			set { textAlign=value; }
		}
		
		[MonoTODO]
		public int Width {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		/// --- Methods ---
		[MonoTODO]
		public object Clone() {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void Dispose(bool disposing) {
			throw new NotImplementedException ();
		}
		
		public override string ToString() {
			//FIXME: add class specific info to the string
			return base.ToString();
		}
	}
}
