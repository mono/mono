//
// System.Windows.Forms.DrawItemEventArgs
//
// Author:
//   stubbed out by Richard Baumann (biochem333@nyc.rr.com)
//   Implemented by Richard Baumann <biochem333@nyc.rr.com>
//   Dennis Hayes (dennish@Raytek.com)
//   Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) Ximian, Inc., 2002
//

using System;
using System.Drawing;

namespace System.Windows.Forms {

	/// <summary>
	///	Provides data for the DrawItem event.
	/// </summary>
	public class DrawItemEventArgs : EventArgs {

		#region Fields
		private Color backColor;
		private Rectangle bounds;
		private Font font;
		private Color foreColor;
		private Graphics graphics;
		private int index;
		private DrawItemState state;
		#endregion

		//
		//  --- Constructors/Destructors
		//
		public DrawItemEventArgs(Graphics graphics, Font font, Rectangle bounds, int index, DrawItemState state) : base()
		{
			this.graphics = graphics;
			this. font = font;
			this. bounds = bounds;
			this.index = index;
			this.state = state;
			foreColor = SystemColors.WindowText;
			backColor = SystemColors.Window;
			throw new NotImplementedException ();
		}
		public DrawItemEventArgs(Graphics graphics, Font font, Rectangle bounds, int index,
		                          DrawItemState state, Color foreColor, Color backColor) : base()
		{
			this.graphics = graphics;
			this. font = font;
			this. bounds = bounds;
			this.index = index;
			this.state = state;
			this.foreColor = foreColor;
			this.backColor = backColor;
		}

		#region Public Methods
		public virtual void DrawBackground()
		{
			SolidBrush temp = new SolidBrush(BackColor);
			graphics.FillRectangle(temp,bounds);
			temp.Dispose();
		}
		public virtual void DrawFocusRectangle()
		{
			if( (DrawItemState.Focus == (DrawItemState.Focus & state)) && // check for focus
			    (DrawItemState.NoFocusRect != (DrawItemState.NoFocusRect & state))){ // check if this matters {

				ControlPaint.DrawFocusRectangle(graphics,bounds,foreColor,backColor);
			}
		}
		#endregion

		#region Public Properties
		public Color BackColor 
		{
			get { 
					return (DrawItemState.Selected == (state & DrawItemState.Selected)) ? SystemColors.Highlight : backColor; 
			}
		}
		public Rectangle Bounds {
			get { 
					return bounds; 
			}
		}
		public Font Font {
			get { 
					return font; 
			}
		}
		public Color ForeColor {
			get { 
					return (DrawItemState.Selected == (state & DrawItemState.Selected)) ? SystemColors.HighlightText : foreColor; 
			}
		}
		public Graphics Graphics {
			get { 
					return graphics; 
			}
		}
		public int Index {
			get { 
					return index; 
			}
		}
		public DrawItemState State {
			get { 
					return state; 
			}
		}
		#endregion

		#region Operators 


		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two DrawItemEventArgs objects.
		///	The return value is based on the equivalence of
		///	BackColor, Bounds, Font, ForeColor, Graphics, Index and State  Property
		///	of the two DrawItemEventArgs.
		/// </remarks>
		public static bool operator == (DrawItemEventArgs DrawItemEventArgsA, DrawItemEventArgs DrawItemEventArgsB) 
		{
			return (DrawItemEventArgsA.BackColor ==  DrawItemEventArgsB.BackColor) && 
				   (DrawItemEventArgsA.Bounds ==  DrawItemEventArgsB.Bounds) && 
				   (DrawItemEventArgsA.Font ==  DrawItemEventArgsB.Font) && 
				   (DrawItemEventArgsA.ForeColor ==  DrawItemEventArgsB.ForeColor) && 
				   (DrawItemEventArgsA.Graphics ==  DrawItemEventArgsB.Graphics) && 
				   (DrawItemEventArgsA.Index ==  DrawItemEventArgsB.Index) && 
				   (DrawItemEventArgsA.State ==  DrawItemEventArgsB.State);

		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two DrawItemEventArgs objects.
		///	The return value is based on the equivalence of
		///	BackColor, Bounds, Font, ForeColor, Graphics, Index and State  Property
		///	of the two DrawItemEventArgs.
		/// </remarks>
		public static bool operator != (DrawItemEventArgs DrawItemEventArgsA, DrawItemEventArgs DrawItemEventArgsB) 
		{
			return (DrawItemEventArgsA.BackColor !=  DrawItemEventArgsB.BackColor) || 
				(DrawItemEventArgsA.Bounds !=  DrawItemEventArgsB.Bounds) || 
				(DrawItemEventArgsA.Font !=  DrawItemEventArgsB.Font) || 
				(DrawItemEventArgsA.ForeColor !=  DrawItemEventArgsB.ForeColor) || 
				(DrawItemEventArgsA.Graphics !=  DrawItemEventArgsB.Graphics) || 
				(DrawItemEventArgsA.Index !=  DrawItemEventArgsB.Index) || 
				(DrawItemEventArgsA.State !=  DrawItemEventArgsB.State);

		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this
		///	DrawItemEventArgs and another
		///	object.
		/// </remarks>
		public override bool Equals (object obj) 
		{
			if (!(obj is DrawItemEventArgs))return false;
			return (this == (DrawItemEventArgs) obj);
		}

		/// <summary>
		///	GetHashCode Method
		/// </summary>
		///
		/// <remarks>
		///	Calculates a hashing value.
		/// </remarks>
		[MonoTODO]
		public override int GetHashCode () 
		{
			//FIXME: add class specific stuff;
			return base.GetHashCode();
		}

		/// <summary>
		///	ToString Method
		/// </summary>
		///
		/// <remarks>
		///	Formats the object as a string.
		/// </remarks>
		[MonoTODO]
		public override string ToString () 
		{
			//FIXME: add class specific stuff;
			return base.ToString() + " DrawItemEventArgs";
		}

		#endregion

	}
}
