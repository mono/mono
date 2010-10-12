//
// System.Web.Compilation.AppCodeCompiler: A compiler for the App_Code folder
//
// Authors:
//   Marek Habersack (grendello@gmail.com)
//
// (C) 2006 Marek Habersack
// (C) 2007-2010 Novell, Inc (http://novell.com/)
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
using System.Security.Permissions;

namespace System.Web.UI 
{
	// CAS
        [AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
        [AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class HiddenFieldPageStatePersister : PageStatePersister
	{
		public HiddenFieldPageStatePersister (Page page)
			: base(page)
		{
		}
		
		public override void Load ()
		{
#if TARGET_J2EE
			if (Page.FacesContext != null) {
				if (Page.PageState != null) {
					ViewState = Page.PageState.First;
					ControlState = Page.PageState.Second;
				}
				return;
			}
#endif
			string rawViewState = Page.RawViewState;
			IStateFormatter formatter = StateFormatter;
			if (!String.IsNullOrEmpty (rawViewState)) {
				Pair pair = formatter.Deserialize (rawViewState) as Pair;
				if (pair != null) {
					ViewState = pair.First;
					ControlState = pair.Second;
				}
			}
		}

		public override void Save ()
		{
#if TARGET_J2EE
			if (Page.FacesContext != null) {
				if (ViewState != null || ControlState != null)
					Page.PageState = new Pair (ViewState, ControlState);
				return;
			}
#endif
			IStateFormatter formatter = StateFormatter;
			Page.RawViewState = formatter.Serialize (new Pair (ViewState, ControlState));
		}
	}
}

