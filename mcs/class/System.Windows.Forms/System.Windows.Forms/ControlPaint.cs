//
// System.Windows.Forms.ControlPaint.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) Ximian, Inc 2002
//


using System.Drawing;

namespace System.Windows.Forms {

	/// <summary>
	/// Provides methods used to paint common Windows controls and their elements.
	/// </summary>
	
	[MonoTODO]
  public sealed class ControlPaint {

		#region Properties
		[MonoTODO]
		public static Color ContrastControlDark {

			get { throw new NotImplementedException (); }
		}
		#endregion
		
		#region Methods
		/// following methods were not stubbed out, because they only support .NET framework:
		/// - public static IntPtr CreateHBitmap16Bit(Bitmap bitmap,Color background)
		/// - public static IntPtr CreateHBitmapColorMask(Bitmap bitmap,IntPtr monochromeMask);
		/// - public static IntPtr CreateHBitmapTransparencyMask(Bitmap bitmap);
		[MonoTODO]
		public static Color Dark(Color baseColor) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static Color Dark(Color baseColor,float percOfDarkDark) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static Color DarkDark(Color baseColor) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static void DrawBorder(
			Graphics graphics) {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static void DrawBorder(
			Graphics graphics,
			Rectangle bounds,
			Color leftColor,
			int leftWidth,
			ButtonBorderStyle leftStyle,
			Color topColor,
			int topWidth,
			ButtonBorderStyle topStyle,
			Color rightColor,
			int rightWidth,
			ButtonBorderStyle rightStyle,
			Color bottomColor,
			int bottomWidth,
			ButtonBorderStyle bottomStyle) {

			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static void DrawBorder3D(
			Graphics graphics,
			Rectangle rectangle) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawBorder3D(
			Graphics graphics,
			Rectangle rectangle,
			Border3DStyle Style) {
			//FIXME:

		}
		
		[MonoTODO]
		public static void DrawBorder3D(
			Graphics graphics,
			Rectangle rectangle,
			Border3DStyle Style,
			Border3DSide Sides) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawBorder3D(
			Graphics graphics, int x) {
			//FIXME:
		}
		//is this part of spec? I do not think so.
		//[MonoTODO]
		//public static void DrawBorder3D(
		//	Graphics graphics, int x) {
		//	throw new NotImplementedException ();
		//}
		
		[MonoTODO]
		public static void DrawBorder3D(
			Graphics graphics,
			int x,
			int y,
			int width,
			int height) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawBorder3D(
			Graphics graphics,
			int x,
			int y,
			int width,
			int height,
			Border3DStyle style) {
			//FIXME:
		}

		[MonoTODO]
		public static void DrawBorder3D(
			Graphics graphics,
			int x,
			int y,
			int width,
			int height,
			Border3DStyle style,
			Border3DSide sides) {
			//FIXME:
		}

		[MonoTODO]
		public static void DrawButton(
			Graphics graphics,
			Rectangle rectangle) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawButton(
			Graphics graphics,
			int x,
			int y,
			int width,
			int height,
			ButtonState state) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawCaptionButton(
			Graphics graphics,
			Rectangle rectangle) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawCaptionButton(
			Graphics graphics,
			int x,
			int y,
			int width,
			int height,
			CaptionButton button,
			ButtonState state) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawCheckBox(
			Graphics graphics,
			Rectangle rectangle) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawCheckBox(
			Graphics graphics,
			int x,
			int y,
			int width,
			int height,
			ButtonState state) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawComboButton(
			Graphics graphics,
			Rectangle rectangle) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawComboButton(
			Graphics graphics,
			int x,
			int y,
			int width,
			int height,
			ButtonState state) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawContainerGrabHandle(Graphics graphics,Rectangle bounds) 
		{
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawFocusRectangle(
			Graphics graphics,
			Rectangle rectangle) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawFocusRectangle(
			Graphics graphics,
			Rectangle rectangle,
			Color foreColor,
			Color backColor) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawGrabHandle(
			Graphics graphics,
			Rectangle rectangle,
			bool primary,
			bool enabled) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawGrid(
			Graphics graphics,
			Rectangle area,
			Size pixelsBetweenDots,
			Color backColor) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawImageDisabled(
			Graphics graphics,
			Image image,
			int x,
			int y,
			Color background) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawLockedFrame(
			Graphics graphics,
			Rectangle rectangle,
			bool primary) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawMenuGlyph(
			Graphics graphics,
			Rectangle rectangle,
			MenuGlyph glyph) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawMenuGlyph(
			Graphics graphics,
			int x,
			int y,
			int width,
			int height,
			MenuGlyph glyph) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawMixedCheckBox(
			Graphics graphics,
			Rectangle rectangle,
			ButtonState state) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawMixedCheckBox(
			Graphics graphics,
			int x,
			int y,
			int width,
			int height,
			ButtonState state) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawRadioButton(
			Graphics graphics,
			Rectangle rectangle,
			ButtonState state) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawRadioButton(
			Graphics graphics,
			int x,
			int y,
			int width,
			int height,
			ButtonState state) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawReversibleFrame(
			Rectangle rectangle,
			Color backColor) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawReversibleLine(
			Point start,
			Point end,
			Color backColor) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawScrollButton(
			Graphics graphics,
			Rectangle rectangle,
			ScrollButton button) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawScrollButton(
			Graphics graphics,
			int x,
			int y,
			int width,
			int height,
			ScrollButton button,
			ButtonState state) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawSelectionFrame(
			Graphics graphics,
			bool active,
			Rectangle outsideRect,
			Rectangle insideRect,
			Color backColor) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawSizeGrip(
			Graphics graphics,
			Color backColor) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawSizeGrip(
			Graphics graphics,
			Color backColor,
			int x,
			int y,
			int width,
			int height) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawStringDisabled(
			Graphics graphics,
			string s,
			Font font,
			Color color,
			RectangleF layoutRectangle,
			StringFormat format) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void FillReversibleRectangle(
			Rectangle rectangle,
			Color backColor) {
			//FIXME:
		}
		
		[MonoTODO]
		public static Color Light(Color baseColor) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static Color Light(Color baseColor,float percOfLightLight) 
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public static Color LightLight(Color baseColor) 
		{
			throw new NotImplementedException ();
		}
		#endregion
	}
}
