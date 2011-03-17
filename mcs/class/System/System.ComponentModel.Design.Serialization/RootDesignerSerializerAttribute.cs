//
// System.ComponentModel.Design.Serialization.RootDesignerSerializerAttribute.cs
//
// Authors:
//   Alejandro Sánchez Acosta (raciel@gnome.org)
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

namespace System.ComponentModel.Design.Serialization
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
	[Obsolete ("Use DesignerSerializerAttribute instead")]
	public sealed class RootDesignerSerializerAttribute : Attribute
	{
		private string serializer;
		private string baseserializer;
		private bool reload;
		
		public RootDesignerSerializerAttribute (string serializerTypeName, string baseSerializerTypeName, bool reloadable) {
			this.serializer = serializerTypeName;
			this.baseserializer = baseSerializerTypeName;
			this.reload = reloadable;
		}

		public RootDesignerSerializerAttribute (string serializerTypeName, Type baseSerializerType, bool reloadable)
			: this (serializerTypeName, baseSerializerType.AssemblyQualifiedName, reloadable)
		{
		}

		public RootDesignerSerializerAttribute (Type serializerType, Type baseSerializerType, bool reloadable) 
			: this (serializerType.AssemblyQualifiedName, baseSerializerType.AssemblyQualifiedName, reloadable)
		{
		}

		public bool Reloadable {
			get {
				return this.reload;
			}
		}

		public string SerializerBaseTypeName {
			get {
				return this.baseserializer;
			}
		}

		public string SerializerTypeName {
			get {
				return this.serializer;
			}
		}

		public override object TypeId {
			get { return string.Concat (this.ToString(), baseserializer);}
		}
	}
}
