//
// System.Windows.Forms.ColumnHeader.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) Ximian, Inc., 2002
//

using System.ComponentModel;

namespace System.Windows.Forms {

	/// <summary>
	/// Displays a single column header in a ListView control.
	/// </summary>

	[MonoTODO]
	public class ColumnHeader : Component, ICloneable {

		// private fields
		string text;
		HorizontalAlignment textAlign;
		int width;
		int index;
		private ListView container = null;
		int serial = 0;
		
		/// --- constructor ---		
		public ColumnHeader() : base () {
			text = null;
			textAlign = HorizontalAlignment.Left;
			width = -2;
			index = -1;//default to not in list
		}
		
		//
		//  --- Internal Methods for the implementation
		//
		internal int Serial {
			get { return serial; }
			set { serial = value; }
		}
		
		internal ListView Container {			
			set{container=value;}
		}		
		
		internal int CtrlIndex{			
			set{index=value;}
		}		
		
		//
		// --- Public Properties ---
		//		
		public int Index {
			get { return index; }
		}		
		
		public ListView ListView { 	//return parent control.		
			get { return container; }			
		}
		
		public string Text {
			get { return text; }
			set { text = value; }
		}
		
		
		public HorizontalAlignment TextAlign {
			get { return textAlign; }
			set { textAlign = value; }
		}		
		
		public int Width {
			get { return width; }
			set { width = value; }
		}
		
		/// --- Methods ---
		[MonoTODO]
		public object Clone() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void Dispose(bool disposing) 
		{
			base.Dispose(disposing);
		}
		
		public override string ToString() 
		{
			//FIXME: add class specific info to the string
			return "Column header " + text;
		}
	}
}
