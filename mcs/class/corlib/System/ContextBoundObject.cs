//
// System.ContextBoundObject.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
// TODO: Mucho left to implement.
//

namespace System {

	/// <summary>
	///   Base class for all the context-bound classes
	/// </summary>
	[Serializable]
	public abstract class ContextBoundObject : MarshalByRefObject {

		protected ContextBoundObject ()
		{
		}
	}
}
