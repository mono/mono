//
// System.MonoDummy.cs
//
// Author:
//   Dietmar Maurer (dietmar@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System {
	[MonoTODO("Remove this class when no longer needed by mono_class_from_typeref in class.c")]
	public class MonoDummy {

		public MonoDummy ()
		{
			throw new ExecutionEngineException ();
		}
		
	}
}
