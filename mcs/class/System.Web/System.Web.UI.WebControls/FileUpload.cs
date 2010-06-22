//
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
//
// Authors:
//	Andrew Skiba <andrews@mainsoft.com>
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
using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Security.Permissions;
using System.IO;

namespace System.Web.UI.WebControls
{
	[ControlValueProperty ("FileBytes")]
	[ValidationProperty ("FileName")]
	[Designer ("DesignerBaseTypeNameSystem.ComponentModel.Design.IDesignerDesignerTypeNameSystem.Web.UI.Design.WebControls.PreviewControlDesigner, " + Consts.AssemblySystem_Design)]
	[AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal, Unrestricted=false)]
	[AspNetHostingPermissionAttribute(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal, Unrestricted=false)]
	public class FileUpload : WebControl
	{
		public FileUpload ()
			: base (HtmlTextWriterTag.Input)
		{
		}

		byte[] cachedBytes;
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Bindable (true, BindingDirection.OneWay)]
		[Browsable (false)]
		public byte[] FileBytes {
			get {
				if (cachedBytes == null) {
					cachedBytes = new byte[FileContent.Length];
					FileContent.Read (cachedBytes, 0, cachedBytes.Length);
				}
				return (byte [])(cachedBytes.Clone ());
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Stream FileContent {
			get {
				if (PostedFile == null)
					return Stream.Null;
				else {
					Stream ret = PostedFile.InputStream;
					if (ret != null)
						ret.Position = 0;
					
					return ret;
				}
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string FileName {
			get {
				if (PostedFile == null)
					return String.Empty;
				else
					return Path.GetFileName (PostedFile.FileName);
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public bool HasFile {
			get {
				HttpPostedFile pf = PostedFile;
				return (pf != null && !String.IsNullOrEmpty (pf.FileName));
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public HttpPostedFile PostedFile {
			get {
				Page page = Page;
				if (page == null || !page.IsPostBack)
					return null;
				if (Context == null || Context.Request == null)
					return null;
				return Context.Request.Files[UniqueID];
			}
		}

		protected override void AddAttributesToRender (HtmlTextWriter writer)
		{
			writer.AddAttribute (HtmlTextWriterAttribute.Type, "file", false);
			if (!string.IsNullOrEmpty (UniqueID))
				writer.AddAttribute (HtmlTextWriterAttribute.Name, UniqueID);
			base.AddAttributesToRender (writer);
		}

		protected internal override void OnPreRender (System.EventArgs e)
		{
			base.OnPreRender (e);
			Page page = Page;
			if (page != null)
				page.Form.Enctype = "multipart/form-data";
		}

		protected internal override void Render (HtmlTextWriter writer)
		{
			Page page = Page;
			if (page != null)
				page.VerifyRenderingInServerForm (this);
			base.Render (writer);
		}

		public void SaveAs (string filename)
		{
			HttpPostedFile file = PostedFile;
			if (file != null)
				file.SaveAs (filename);
		}
	}
}

