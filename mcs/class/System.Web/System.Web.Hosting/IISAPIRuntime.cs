//
// System.Web.Hosting.IISAPIRuntime.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
//

using System;
using System.Runtime.InteropServices;

namespace System.Web.Hosting
{
	[Guid ("c4918956-485b-3503-bd10-9083e3f6b66c")]
	[ComVisible (false)]
        public interface IISAPIRuntime
        {
                void DoGCCollect ();
                int ProcessRequest ([In] IntPtr ecb, [In] int useProcessModel);
                void StartProcessing ();
                void StopProcessing ();
        }
}
