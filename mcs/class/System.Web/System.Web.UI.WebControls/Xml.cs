//
// System.Web.UI.WebControls.Xml.cs
//
// Authors:
//	Miguel de Icaza (miguel@novell.com)
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

using System.ComponentModel;
using System.Security.Permissions;
using System.Xml;
using System.Xml.Xsl;

using System.Xml.XPath;
using System.Collections;

namespace System.Web.UI.WebControls {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[DefaultProperty ("DocumentSource")]
	[Designer ("System.Web.UI.Design.WebControls.XmlDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[PersistChildren (true)]
	[ControlBuilder (typeof (XmlBuilder))] 
	public class Xml : Control {
		// Property set variables
		XmlDocument xml_document;
		XPathNavigator xpath_navigator;
		string xml_content;
		string xml_file;

		XslTransform xsl_transform;
		XsltArgumentList transform_arguments;
		string transform_file;

		public Xml ()
		{
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[MonoTODO ("Anything else?")]
		public override string ClientID
		{
			get {
				return base.ClientID;
			}
		}

		[MonoTODO ("Anything else?")]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override ControlCollection Controls 
		{
			get {
				return base.Controls;
			}
		}
		

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
[Obsolete ("Use the XPathNavigator property instead by creating an XPathDocument and calling CreateNavigator().")]
		public XmlDocument Document {
			get {
				return xml_document;
			}

			set {
				xml_content = null;
				xml_file = null;
				xml_document = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string DocumentContent {
			get {
				return (xml_content != null)? xml_content : "";
			}

			set {
				xml_content = value;
				xml_file = null;
				xml_document = null;
			}
		}

		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.XmlUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		[MonoLimitation ("Absolute path to the file system is not supported; use a relative URI instead.")]
		public string DocumentSource {
			get {
				if (xml_file == null)
					return "";
				
				return xml_file;
			}

			set {
				xml_content = null;
				xml_file = value;
				xml_document = null;
			}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		[DefaultValue (false)]
		public override bool EnableTheming 
		{
			get { return false; }
			set { throw new NotSupportedException (); }
		}

		[DefaultValue ("")]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		public override string SkinID
		{
			// MSDN: Always returns an empty string (""). This property is not supported.
			get { return String.Empty; }
			// MSDN: Any attempt to set the value of this property throws a NotSupportedException exception.
			set { throw new NotSupportedException ("SkinID is not supported on Xml control"); }
		}
		

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public XslTransform Transform {
			get {
				return xsl_transform;
			}

			set {
				transform_file = null;
				xsl_transform = value;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public XsltArgumentList TransformArgumentList {
			get {
				return transform_arguments;
			}

			set {
				transform_arguments = value;
			}
		}

		[DefaultValue ("")]
		[Editor ("System.Web.UI.Design.XslUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[MonoLimitation ("Absolute path to the file system is not supported; use a relative URI instead.")]
		public string TransformSource {
			get {
				if (transform_file == null)
					return "";
				return transform_file;
			}

			set {
				transform_file = value;
				xsl_transform = null;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public XPathNavigator XPathNavigator 
		{
			get { return xpath_navigator; }
			set { xpath_navigator = value; }
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Control FindControl (string id) 
		{
			return null;
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public override void Focus ()
		{
			throw new NotSupportedException ();
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public override bool HasControls ()
		{
			return false;
		}

		protected internal
		override void Render (HtmlTextWriter output)
		{
			XmlDocument xml_doc = null;

			if (xpath_navigator == null) {
				if (xml_document != null)
					xml_doc = xml_document;
				else {
					if (xml_content != null) {
						xml_doc = new XmlDocument ();
						xml_doc.LoadXml (xml_content);
					}
					else if (xml_file != null) {
						xml_doc = new XmlDocument ();
						xml_doc.Load (MapPathSecure (xml_file));
					}
					else
						return;
				}
			}

			XslTransform t = xsl_transform;
			if (transform_file != null){
				t = new XslTransform ();
				t.Load (MapPathSecure (transform_file));
			}

			if (t != null){
				if (xpath_navigator != null) {
					t.Transform(xpath_navigator, transform_arguments, output);
				}
				else {
					t.Transform (xml_doc, transform_arguments, output, null);
				}
				return;
			}
				
			XmlTextWriter xmlwriter = new XmlTextWriter (output);
			xmlwriter.Formatting = Formatting.None;
			if (xpath_navigator != null) {
				xmlwriter.WriteStartDocument ();
				xpath_navigator.WriteSubtree (xmlwriter);
			}
			else {
				xml_doc.Save (xmlwriter);
			}
		}

		protected override void AddParsedSubObject (object obj)
		{
			LiteralControl lc = obj as LiteralControl;
			
			if (lc != null){
				xml_document = new XmlDocument ();
				xml_document.LoadXml (lc.Text);
			} else {
				throw new HttpException (
							 String.Format ("Objects of type {0} are not supported as children of the Xml control",
									obj.GetType ()));
			}
		}

		protected override ControlCollection CreateControlCollection ()
		{
			return new EmptyControlCollection (this);
		}

		[MonoTODO("Always returns null")]
		protected override IDictionary GetDesignModeState ()
		{
			return null;
		}
	}
}
