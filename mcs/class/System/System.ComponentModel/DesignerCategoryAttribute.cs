//
// System.ComponentModel.DesignerCategoryAttribute.cs
//
// Author:
//   Alan Tam Siu Lung (Tam@SiuLung.com)
//

namespace System.ComponentModel {

	/// <summary>
	///   Designer Attribute for classes.
	/// </summary>
	
	/// <remarks>
	/// </remarks>
	
	[AttributeUsage(AttributeTargets.Class)]
		public sealed class DesignerCategoryAttribute : Attribute
		{
			string category;
			public static readonly DesignerCategoryAttribute Component;
			public static readonly DesignerCategoryAttribute Form;
			public static readonly DesignerCategoryAttribute Generic;
			static readonly DesignerCategoryAttribute Default;
			
			public DesignerCategoryAttribute ()
			{
				this.category = "";
			}
			
			public DesignerCategoryAttribute (string category)
			{
				this.category = category;
			}
			
			public override object TypeId {
				get {
					return GetType ();
				}
			}
			
			public string Category {
				get {
					return category;
				}
			}
			
			public override bool Equals (object obj)
			{
				if (!(obj is DesignerCategoryAttribute))
					return false;
				if (obj == this)
					return true;
				return ((DesignerCategoryAttribute) obj).category == category;
			}
			
			public override int GetHashCode ()
			{
				return category.GetHashCode ();
			}
			
			public override bool IsDefaultAttribute ()
			{
				return category == DesignerCategoryAttribute.Default.Category; // FIXME
			}
		}
}
