//
// System.Windows.Forms.MeasureItemEventArgs.cs
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//	Partially completed by Dennis Hayes (dennish@raytek.com)
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
		private Graphics graphics;
		private int index;
		private int itemheight;
		private int itemwidth;

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
			itenheight = ItemHeight;
		}
		
		//
		// -- Public Methods
		//

//		public virtual bool Equals(object o) 
//		{
//			throw new NotImplementedException();
//		}
//
//		public static bool Equals(object o, object o)
//		{
//			throw new NotImplementedException();
//		}
//
//		public virtual int GetHashCode()
//		{
//			throw new NotImplementedException();
//		}
//
//		public Type GetType()
//		{
//			throw new NotImplementedException();
//		}
//
//		public virtual string ToString()
//		{
//			throw new NotImplementedException();
//		}
//
//		//
//		// -- Protected Methods
//		//
//
//		~MeasureItemEventArgs() 
//		{
//			throw new NotImplementedException();
//		}
//
//		protected object MemberwiseClone() 
//		{
//			throw new NotImplementedException();
//		}

		//
		// -- Public Properties
		//

		public Graphics Graphics  {
			get { 
				return graphics;
			}
		}

		public int Index  {
			get {
				return index;
			}
		}

		public int ItemHeight  {
			get {
				return itemheight;
			}
			set {
				itemheight = value;
			}
		}

		public int ItemWidth  {
			get {
				return itemwidth;
			}
			set {
				itemwidth = value;
			}
		}
	}
}
		
