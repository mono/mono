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

	public class ColorDialog : CommonDialog {

		private bool allowFullOpen;
		private bool anyColor;
		private Color color;
		private int[] customColors;
		private bool fullOpen;
		private bool showHelp;
		private bool solidColorOnly;

		public ColorDialog() : base() 
		{
			Reset ( );
		}
		
		public virtual bool AllowFullOpen {
			get {
				return allowFullOpen;
			}
			set {
				allowFullOpen = value;
			}
		}
		
		public virtual bool AnyColor {
			get {
				return anyColor;
			}
			set {
				anyColor = value;
			}
		}
		
		public Color Color {
			get {
				return color;
			}
			set {
				color = value;
			}
		}
		
		public int[] CustomColors {
			get {
				return customColors;
			}
			set {
				if ( value != null ) {
					for ( int i = 0; i < Math.Min ( customColors.Length, value.Length ); i++ )
						customColors [ i ] = value [ i ];
				}
			}
		}
		
		public virtual bool FullOpen {
			get {
				return fullOpen;
			}
			set {
				fullOpen = value;
			}
		}
		
		public virtual bool ShowHelp {
			get {
				return showHelp;
			}
			set {
				showHelp = value;
			}
		}
		
		public virtual bool SolidColorOnly {
			get {
				return solidColorOnly;
			}
			set {
				solidColorOnly = value;
			}
		}

		public override void Reset() 
		{
			allowFullOpen = true;
			anyColor = false;
			color = Color.Black;

			customColors = new int [ 16 ];
			for ( int i = 0; i < customColors.Length; i++ )
				customColors [ i ] = Win32.RGB ( Color.White );

			fullOpen = false;
			showHelp = false;
			solidColorOnly = false;
		}
		
		protected override bool RunDialog( IntPtr hwndOwner ) 
		{
			CHOOSECOLOR cc = new CHOOSECOLOR (  );
			cc.hwndOwner = hwndOwner;
			cc.lStructSize  = ( uint ) Marshal.SizeOf( cc );
			cc.Flags = (int) ( ChooseColorFlags.CC_RGBINIT | ChooseColorFlags.CC_ENABLEHOOK );

			cc.lpfnHook = new Win32.FnHookProc ( this.HookProc );

			if ( AllowFullOpen ) {
				if ( FullOpen )
					cc.Flags |= (int) ChooseColorFlags.CC_FULLOPEN;
			}
			else cc.Flags |= (int) ChooseColorFlags.CC_PREVENTFULLOPEN;

			if ( AnyColor )
				cc.Flags |= (int) ChooseColorFlags.CC_ANYCOLOR;

			if ( ShowHelp )
				cc.Flags |= (int) ChooseColorFlags.CC_SHOWHELP;

			if ( SolidColorOnly )
				cc.Flags |= (int) ChooseColorFlags.CC_SOLIDCOLOR;

			cc.rgbResult = Win32.RGB ( Color );

			cc.lpCustColors = Marshal.AllocHGlobal ( Marshal.SizeOf( customColors[0] ) * customColors.Length );
			Marshal.Copy ( customColors, 0, cc.lpCustColors, customColors.Length );
			
			bool res = false;
			try {
				res = Win32.ChooseColor ( ref cc );

				if ( res ) {
					this.Color = Color.FromArgb ( cc.rgbResult );
					Marshal.Copy ( cc.lpCustColors, customColors, 0, customColors.Length );
				}
			}
			finally {
				Marshal.FreeHGlobal ( cc.lpCustColors );
			}

			return res;
		}
		
		public override string ToString() 
		{
			return GetType( ).FullName.ToString ( ) + ", Color: " + Color.ToString ( );
		}
	}
}
