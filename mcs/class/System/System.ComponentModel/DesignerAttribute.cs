//
// System.ComponentModel.DesignerAttribute.cs
//
// Author:
//   Alejandro Sánchez Acosta (raciel@es.gnu.org)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Alejandro Sánchez Acosta
// (C) 2003 Andreas Nahr
//

namespace System.ComponentModel 
{

	/// <summary>
	///   Designer Attribute for classes. 
	/// </summary>
	
	/// <remarks>
	/// </remarks>
	
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
	public sealed class DesignerAttribute : Attribute
	{
		private string name;
		private string basetypename;
			
		public DesignerAttribute (string designerTypeName)
		{
			name = designerTypeName;
		}

		public DesignerAttribute (Type designerType)
			: this (designerType.AssemblyQualifiedName)
		{
		}

		public DesignerAttribute (string designerTypeName, Type designerBaseType)
			: this (designerTypeName, designerBaseType.AssemblyQualifiedName)
		{
		}

		public DesignerAttribute (Type designerType, Type designerBaseType)
			: this (designerType.AssemblyQualifiedName, designerBaseType.AssemblyQualifiedName)
		{
		}

		public DesignerAttribute (string designerTypeName, string designerBaseTypeName)
		{
			name = designerTypeName;
			basetypename = designerBaseTypeName;
        	}

		public string DesignerBaseTypeName {
            		get {
				return basetypename;
 			}
		}

		public string DesignerTypeName {
			get {
				return name;
			}
		}

		public override object TypeId {
			get {
				return this.GetType ();
			}
		}
			
		public override bool Equals (object obj)
		{
			if (!(obj is DesignerAttribute))
				return false;
			return ((DesignerAttribute) obj).DesignerBaseTypeName.Equals (basetypename) && 
				((DesignerAttribute) obj).DesignerTypeName.Equals (name);
		}				

		public override int GetHashCode ()
		{
			return string.Concat(name, basetypename).GetHashCode ();
		}
	}
}
