//
// System.MonoDummy.cs
//
// Author:
//   Dietmar Maurer (dietmar@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System {

	public class MonoDummy {

		public MonoDummy ()
		{
			throw new ExecutionEngineException ();
		}
		
	}
}
