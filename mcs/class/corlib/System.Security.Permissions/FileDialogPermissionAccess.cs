//
// System.Security.Permissions.FileDialogPermissionAccess
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc. http://www.ximian.com
//

using System;

[Flags]
[Serializable]
public enum FileDialogPermissionAccess {
	None = 0,
	Open = 1,
	Save = 2,
	OpenSave = 3,
}

