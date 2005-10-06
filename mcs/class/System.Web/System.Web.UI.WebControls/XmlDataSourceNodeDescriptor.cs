//
// System.Web.UI.WebControls.XmlDataSourceNodeDescriptor
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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
using System;
using System.Collections;
using System.ComponentModel;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Schema;
using AC = System.ComponentModel.AttributeCollection;

namespace System.Web.UI.WebControls
{
	internal class XmlDataSourceNodeDescriptor: ICustomTypeDescriptor, IXPathNavigable
	{
		XmlNode node;
		
		public XmlDataSourceNodeDescriptor (XmlNode node)
		{
			this.node = node;
		}
		
		public XmlNode Node {
			get { return node; }
		}
		
		public AC GetAttributes()
		{
			return AC.Empty;
		}

		public string GetClassName()
		{
			return "XmlDataSourceNodeDescriptor";
		}

		public string GetComponentName()
		{
			return null;
		}

		public TypeConverter GetConverter()
		{
			return null;
		}

		public EventDescriptor GetDefaultEvent()
		{
			return null;
		}

		public PropertyDescriptor GetDefaultProperty()
		{
			return null;
		}

		public object GetEditor(Type editorBaseType)
		{
			return null;
		}

		public EventDescriptorCollection GetEvents()
		{
			return null;
		}

		public EventDescriptorCollection GetEvents(Attribute[] arr)
		{
			return null;
		}

		public PropertyDescriptorCollection GetProperties()
		{
			if (node.Attributes != null) {
				PropertyDescriptor[] props = new PropertyDescriptor [node.Attributes.Count];
				for (int n=0; n<props.Length; n++)
					props [n] = new XmlDataSourcePropertyDescriptor (node.Attributes [n].Name, node.IsReadOnly);
				return new PropertyDescriptorCollection (props);
			} else
				return PropertyDescriptorCollection.Empty;
		}

		public PropertyDescriptorCollection GetProperties(Attribute[] arr)
		{
			return GetProperties ();
		}

		public object GetPropertyOwner (PropertyDescriptor pd)
		{
			if (pd is XmlDataSourcePropertyDescriptor)
				return this;
			return null;
		}

		public XPathNavigator CreateNavigator ()
		{
			return node.CreateNavigator();
		}
	}
}
#endif

