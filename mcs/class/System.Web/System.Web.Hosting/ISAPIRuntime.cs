//
// System.Web.Hosting.ISAPIRuntime.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//   Gonzalo Paniagua (gonzalo@ximian.com)
//
// (C) Bob Smith
// (c) 2002 Ximian, Inc. (http://www.ximian.com)
//

using System;

namespace System.Web.Hosting
{
        public sealed class ISAPIRuntime : IISAPIRuntime
        {
		[MonoTODO]
                public ISAPIRuntime ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
                public void DoGCCollect ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
                public int ProcessRequest (IntPtr ecb, int iWRType)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
                public void StartProcessing ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
                public void StopProcessing ()
		{
			throw new NotImplementedException ();
		}
        }
}
