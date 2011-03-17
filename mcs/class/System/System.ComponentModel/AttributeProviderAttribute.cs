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

//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
//

namespace System.ComponentModel 
{
	[AttributeUsage (AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class AttributeProviderAttribute : Attribute
	{
		string type_name;
		string property_name;

		public AttributeProviderAttribute (Type type)
		{
			type_name = type.AssemblyQualifiedName;
		}

		public AttributeProviderAttribute (string typeName, string propertyName)
		{
			type_name = typeName;
			property_name = propertyName;
		}

		public AttributeProviderAttribute (string typeName)
		{
			type_name = typeName;
		}

		public string PropertyName {
			get { return property_name; }
		}

		public string TypeName {
			get { return type_name; }
		}
	}
}
