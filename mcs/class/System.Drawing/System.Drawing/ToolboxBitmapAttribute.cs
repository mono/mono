//
// System.Drawing.ToolboxBitmapAttribute.cs
//
// Authors:
//   Dennis Hayes (dennish@Raytek.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// Copyright (C) 2002 Ximian, Inc. http://www.ximian.com
// Copyright (C) 2004 Novell, Inc. http://www.novell.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
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

using System;

namespace System.Drawing
{
	[AttributeUsage (AttributeTargets.Class)]
	public class ToolboxBitmapAttribute : Attribute
	{
		private Image smallImage;
		private Image bigImage;
		public static readonly ToolboxBitmapAttribute Default = new ToolboxBitmapAttribute();

		private ToolboxBitmapAttribute ()
		{
		}

		[MonoTODO ("implement")]
		public ToolboxBitmapAttribute (string imageFile)
		{
			//
			// TODO: Add constructor logic here
			//
		}
		
		[MonoTODO ("implement")]
		public ToolboxBitmapAttribute (Type t)
		{
			//
			// TODO: Add constructor logic here
			//
		}
		
		[MonoTODO ("implement")]
		public ToolboxBitmapAttribute (Type t, string name)
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public override bool Equals (object value)
		{
			if (!(value is ToolboxBitmapAttribute))
				return false;
			if (value == this)
				return true;
			return ((ToolboxBitmapAttribute) value).smallImage == this.smallImage;
		}

		public override int GetHashCode ()
		{
			return (smallImage.GetHashCode () ^ bigImage.GetHashCode ());
		}

		public Image GetImage (object component)
		{
			return GetImage (component.GetType(), null, false);
		}

		public Image GetImage (object component, bool large)
		{
			return GetImage (component.GetType(), null, large);
		}

		public Image GetImage (Type type)
		{
			return GetImage (type, null, false);
		}

		public Image GetImage (Type type, bool large)
		{
			return GetImage (type, null, large);
		}

		[MonoTODO ("implement")]
		public Image GetImage (Type type, string imgName, bool large)
		{
			return null;
		}

		[MonoTODO ("implement")]
		public static Image GetImageFromResource (Type t, string imageName, bool large)
		{
			return null;
		}
	}
}
