
using System;
using java.awt;
namespace System.Drawing.Drawing2D 
{
	/// <summary>
	/// Summary description for HatchBrush.
	/// </summary>
	public sealed class HatchBrush : Brush 
	{
		private HatchStyle _style;
		private Color _foreColor;
		private Color _backColor;

		[MonoTODO]
		public HatchBrush (HatchStyle hatchStyle, Color foreColor)
					: this (hatchStyle, foreColor, Color.Black)
		{
		}

		[MonoTODO]
		public HatchBrush(HatchStyle hatchStyle, Color foreColor, Color backColor)
		{
			_style = hatchStyle;
			_foreColor = foreColor;
			_backColor = backColor;
		}

		public Color BackgroundColor {
			get {
				return _backColor;
			}
		}

		public Color ForegroundColor {
			get {
				return _foreColor;
			}
		}

		public HatchStyle HatchStyle {
			get {
				return _style;
			}
		}

		public override object Clone ()
		{
			return new HatchBrush (_style, _foreColor, _backColor);
		}

		protected override Paint NativeObject {
			get {
				// FALLBACK: Solid color brush will be used
				return _foreColor.NativeObject;
			}
		}

	}
}
