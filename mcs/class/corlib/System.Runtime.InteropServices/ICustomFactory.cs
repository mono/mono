//
// System.Runtime.InteropServices.ICustomFactory.cs
//
// Author:
//   Kevin Winchester (kwin@ns.sympatico.ca)
//
// (C) 2002 Kevin Winchester
//

namespace System.Runtime.InteropServices {

	public interface ICustomFactory {
		MarshalByRefObject CreateInstance (Type serverType);
	}
}
