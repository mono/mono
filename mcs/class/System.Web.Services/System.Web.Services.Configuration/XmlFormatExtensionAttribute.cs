 // 
// System.Web.Services.Configuration.XmlFormatExtensionAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
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

namespace System.Web.Services.Configuration {
	[AttributeUsage (AttributeTargets.Class, Inherited = true)]
	public sealed class XmlFormatExtensionAttribute : Attribute {

		#region Fields

		string elementName;
		string ns;
		Type[] extensionPoints;

		#endregion // Fields

		#region Constructors

		public XmlFormatExtensionAttribute ()
		{
		}

		public XmlFormatExtensionAttribute (string elementName, string ns, Type extensionPoint1)
			: this (elementName, ns, new Type[1] {extensionPoint1})
		{
		}

		public XmlFormatExtensionAttribute (string elementName, string ns, Type[] extensionPoints)
			: this ()
		{
			this.elementName = elementName;
			this.ns = ns;
			this.extensionPoints = extensionPoints;
		}

		public XmlFormatExtensionAttribute (string elementName, string ns, Type extensionPoint1, Type extensionPoint2)
			: this (elementName, ns, new Type[2] {extensionPoint1, extensionPoint2})
		{
		}

		public XmlFormatExtensionAttribute (string elementName, string ns, Type extensionPoint1, Type extensionPoint2, Type extensionPoint3)
			: this (elementName, ns, new Type[3] {extensionPoint1, extensionPoint2, extensionPoint3})
		{
		}

		public XmlFormatExtensionAttribute (string elementName, string ns, Type extensionPoint1, Type extensionPoint2, Type extensionPoint3, Type extensionPoint4)
			: this (elementName, ns, new Type[4] {extensionPoint1, extensionPoint2, extensionPoint3, extensionPoint4})
		{
		}
		
		#endregion // Constructors

		#region Properties

		public string ElementName {
			get { return elementName; }
			set { elementName = value; }
		}

		public Type[] ExtensionPoints {
			get { return extensionPoints; }
			set { extensionPoints = value; }
		}

		public string Namespace {
			get { return ns; }
			set { ns = value; }
		}

		#endregion // Properties
	}
}
