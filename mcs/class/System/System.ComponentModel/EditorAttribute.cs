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

	[AttributeUsage (AttributeTargets.All)]
	public sealed class EditorAttribute : Attribute {
		
		string name;	
		string basename;
		Type baseType;
		Type nametype;

		public EditorAttribute ()
		{
			this.name = "";
		}

		public EditorAttribute (string typeName, string baseTypeName)
		{
			name = typeName;
			basename = baseTypeName;
		}

		public EditorAttribute (string typeName, Type baseType)
		{
			name = typeName;
			this.baseType = baseType;	
		}

		public EditorAttribute (Type type, Type baseType)
		{
			nametype = type;
			this.baseType = baseType;
		}

		public string EditorBaseTypeName {
			get {
				return basename;
			}
		}
		
		public string EditorTypeName {
			get {
				return name;
			}
		}

		public override object TypeId {
			get {
				return this.GetType ();
			}
		}
		
		public override bool Equals (object o)
		{
			if (!(obj is EditorAttribute))
				return false;

			return (((EditorAttribute) obj).name == name) &&
				(((EditorAttribute) obj).basename == basename) &&
				(((EditorAttribute) obj).baseType == baseType) &&
				(((EditorAttribute) obj).nametype == nametype);

		}
		
		public override int GetHashCode ()
		{
                        if (name == null)
	                        return 0;

                        return name.GetHashCode ();
		}
	}
}
