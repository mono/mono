//
// System.Windows.Forms.PaintEventArgs.cs
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//	Dennis Hayes (dennish@raytek.com)
//  Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) 2002 Ximian, Inc
//
using System.Drawing;

namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>

	public class PaintEventArgs : EventArgs, IDisposable {

		#region Fields

			private Graphics mgraphics;
			private Rectangle mclipRect;

		#endregion


		public PaintEventArgs(Graphics graphics, Rectangle clipRect )
		{
				this.mgraphics = graphics;
				this.mclipRect = clipRect;
		}

		
		#region Public Properties
		public Rectangle ClipRectangle 
		{
			get {
				return mclipRect;
			}
		}
		
		public Graphics Graphics {
			get {
				return mgraphics;
			}
		}
		#endregion

		#region Public Methods

		[MonoTODO]
		public void Dispose()
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two PaintEventArgs objects.
		///	The return value is based on the equivalence of
		///	Graphics and ClipRectangle Property
		///	of the two PaintEventArgs.
		/// </remarks>
		public static bool operator == (PaintEventArgs PaintEventArgsA, PaintEventArgs PaintEventArgsB) 
		{
			return (PaintEventArgsA.Graphics == PaintEventArgsB.Graphics) && (PaintEventArgsA.ClipRectangle == PaintEventArgsB.ClipRectangle);
		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two PaintEventArgs objects.
		///	The return value is based on the equivalence of
		///	Graphics and ClipRectangle Property
		///	of the two PaintEventArgs.
		/// </remarks>
		public static bool operator != (PaintEventArgs PaintEventArgsA, PaintEventArgs PaintEventArgsB) 
		{
			return (PaintEventArgsA.Graphics != PaintEventArgsB.Graphics) || (PaintEventArgsA.ClipRectangle != PaintEventArgsB.ClipRectangle);
		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this
		///	PaintEventArgs and another
		///	object.
		/// </remarks>
		public override bool Equals (object obj) 
		{
			if (!(obj is PaintEventArgs))return false;
			return (this == (PaintEventArgs) obj);
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
			return base.ToString();
		}

		#endregion

		#region Protected Methods

		[MonoTODO]
		protected virtual void Dispose(bool disposing)
		{
			throw new NotImplementedException ();
		}
		
		#endregion

	 }
}
