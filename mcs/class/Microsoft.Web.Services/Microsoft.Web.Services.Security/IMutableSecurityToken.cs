//
// IMutableSecurityToken.cs - IMutableSecurityToken interface
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace Microsoft.Web.Services.Security {

	public interface IMutableSecurityToken {

		SecurityToken Clone();

 	}
}
