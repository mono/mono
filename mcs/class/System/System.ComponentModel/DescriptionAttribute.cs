//
// System.ComponentModel.DescriptionAttribute.cs
//
// Author:
//  Miguel de Icaza (miguel@ximian.com)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// (C) 2003 Andreas Nahr
//
//

namespace System.ComponentModel {

	[AttributeUsage (AttributeTargets.All)]
	public class DescriptionAttribute : Attribute {
		private string desc;

		public static readonly DescriptionAttribute Default = new DescriptionAttribute ();
			
		public DescriptionAttribute ()
		{
			desc = "";
		}

		public DescriptionAttribute (string name)
		{
			desc = name;
		}

		public virtual string Description {
			get {
				return DescriptionValue;
			}
		}

		//
		// Notice that the default Description implementation uses this by default
		//
		protected string DescriptionValue {
			get {
				return desc;
			}

			set {
				desc = value;
			}
		}

		public override bool Equals (object obj)
		{
			if (!(obj is DescriptionAttribute))
				return false;
			if (obj == this)
				return true;
			return ((DescriptionAttribute) obj).Description == desc;
		}
			
		public override int GetHashCode ()
		{
			return desc.GetHashCode ();
		}
	}
}

