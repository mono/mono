

//
// System/GC.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System {
	public sealed class GC {

		private GC () {}
		
		[MonoTODO]
		public static void SuppressFinalize (object obj)
		{
			//throw new NotImplementedException ();
		}

	}
}
