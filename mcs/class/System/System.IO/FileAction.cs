// 
// System.IO.FileAction.cs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2004 Novell, Inc. (http://www.novell.com)
//

namespace System.IO {
	enum FileAction {
		Added = 1,
		Removed = 2,
		Modified = 3,
		RenamedOldName = 4,
		RenamedNewName = 5
	}
}
