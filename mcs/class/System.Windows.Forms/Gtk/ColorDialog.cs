//
// System.Windows.Forms.ColorDialog.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//   Aleksey Ryabchuk (ryabchuk@yahoo.com)
//
// (C) Ximian, Inc., 2002
//

using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {

	/// <summary>
	/// Represents a common dialog box that displays available colors along with controls that allow the user to define custom colors.
	/// </summary>
	[MonoTODO]
	public class ColorDialog : CommonDialog {

		private bool allowFullOpen;
		private bool anyColor;
		private Color color;
		private int[] customColors;
		private bool fullOpen;
		private bool showHelp;
		private bool solidColorOnly;
		[MonoTODO]
		public ColorDialog() : base(){
			Reset ( );
		}

		// these are show to be in the ms implmentation by winchurn
		// but are not valid since they are not abstract or have any accessors.
		//protected virtual IntPtr Instance {		}
		//protected virtual int Options {		}
		[MonoTODO]
		public virtual bool AllowFullOpen {
			get {return allowFullOpen;}
			set {allowFullOpen = value;}
		}
		[MonoTODO]
		public virtual bool AnyColor {
			get {return anyColor;}
			set {anyColor = value;}
		}
		[MonoTODO]
		public Color Color {
			get {return color;}
			set {color = value;}
		}
		[MonoTODO]
		public int[] CustomColors {
			get {return customColors;}
			set {
				if ( value != null ) {
					for ( int i = 0; i < Math.Min ( customColors.Length, value.Length ); i++ )
						customColors [ i ] = value [ i ];
				}
			}
		}
		[MonoTODO]
		public virtual bool FullOpen {
			get {return fullOpen;}
			set {fullOpen = value;}
		}
		[MonoTODO]
		public virtual bool ShowHelp {
			get {return showHelp;}
			set {showHelp = value;}
		}
		[MonoTODO]
		public virtual bool SolidColorOnly {
			get {return solidColorOnly;}
			set {solidColorOnly = value;}
		}

		public override void Reset() 
		{
			/*allowFullOpen = true;
			anyColor = false;
			color = Color.Black;

			customColors = new int [ 16 ];
			for ( int i = 0; i < customColors.Length; i++ )
				customColors [ i ] = Win32.RGB ( Color.White );

			fullOpen = false;
			showHelp = false;
			solidColorOnly = false;*/
		}
		[MonoTODO]
		protected override bool RunDialog( IntPtr hwndOwner ){
			Dialog.Run();
			return false;			
		}
		[MonoTODO]
		public override string ToString() {
			return GetType( ).FullName.ToString ( ) + ", Color: " + Color.ToString ( );
		}
		[MonoTODO]
		internal override Gtk.Dialog CreateDialog (){
			return new Gtk.ColorSelectionDialog("ColorSelectionDialog");
		}
	}
}
