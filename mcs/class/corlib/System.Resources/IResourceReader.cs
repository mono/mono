//
// System.Resources.IResourceReader.cs
//
// Author:
//	Duncan Mak (duncan@ximian.com)
//
// (C) 2001 Ximian, Inc. http://www.ximian.com
//

namespace System.Resources {

	   public interface IResourceReader : IEnumerable, IDisposible {
			 void Close();
			 IDictionaryEnumerator GetEnumerator();
	   }
}
