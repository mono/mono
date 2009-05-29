//
// System.Web.UI.WebControls.WebParts.EditorPart.cs
//
// Authors:
//      Chris Toshok (toshok@ximian.com)
//
// (C) 2006 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.ComponentModel;

namespace System.Web.UI.WebControls.WebParts
{

	[BindableAttribute(false)]
	[Designer ("System.Web.UI.Design.WebControls.WebParts.EditorPartDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public abstract class EditorPart : Part
	{	
		bool				display = true;
#pragma warning disable 0649
		WebPart				webPartToEdit;
#if false
		WebPartManager		manager;
		EditorZoneBase		zone;
#endif
		object zone;
		string				displayTitle;
#pragma warning restore 0649

		protected EditorPart() {}

		public abstract bool ApplyChanges ();

		protected override IDictionary GetDesignModeState ()
		{
			throw new NotImplementedException ();
		}

		protected internal override void OnPreRender (EventArgs e)
		{
			if(zone ==  null)
				throw new InvalidOperationException();
			base.OnPreRender(e);
			if(!Display)
				Visible = false;
		}
#if false
		protected override void SetDesignModeState (IDictionary data)
		{
			EditorZoneBase stateZone = data["Zone"] as EditorZoneBase;
			if(stateZone != null)
				zone = stateZone;
		}
#endif
		public abstract void SyncChanges ();

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual bool Display {
			get { 
				return display;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string DisplayTitle {
			get { return displayTitle; }
		}

#if false
		protected WebPartManager WebPartManager {
			get { throw new NotImplementedException (); }
		}
#endif

		protected WebPart WebPartToEdit {
			get { return webPartToEdit; }
		}

#if false
		protected EditorZoneBase Zone {
			get { throw new NotImplementedException (); }
		}
#endif
	}
}

#endif
