//
// System.ComponentModel.DesignerAttribute.cs
//
// Author:
//   Alejandro Sánchez Acosta (raciel@es.gnu.org)
//
// (C) Alejandro Sánchez Acosta
//

namespace System.ComponentModel {

	/// <summary>
	///   Designer Attribute for classes. 
	/// </summary>
	
	/// <remarks>
	/// </remarks>
	
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
		public sealed class DesignerAttribute : Attribute
		{
			string name;
			string basetypename;
			Type type;
			Type basetype;
			
			public DesignerAttribute (string designerTypeName)
			{
				name  = designerTypeName;
			}

			public DesignerAttribute(Type designerType)
			{
				type = designerType;
			}

			public DesignerAttribute(string designerTypeName, string designerBaseTypeName)
			{
				name = designerTypeName;
				basetypename = designerBaseTypeName;
			}

			public DesignerAttribute(string designerTypeName, Type designerBaseType)
			{
				name = designerTypeName;
				basetype = designerBaseType;
			}

			public DesignerAttribute(Type designerType, Type designerBaseType)
			{
				type = designerType;
				basetype = designerBaseType;
			}

			public string DesignerBaseTypeName 
			{
				get
				{
					return basetypename;
				}
			}

			public string DesignerTypeName 
			{
				get
				{
					return name;
				}
			}

			public override object TypeId 
			{
				get 
				{
					return this.GetType();
				}
			}
			
			public override bool Equals(object obj)
			{
	                        if (!(obj is DesignerAttribute))
	                                return false;
	                        return (((DesignerAttribute) obj).name == name) && 
					(((DesignerAttribute) obj).basetype == basetype) &&
					(((DesignerAttribute) obj).type == type) &&
					(((DesignerAttribute) obj).basetypename == basetypename);
			}				

			public override int GetHashCode()
			{
				return base.GetHashCode ();
			}
		}
}
