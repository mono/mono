//
// System.Security.Principal.WindowsImpersonationContext
//
// Authors:
//      Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;

namespace System.Security.Principal
{
	public class WindowsImpersonationContext
	{
		internal WindowsImpersonationContext ()
		{
		}

		~WindowsImpersonationContext ()
		{
		}

		[MonoTODO]
		public void Undo ()
		{
			throw new NotImplementedException ();
		}
	}
}

