// System.Drawing.Design.PaintvalueEventArgs.cs
// 
// Author:
//      Alejandro Sánchez Acosta  <raciel@es.gnu.org>
// 
// (C) Alejandro Sánchez Acosta
// 

using System.ComponentModel;

namespace System.Drawing.Design
{
	public class PaintValueEventArgs : EventArgs
	{
		private ITypeDescriptorContext context;
		private object value;
		private Graphics graphics;
		private Rectangle bounds;
		
		public PaintValueEventArgs(ITypeDescriptorContext context, object value, Graphics graphics, Rectangle bounds) {
			this.context = context;
			this.value = value;
			this.graphics = graphics;
			this.bounds = bounds;
		}

		public Rectangle Bounds 
		{
			get {
				return bounds;
			}
		}

		public ITypeDescriptorContext Context 
		{
			get {
				return context;
			}
		}

		public Graphics Graphics 
		{
			get {
				return graphics;
			}				
		}

		public object Value 
		{
			get {
				return value;
			}
		}
	}
}

