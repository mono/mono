//
// System.ComponentModel.DefaultPropertyAttribute
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// (C) 2003 Andreas Nahr
//

using System;

namespace System.ComponentModel
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class DefaultPropertyAttribute : Attribute
	{
		private string property_name;

		public static readonly DefaultPropertyAttribute Default = new DefaultPropertyAttribute (null);

		public DefaultPropertyAttribute (string name)
		{
			property_name = name;
		}

		public string Name
		{
			get { return property_name; }
		}

		public override bool Equals (object o)
		{
			if (!(o is DefaultPropertyAttribute))
				return false;

			return (((DefaultPropertyAttribute) o).Name == property_name);
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}
	}
}

