//
// System.Web.UI.WebControls.Content.cs
//
// Authors:
//   Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2005 Novell, Inc.
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
using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	[ToolboxItem (false)]
	[DesignerAttribute ("System.Web.UI.Design.WebControls.ContentDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[ControlBuilder(typeof(ContentBuilderInternal))]
	public class Content: Control, INamingContainer
	{
		string placeHolderId;
		
		[ThemeableAttribute (false)]
		[DefaultValueAttribute ("")]
		[WebCategoryAttribute ("Behavior")]
		[IDReferencePropertyAttribute (typeof(ContentPlaceHolder))]
		public string ContentPlaceHolderID {
			get { return placeHolderId; }
			set { placeHolderId = value; }
		}

		static readonly object DataBindingEvent = new object ();
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public new event EventHandler DataBinding {
			add { Events.AddHandler (DataBindingEvent, value); }
			remove { Events.RemoveHandler (DataBindingEvent, value); }
		}

		static readonly object DisposedEvent = new object ();
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public new event EventHandler Disposed {
			add { Events.AddHandler (DisposedEvent, value); }
			remove { Events.RemoveHandler (DisposedEvent, value); }
		}

		static readonly object InitEvent = new object ();
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public new event EventHandler Init {
			add { Events.AddHandler (InitEvent, value); }
			remove { Events.RemoveHandler (InitEvent, value); }
		}

		static readonly object LoadEvent = new object ();
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public new event EventHandler Load {
			add { Events.AddHandler (LoadEvent, value); }
			remove { Events.RemoveHandler (LoadEvent, value); }
		}

		static readonly object PreRenderEvent = new object ();
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public new event EventHandler PreRender {
			add { Events.AddHandler (PreRenderEvent, value); }
			remove { Events.RemoveHandler (PreRenderEvent, value); }
		}

		static readonly object UnloadEvent = new object ();
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public new event EventHandler Unload {
			add { Events.AddHandler (UnloadEvent, value); }
			remove { Events.RemoveHandler (UnloadEvent, value); }
		}
	}
}

#endif
