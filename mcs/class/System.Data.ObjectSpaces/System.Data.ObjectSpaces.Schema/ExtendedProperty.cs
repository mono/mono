//
// System.Data.ObjectSpaces.Schema.ExtendedProperty.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
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

#if NET_2_0

using System.Xml;

namespace System.Data.ObjectSpaces.Schema {
	public class ExtendedProperty 
	{
		#region Fields

		XmlQualifiedName qname;
		object propertyValue;
		string prefix;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public ExtendedProperty (XmlQualifiedName qualifiedName, object propertyValue)
		{
			QualifiedName = qualifiedName;
			PropertyValue = propertyValue;
		}

		public ExtendedProperty (XmlQualifiedName qualifiedName, object propertyValue, string prefix)
			: this (qualifiedName, propertyValue)
		{
			Prefix = prefix;
		}

		#endregion // Constructors

		#region Properties

		public string Prefix {
			get { return prefix; }
			set { prefix = value; }
		}

		public object PropertyValue {
			get { return propertyValue; }
			set { propertyValue = value; }
		}

		public XmlQualifiedName QualifiedName {
			get { return qname; }
			set { qname = value; }
		}

		#endregion // Properties
	}
}

#endif // NET_2_0
