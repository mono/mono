//
// System.Web.UI.Design.WebControls.DataListDesigner.cs
//
// Author: Duncan Mak (duncan@novell.com)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Windows.Forms.Design;

namespace System.Web.UI.Design.WebControls {

	public abstract class DataListDesigner : BaseDataListDesigner
	{
		public DataGridDesigner ()
			: base ()
		{
		}

		public override bool AllowResize {
			get { throw new NotImplementedException (); }
		}

		protected bool TemplatesExist {
			get { throw new NotImplementedException (); }
		}
				
		protected override ITemplateEditingFrame CreateTemplateEditingFrame (
			TemplateEditingVerb verb)
		{
			throw new NotImplementedException ();
		}

		protected override void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}

		protected override TemplateEditingVerb GetCachedTemplatedEditingVerbs ()
		{
			throw new NotImplementedException ();
		}

		public override string GetDesignTimeHtml ()
		{
			throw new NotImplementedException ();
		}

		public override string GetEmptyDesignTimeHtml ()
		{
			throw new NotImplementedException ();
		}
		
		public override string GetErrorDesignTimeHtml (Exception e)
		{
			throw new NotImplementedException ();
		}

		public override string GetTemplateContainerDataItemProperty (string template_name)
		{
			throw new NotImplementedException ();
		}

		public override string GetTemplateContent (
			ITemplateEditingFrame editing_frame,
			string template_name,
			out bool allow_editing)
		{
			throw new NotImplementedException ();
		}

		public override Type GetTemplatePropertyParentType (string template_name)
		{
			throw new NotImplementedException ();
		}

		public override void Initialize (IComponent component)
		{
			throw new NotImplementedException ();
		}

		protected void OnColumnsChanged ()
		{
			throw new NotImplementedException ();
		}

		protected abstract void OnTemplateEditingVerbsChanged ()
		{
			throw new NotImplementedException ();
		}

		public override void SetTemplateContent (
			ITemplateEditingFrame editing_frame,
			string template_name,
			string template_content)
		{
			throw new NotImplementedException ();
		}
	}
}