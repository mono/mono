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
	/// </summary>

	[MonoTODO]
	public class ColorDialog : CommonDialog {

		// private fields
		private bool allowFullOpen;
		private bool anyColor;
		private Color color;
		private int[] customColors;
		private bool fullOpen;
		private bool showHelp;
		private bool solidColorOnly;

		/// --- Constructor ---
		public ColorDialog() : base() 
		{
			allowFullOpen = true;
			anyColor = false;
			color = Color.Black;
			customColors = null;
			fullOpen = false;
			showHelp = false;
			solidColorOnly = false;
		}
		
		
		
		
		/// --- Properties ---
		/// following properties are not stubbed out, because they only support .NET framework
		/// - protected virtual IntPtr Instance {get;}
		/// - protected virtual int Options {get;}
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
				anyColor=value;
			}
		}
		
		public Color Color {
			get {
				return color;
			}
			set {
				color=value;
			}
		}
		
		public int[] CustomColors {
			get {
				return customColors;
			}
			set {
				customColors=value;
			}
		}
		
		public virtual bool FullOpen {
			get {
				return fullOpen;
			}
			set {
				fullOpen=value;
			}
		}
		
		public virtual bool ShowHelp {
			get {
				return showHelp;
			}
			set {
				showHelp=value;
			}
		}
		
		public virtual bool SolidColorOnly {
			get {
				return solidColorOnly;
			}
			set {
				solidColorOnly=value;
			}
		}

		/// --- ColorDialog methods ---
		[MonoTODO]
		public override void Reset() 
		{
			allowFullOpen = true;
			anyColor = false;
			color = Color.Black;
			customColors = null;
			fullOpen = false;
			showHelp = false;
			solidColorOnly = false;
		}
		
		[MonoTODO]
		protected override bool RunDialog(IntPtr hwndOwner) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		//FIXME: do a better tostring
		public override string ToString() 
		{
			return "Color Dialog " + color.ToString();
		}
		
	}
}
