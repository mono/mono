//
// System.Security.ISecurityEncodable.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc. http://www.ximian.com
//

namespace System.Security {

	public interface ISecurityEncodable {

		void FromXml (SecurityElement e);

		SecurityElement ToXml ();
	}
}
