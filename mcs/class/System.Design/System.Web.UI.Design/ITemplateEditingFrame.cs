//
// System.Web.UI.Design.ITemplateEditingFrame
//
// Authors:
//      Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004 Novell
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

using System.Web.UI.WebControls;

namespace System.Web.UI.Design
{
#if NET_2_0
	[Obsolete ("Template editing is supported in ControlDesigner.TemplateGroups with SetViewFlags(ViewFlags.TemplateEditing, true) in 2.0.")]
#endif
	public interface ITemplateEditingFrame : IDisposable
	{
		void Close (bool saveChanges);
		void Open ();
		void Resize (int width, int height);
		void Save ();
		void UpdateControlName (string newName);

		Style ControlStyle {
			get;
		}
		int InitialHeight {
			get;
			set;
		}
		int InitialWidth {
			get;
			set;
		}
		string Name {
			get;
		}
		string[] TemplateNames {
			get;
		}
		Style[] TemplateStyles {
			get;
		}
		TemplateEditingVerb Verb {
			get;
			set;
		}
	}
}
