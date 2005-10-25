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
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using Microsoft.Web.UI;

namespace Microsoft.Web
{

	[ParseChildren (true, "Bindings")]
	public abstract class ScriptComponent :  Control, IScriptComponent, IScriptObject
	{
		protected ScriptComponent ()
		{
		}

		protected virtual void AddAttributesToElement (ScriptTextWriter writer)
		{
			if (ID != null && ID != "")
				writer.WriteAttributeString ("id", ID);
		}

		ScriptTypeDescriptor IScriptObject.GetTypeDescriptor ()
		{
			ScriptTypeDescriptor td = new ScriptTypeDescriptor(this);
			InitializeTypeDescriptor (td);
			td.Close();
			return td;
		}

		protected virtual void InitializeTypeDescriptor (ScriptTypeDescriptor typeDescriptor)
		{
			foreach (ScriptEvent ev in ((IEnumerable<ScriptEvent>)ScriptEvents))
				typeDescriptor.AddEvent (new ScriptEventDescriptor (ev.Name, ev.SupportsActions));
			typeDescriptor.AddProperty (new ScriptPropertyDescriptor ("bindings", ScriptType.Array, true, "Bindings"));
			typeDescriptor.AddProperty (new ScriptPropertyDescriptor ("dataContext", ScriptType.Object));
			typeDescriptor.AddProperty (new ScriptPropertyDescriptor ("id", ScriptType.String, "ID"));
		}

		public ScriptTypeDescriptor GetTypeDescriptor ()
		{
			ScriptTypeDescriptor td = new ScriptTypeDescriptor(this);
			InitializeTypeDescriptor (td);
			td.Close();
			return td;
		}

		protected override void OnInit (EventArgs e)
		{
			base.OnInit (e);

			ScriptManager mgr = ScriptManager.GetCurrentScriptManager (Page);
			if (mgr == null)
				throw new InvalidOperationException ("The page must contain a ScriptManager or the control must be placed inside a container");

			mgr.RegisterComponent (this);
		}

		void IScriptComponent.RenderScript (ScriptTextWriter writer)
		{
			RenderScriptBeginTag (writer);
			AddAttributesToElement (writer);
			RenderScriptTagContents (writer);
			RenderScriptEndTag (writer);
		}

		protected virtual void RenderScriptBeginTag (ScriptTextWriter writer)
		{
			writer.WriteStartElement (TagName);
		}

		protected virtual void RenderScriptEndTag (ScriptTextWriter writer)
		{
			writer.WriteEndElement ();
		}

		protected virtual void RenderScriptTagContents (ScriptTextWriter writer)
		{
			if (bindings != null && bindings.Count > 0) {
				writer.WriteStartElement ("bindings");
				foreach (Binding b in bindings) {
					b.RenderScript (writer);
				}
				writer.WriteEndElement (); // bindings
			}
		}

		BindingCollection bindings;
		public BindingCollection Bindings {
			get {
				if (bindings == null)
					bindings = new BindingCollection(this);
				return bindings;
			}
		}

		public IScriptComponentContainer Container {
			get {
				throw new NotImplementedException ();
			}
		}

		ScriptEvent propertyChanged = null;
		public ScriptEvent PropertyChanged {
			get {
				if (propertyChanged == null)
					propertyChanged = new ScriptEvent (this, "propertyChanged", true);
				return propertyChanged;
			}
		}

		ScriptEventCollection scriptEvents;
		protected ScriptEventCollection ScriptEvents {
			get {
				if (scriptEvents == null) {
					scriptEvents = new ScriptEventCollection (this);
					scriptEvents.Add (PropertyChanged);
				}

				return scriptEvents;
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
				return null;
			}
		}
	}
}

#endif
