//
// System.Globalization.TextElementEnumerator.cs
//
// Author:
//	Dick Porter (dick@ximian.com)
//
// (C) 2002 Ximian, Inc.
// (C) 2004 Novell, Inc.
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Collections;

namespace System.Globalization {

	[Serializable]
#if NET_2_0
	[System.Runtime.InteropServices.ComVisible (true)]
#endif
	public class TextElementEnumerator: IEnumerator {
		private int index;
		private int elementindex;
		private int startpos;
		private string str;
		private string element;
		
		/* Hide the .ctor() */
		internal TextElementEnumerator(string str, int startpos) {
			this.index = -1;
			this.startpos = startpos;
			this.str = str.Substring (startpos);
			this.element = null;
		}

		public object Current 
		{
			get {
				if (element == null) {
					throw new InvalidOperationException ();
				}

				return(element);
			}
		}

		public int ElementIndex 
		{
			get {
				if (element == null) {
					throw new InvalidOperationException ();
				}

				return(elementindex + startpos);
			}
		}

		public string GetTextElement()
		{
			if (element == null) {
				throw new InvalidOperationException ();
			}

			return(element);
		}

		public bool MoveNext()
		{
			elementindex = index + 1;
			
			if (elementindex < str.Length) {
				element = StringInfo.GetNextTextElement (str, elementindex);
				index += element.Length;
				
				return(true);
			} else {
				element = null;

				return(false);
			}
		}

		public void Reset()
		{
			element = null;
			index = -1;
		}
	}
}
