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
	}
}
		
