//
// System.Drawing.ToolboxBitmapAttribute.cs
//
// Authors:
//   Dennis Hayes (dennish@Raytek.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Ximian, Inc
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
