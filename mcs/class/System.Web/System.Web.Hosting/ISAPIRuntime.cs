//
// System.Web.Hosting.ISAPIRuntime.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
//

using System;

namespace System.Web.Hosting
{
        public sealed class ISAPIRuntime : IISAPIRuntime
        {
                public ISAPIRuntime();
                public void DoGCCollect();
                public int ProcessRequest(IntPtr ecb, int iWRType);
                public void StartProcessing();
                public void StopProcessing();
        }
}
