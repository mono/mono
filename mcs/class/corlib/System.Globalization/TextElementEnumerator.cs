//
// System.Globalization.TextElementEnumerator.cs
//
// Author:
//	Dick Porter (dick@ximian.com)
//
// (C) 2002 Ximian, Inc.
// (C) 2004 Novell, Inc.
//

using System.Collections;

namespace System.Globalization {

	[Serializable]
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
