//
// System.Resources.ResXResourcereader.cs
//
// Author:
//	Duncan Mak <duncan@ximian.com>
//
// 2001(C) Ximian, Inc.		http://www.ximian.com
//

using System.Collections;

namespace System.Resources {
	   public class ResXResourceReader : IResourceReader, IEnumerable, IDisposable {
			public ResXResourceReader () {}
	
			// TODO:
			public void Close () {}
			IDictionaryEnumerator IResourceReader.GetEnumerator () { return null; }
			IEnumerator IEnumerable.GetEnumerator () { return null; }




			// TODO:
			protected virtual void Dispose (bool disposing) {}
			public void Dispose () {}
	   	}
}
