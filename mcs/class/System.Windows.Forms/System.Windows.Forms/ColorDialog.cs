//
// System.Windows.Forms.ColorDialog.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) Ximian, Inc., 2002
//

using System.Drawing;

namespace System.Windows.Forms {

	/// <summary>
	/// Represents a common dialog box that displays available colors along with controls that allow the user to define custom colors.
	///
	/// ToDo note:
	///  - nothing is implemented
	/// </summary>

	[MonoTODO]
	public class ColorDialog : CommonDialog {

		// private fields
		//private bool allowFullOpen;
		//private bool anyColor;
		//private Color color;
		//private int[] customColors;
		//private bool fullOpen;
		//private bool showHelp;
		//private bool solidColorOnly;

		/// --- Constructor ---
		protected ColorDialog() : base() 
		{
			//allowFullOpen = true;
			//anyColor = false;
			//color = Color.Black;
			//customColors = null;
			//fullOpen = false;
			//showHelp = false;
			//solidColorOnly = false;
		}
		
		
		
		
		/// --- Properties ---
		/// following properties are not stubbed out, because they only support .NET framework
		/// - protected virtual IntPtr Instance {get;}
		/// - protected virtual int Options {get;}
		public virtual bool AllowFullOpen {
			get {
				throw new NotImplementedException ();
				//return allowFullOpen;
			}
			set {
				throw new NotImplementedException ();
				//allowFullOpen=value;
			}
		}
		
		public virtual bool AnyColor {
			get {
				throw new NotImplementedException ();
				//return anyColor;
			}
			set {
				throw new NotImplementedException ();
				//anyColor=value;
			}
		}
		
		public Color Color {
			get {
				throw new NotImplementedException ();
				//return color;
			}
			set {
				throw new NotImplementedException ();
				//color=value;
			}
		}
		
		public int[] CustomColors {
			get {
				throw new NotImplementedException ();
				//return customColors;
			}
			set {
				throw new NotImplementedException ();
				//customColors=value;
			}
		}
		
		public virtual bool FullOpen {
			get {
				throw new NotImplementedException ();
				//return fullOpen;
			}
			set {
				throw new NotImplementedException ();
				//fullOpen=value;
			}
		}
		
		public virtual bool ShowHelp {
			get {
				throw new NotImplementedException ();
				//return showHelp;
			}
			set {
				throw new NotImplementedException ();
				//showHelp=value;
			}
		}
		
		public virtual bool SolidColorOnly {
			get {
				throw new NotImplementedException ();
				//return solidColorOnly;
			}
			set {
				throw new NotImplementedException ();
				//solidColorOnly=value;
			}
		}

		/// --- ColorDialog methods ---
		[MonoTODO]
		public override void Reset() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override bool RunDialog(IntPtr hwndOwner) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override string ToString() 
		{
			throw new NotImplementedException ();
		}
		
	}
}
