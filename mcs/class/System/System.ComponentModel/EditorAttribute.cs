//
// System.ComponentModel.EditorAttribute.cs
//
// Author:
//   Alejandro Sánchez Acosta (raciel@es.gnu.org)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Alejandro Sánchez Acosta
// (C) 2003 Andreas Nahr
//

namespace System.ComponentModel {

	/// <summary>
	///   Editor Attribute for classes. 
	/// </summary>
	[AttributeUsage (AttributeTargets.All, AllowMultiple = true, Inherited = true)]
	public sealed class EditorAttribute : Attribute {
		
		string name;
		string basename;

		public EditorAttribute ()
		{
			this.name = string.Empty;
		}

		public EditorAttribute (string typeName, string baseTypeName)
		{
			name = typeName;
			basename = baseTypeName;
		}

		public EditorAttribute (string typeName, Type baseType)
			: this (typeName, baseType.AssemblyQualifiedName)
		{
		}

		public EditorAttribute (Type type, Type baseType)
			: this (type.AssemblyQualifiedName, baseType.AssemblyQualifiedName)
		{
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
		
		public override bool Equals (object obj)
		{
			if (!(obj is EditorAttribute))
				return false;

			return ((EditorAttribute) obj).EditorBaseTypeName.Equals (basename) &&
				((EditorAttribute) obj).EditorTypeName.Equals (name);
		}
		
		public override int GetHashCode ()
		{
			return string.Concat(name, basename).GetHashCode ();
		}
	}
}
