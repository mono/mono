//
// Microsoft.Web.ScriptComponent
//
// Author:
//   Chris Toshok (toshok@ximian.com)
//
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

#if NET_2_0

using System;
using System.ComponentModel;
using System.Web.UI;

namespace Microsoft.Web
{

	public abstract class ScriptComponent :  Control, IScriptComponent, IScriptObject
	{
		protected ScriptComponent ()
		{
			throw new NotImplementedException ();
		}

		protected virtual void AddAttributesToElement (ScriptTextWriter writer)
		{
			throw new NotImplementedException ();
		}

		protected virtual void InitializeTypeDescriptor (ScriptTypeDescriptor typeDescriptor)
		{
			throw new NotImplementedException ();
		}

		public void RenderScript (ScriptTextWriter writer)
		{
			throw new NotImplementedException ();
		}

		public ScriptTypeDescriptor GetTypeDescriptor ()
		{
			throw new NotImplementedException ();
		}

		protected override void OnInit (EventArgs e)
		{
			base.OnInit (e);
		}

		protected virtual void RenderScriptBeginTag (ScriptTextWriter writer)
		{
			throw new NotImplementedException ();
		}

		protected virtual void RenderScriptEndTag (ScriptTextWriter writer)
		{
			throw new NotImplementedException ();
		}

		protected virtual void RenderScriptTagContents (ScriptTextWriter writer)
		{
			throw new NotImplementedException ();
		}

		public BindingCollection Bindings {
			get {
				throw new NotImplementedException ();
			}
		}

#if notyet
		public IScriptComponentContainer Container {
			get {
				throw new NotImplementedException ();
			}
		}
#endif

		public ScriptEvent PropertyChanged {
			get {
				throw new NotImplementedException ();
			}
		}

		public ScriptEventCollection ScriptEvents {
			get {
				throw new NotImplementedException ();
			}
		}

		public Microsoft.Web.UI.ScriptManager ScriptManager {
			get {
				throw new NotImplementedException ();
			}
		}

		public abstract string TagName { get; }

		IScriptObject IScriptObject.Owner {
			get {
				throw new NotImplementedException ();
			}
		}
	}
}

#endif
