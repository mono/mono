//
// System.Web.Hosting.IISAPIRuntime.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
//

using System;

namespace System.Web.Hosting
{
        public interface IISAPIRuntime
        {
                void DoGCCollect();
                int ProcessRequest(in IntPtr ecb, in int useProcessModel);
                void StartProcessing();
                void StopProcessing();
        }
}
