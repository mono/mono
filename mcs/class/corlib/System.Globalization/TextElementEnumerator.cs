//
// System.Globalization.TextElementEnumerator.cs
//
// Author:
//	Dick Porter (dick@ximian.com)
//
// (C) 2002 Ximian, Inc.
//

using System.Collections;

namespace System.Globalization {

	[Serializable]
	public class TextElementEnumerator: IEnumerator {
		/* Hide the .ctor() */
		TextElementEnumerator() {}

		[MonoTODO]
		public object Current 
		{
			get {
				throw new NotImplementedException();
			}
		}

		[MonoTODO]
		public int ElementIndex 
		{
			get {
				throw new NotImplementedException();
			}
		}

		[MonoTODO]
		public string GetTextElement()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public bool MoveNext()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void Reset()
		{
			throw new NotImplementedException();
		}
	}
}
