//
// System.Windows.Forms.MeasureItemEventArgs.cs
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//	Partially completed by Dennis Hayes (dennish@raytek.com)
//	Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) 2002 Ximian, Inc
//

using System;
using System.Reflection;
using System.Globalization;
//using System.Windows.Forms.AccessibleObject.IAccessible;
using System.Drawing;

namespace System.Windows.Forms  {


	/// <summary>
	/// </summary>

	public class MeasureItemEventArgs : EventArgs {

		#region Fields

		private Graphics graphics;
		private int index;
		private int itemheight = -1;
		private int itemwidth = -1;

		#endregion

		//
		//  --- Constructors
		//
		public MeasureItemEventArgs(Graphics graphics, int index)
		{
			this.index = index;
			this.graphics = graphics;
		}

		public MeasureItemEventArgs(Graphics graphics, int index, int itemheight) 
		{
			this.index = index;
			this.graphics = graphics;
			itemheight = ItemHeight;
		}

		#region Public Properties

		public Graphics Graphics  
		{
			get 
			{ 
				return graphics;
			}
		}

		public int Index  
		{
			get 
			{
				return index;
			}
		}

		public int ItemHeight  
		{
			get 
			{
				return itemheight;
			}
			set 
			{
				itemheight = value;
			}
		}

		public int ItemWidth  
		{
			get 
			{
				return itemwidth;
			}
			set 
			{
				itemwidth = value;
			}
		}

		#endregion
		
		#region Public Methods

		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two MeasureItemEventArgs objects.
		///	The return value is based on the equivalence of
		///	graphics, index, itemheight and itemwidth Property
		///	of the two MeasureItemEventArgs.
		/// </remarks>
		public static bool operator == (MeasureItemEventArgs MeasureItemEventArgsA, MeasureItemEventArgs MeasureItemEventArgsB) 
		{
			return (MeasureItemEventArgsA.Graphics == MeasureItemEventArgsB.Graphics) && 
				   (MeasureItemEventArgsA.Index == MeasureItemEventArgsB.Index) &&
				   (MeasureItemEventArgsA.ItemHeight == MeasureItemEventArgsB.ItemHeight) &&
				   (MeasureItemEventArgsA.ItemWidth == MeasureItemEventArgsB.ItemWidth);
		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two MeasureItemEventArgs objects.
		///	The return value is based on the equivalence of
		///	graphics, index, itemheight and itemwidth Property
		///	of the two MeasureItemEventArgs.
		/// </remarks>
		public static bool operator != (MeasureItemEventArgs MeasureItemEventArgsA, MeasureItemEventArgs MeasureItemEventArgsB) 
		{
			return (MeasureItemEventArgsA.Graphics != MeasureItemEventArgsB.Graphics) || 
				   (MeasureItemEventArgsA.Index != MeasureItemEventArgsB.Index) ||
				   (MeasureItemEventArgsA.ItemHeight != MeasureItemEventArgsB.ItemHeight) ||
				   (MeasureItemEventArgsA.ItemWidth != MeasureItemEventArgsB.ItemWidth);
		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this
		///	PropertyTabChangedEventArgs and another
		///	object.
		/// </remarks>
		public override bool Equals (object obj) 
		{
			if (!(obj is MeasureItemEventArgs))return false;
			return (this == (MeasureItemEventArgs) obj);
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

	}
}
		
