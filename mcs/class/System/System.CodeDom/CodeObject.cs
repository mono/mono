//
// System.CodeDom CodeObject class implementation
//
// Author:
//   Sean MacIsaac (macisaac@ximian.com)
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2001 Ximian, Inc.
//

using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Specialized;

namespace System.CodeDom
{
	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class CodeObject
	{
		private IDictionary userData;

		//
		// Properties
		//
		public IDictionary UserData {
			get {
				if ( userData == null )
					userData = new ListDictionary();
				return userData;
			}
		}
	}
}
