// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2004 Novell, Inc.
//
// Authors:	Marek Safar		(marek.safar@seznam.cz)
//

// COMPLETE

using System.Drawing;
using System.ComponentModel;

namespace System.Windows.Forms.Design
{
	public abstract class PropertyTab: IExtenderProvider
	{
		Bitmap bitmap;
		object[] components;

		protected PropertyTab () {}

		~PropertyTab ()
		{
			Dispose (false);
		}

		public virtual Bitmap Bitmap { 
			get {
				if (bitmap == null) {
					Type t = base.GetType();
					bitmap = new Bitmap (t, t.Name + ".bmp");
				}
				return bitmap;
			}
		}

		public virtual object[] Components { 
			get { return components; }
			set { components = value; }
		}

		public virtual string HelpKeyword { 
			get { return TabName; }
		}

		public abstract string TabName { get; }

		public virtual bool CanExtend (object extendee)
		{
			return true;
		}

		public virtual void Dispose()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing && bitmap != null) {
				bitmap.Dispose ();
				bitmap = null;
			}
		}

		public virtual PropertyDescriptor GetDefaultProperty (object component)
		{
			return TypeDescriptor.GetDefaultProperty(component);
		}

		public virtual PropertyDescriptorCollection GetProperties (object component)
		{
			return GetProperties (component, null);
		}

		public abstract PropertyDescriptorCollection GetProperties (object component, Attribute[] attributes);

		public virtual PropertyDescriptorCollection GetProperties (ITypeDescriptorContext context, object component, Attribute[] attributes)
		{
			return GetProperties (component, attributes);
		}
	}
}
