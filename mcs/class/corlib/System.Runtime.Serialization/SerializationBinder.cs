//
// System.Runtime.Serialization.SerializationBinder.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

namespace System.Runtime.Serialization
{
	[Serializable]
	public abstract class SerializationBinder
	{
		// Constructor
		protected SerializationBinder ()
			: base ()
		{
		}

		public abstract Type BindToType (string assemblyName, string typeName);
	}
}
