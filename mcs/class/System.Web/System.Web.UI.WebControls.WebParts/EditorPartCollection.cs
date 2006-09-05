//
// System.Web.UI.WebControls.WebParts.EditorPartCollection.cs
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

namespace System.Web.UI.WebControls.WebParts
{
	public sealed class EditorPartCollection : ReadOnlyCollectionBase
	{
		public static readonly EditorPartCollection Empty = new EditorPartCollection ();

		public EditorPartCollection ()
		{
		}

		
		public EditorPartCollection (ICollection editorParts)
		{
			foreach (object o in editorParts) {
				/* XXX check the type? */
				InnerList.Add (o);
			}
		}

		public EditorPartCollection (EditorPartCollection existingEditorParts,
					     ICollection editorParts)
		{
			foreach (object o in existingEditorParts)
				InnerList.Add (o);
			foreach (object o in editorParts)
				InnerList.Add (o);
		}

		public bool Contains (EditorPart editorPart)
		{
			return InnerList.Contains (editorPart);
		}

		public void CopyTo (EditorPart[] array,
				    int index)
		{
			((ICollection)this).CopyTo (array, index);
		}

		public int IndexOf (EditorPart editorPart)
		{
			return InnerList.IndexOf (editorPart);
		}

		public EditorPart this [ int index ] {
			get { return (EditorPart) InnerList[index]; }
		}
	}

}

#endif
