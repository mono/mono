//
// System.ComponentModel.EditorAttribute.cs
//
// Author:
//   Alejandro Sánchez Acosta (raciel@es.gnu.org)
//
// (C) Alejandro Sánchez Acosta
//

namespace System.ComponentModel {

	/// <summary>
	///   Editor Attribute for classes. 
	/// </summary>

	[AttributeUsage(AttributeTargets.All)]
	public sealed class EditorAttribute : Attribute {
		
		string name;	
		string basename;
		Type base;
		Type nametype;

		public EditorAttribute()
		{
			this.name = "";
		}

		public EditorAttribute(string typeName, string baseTypeName)
		{
			name = typeName;
			basename = baseTypeName;
		}

		public EditorAttribute(string typeName, Type baseType)
		{
			name = typeName;
			base = baseType;	
		}

		public EditorAttribute(Type type, Type baseType)
		{
			nametype = type;
			base = baseType;
		}

		public string EditorBaseTypeName {
			get
			{
				return basename;
			}
		}
		
		public string EditorTypeName {
			get
			{
				return name;
			}
		}

		public override object TypeId {
			get
			{
				return this.GetType();
			}
		}
		
		public override bool Equals(object obj)
		{
			if (!(o is EditorAttribute))
				return false;
			return (((EditorAttribute) o).name == name) &&
				(((EditorAttribute) o).basename == basename) &&
				(((EditorAttribute) o).base == base) &&
				(((EditorAttribute) o).nametype == nametype);
		}
		
		public override int GetHashCode()
		{
                        if (AttributeName == null)
	                        return 0;
                        return name.GetHashCode ();
		}
	}
}

