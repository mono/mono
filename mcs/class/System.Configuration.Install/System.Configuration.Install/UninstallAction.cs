//
// System.Configuration.Install.UninstallAction.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002
//

using System;

namespace System.Configuration.Install {

	[Serializable]
	public enum UninstallAction {
		NoAction=0x01,
		Remove=0x00
	}
}

