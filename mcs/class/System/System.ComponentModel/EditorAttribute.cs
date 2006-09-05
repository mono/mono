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

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
