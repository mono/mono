//
// PasswordOption.cs: Handles WS-Security PasswordOption
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace Microsoft.Web.Services.Security {

	[Serializable]
	public enum PasswordOption {
		SendNone,
		SendHashed,
		SendPlainText
	}
}
