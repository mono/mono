//
// System.ComponentModel.MemberDescriptor.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.ComponentModel {

	public class MemberDescriptor {
		string name;
		Attribute [] attrs;
		
		protected MemberDescriptor (string name, Attribute [] attrs)
		{
			this.name = name;
			this.attrs = attrs;
		}

		protected MemberDescriptor (MemberDescriptor reference, Attribute [] attrs)
		{
			name = reference.name;
			this.attrs = attrs;
		}

		protected MemberDescriptor (string name)
		{
			this.name = name;
		}

		protected MemberDescriptor (MemberDescriptor reference)
		{
			name = reference.name;
			attrs = reference.attrs;
		}

		protected virtual Attribute [] AttributeArray {
			get {
				return attrs;
			}

			set {
				attrs = value;
			}
		}

		// FIXME: Implement Attributes property

		public virtual string Category {
			get {
				foreach (Attribute attr in attrs){



					if (attr is CategoryAttribute){
						return ((CategoryAttribute) attr).Category;
					}
				}
				return "Misc";
			}
		}

		public virtual string Description {
			get {
				foreach (Attribute attr in attrs){
					if (attr is DescriptionAttribute)
						return ((DescriptionAttribute) attr).Description;
				}

				return "";
			}
		}

		public virtual bool DesignTimeOnly {
			get {
				foreach (Attribute attr in attrs){
					if (attr is DesignOnlyAttribute)
						return ((DesignOnlyAttribute) attr).IsDesignOnly;
				}

				return false;
			}
		}

		//
		// FIXME: Is there any difference between DisplayName and Name?
		//
		public virtual string DisplayName {
			get {
				return name;
			}
		}

		public virtual string Name {
			get {
				return name;
			}
		}

		public virtual bool IsBrowsable {
			get {
				foreach (Attribute attr in attrs){
					if (attr is BrowsableAttribute)
						return ((BrowsableAttribute) attr).Browsable;
				}

				return false;
			}
		}

		protected virtual int NameHashCode {
			get {
				return name.GetHashCode ();
			}
		}
	}
}
