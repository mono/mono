//
// System.Web.UI.WebControls.XmlDataSourcePropertyDescriptor
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
using System.Xml;
using System.ComponentModel;

namespace System.Web.UI.WebControls {

	internal class XmlDataSourcePropertyDescriptor : PropertyDescriptor
	{
		bool readOnly;
		
		public XmlDataSourcePropertyDescriptor (string name, bool readOnly) : base (name, null)
		{
			this.readOnly = readOnly;
		}
		
		public override bool CanResetValue (object o)
		{
			return false;
		}
		
		public override void ResetValue (object o)
		{
		}
		
		public override object GetValue (object o)
		{
			XmlDataSourceNodeDescriptor nd = o as XmlDataSourceNodeDescriptor;
			if (nd != null && nd.Node.Attributes != null) {
				XmlAttribute a = nd.Node.Attributes [Name];
				if (a != null) return a.Value;
			}
			return String.Empty;
		}
		
		public override void SetValue (object o, object value)
		{
			XmlDataSourceNodeDescriptor nd = o as XmlDataSourceNodeDescriptor;
			if (nd != null && nd.Node.Attributes != null) {
				XmlAttribute a = nd.Node.Attributes [Name];
				if (a != null) a.Value = value.ToString ();
			}
		}
		
		public override bool ShouldSerializeValue (object o)
		{
			return o is XmlNode;
		}
		
		public override Type ComponentType {
			get { return typeof (XmlDataSourceNodeDescriptor); }
		}
		
		public override bool IsReadOnly {
			get { return readOnly; }
		}
		
		public override Type PropertyType {
			get { return typeof (string); }
		}
	}
}
#endif

