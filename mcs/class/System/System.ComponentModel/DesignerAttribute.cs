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

using System.ComponentModel.Design;

namespace System.ComponentModel 
{

	/// <summary>
	///   Designer Attribute for classes. 
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
	public sealed class DesignerAttribute : Attribute
	{
		private string name;
		private string basetypename;
			
		public DesignerAttribute (string designerTypeName)
		{
			if (designerTypeName == null)
				throw new NullReferenceException ();
			name = designerTypeName;
			basetypename = typeof(IDesigner).FullName;
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
			if (designerTypeName == null)
				throw new NullReferenceException ();
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
				string baseTypeNameOnly = basetypename;
				int index = baseTypeNameOnly.IndexOf (',');
				if (index != -1) // strip name
					baseTypeNameOnly = baseTypeNameOnly.Substring (0, index);
				return this.GetType ().ToString() + baseTypeNameOnly;
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
