//------------------------------------------------------------------------------
// <copyright file="IStopListeningRegisteredObject.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Hosting {
    using System;

    // An interface that can be additionally implemented by an object passed
    // to HostingEnvironment.RegisterObject that can listen for
    // GL_STOP_LISTENING notifications from IIS.
    public interface IStopListeningRegisteredObject : IRegisteredObject {

        // Called when ASP.NET receives a GL_STOP_LISTENING notification from IIS,
        // signaling that IIS is no longer listening for new requests for this
        // application. The web server may allow in-flight requests to run to
        // completion. Applications which process long-running requests may wish
        // to listen for these notifications so that they may gracefully wind down
        // these requests. Contrast this method with IRegisteredObject.Stop, which
        // signals immediate application shutdown.
        //
        // The StopListening method is currently only supported when running in
        // the IIS integrated mode pipeline. The ASP.NET runtime does not guarantee
        // that the StopListening method will ever fire.
        //
        // This method *must not* throw, otherwise the behavior is undefined (we
        // will probably terminate the process). This method *should not* block,
        // otherwise deadlocks could occur.
        //
        // * THREAD SAFETY NOTE *
        // The StopListening method can be called at any time, including while a call
        // to another method (like IRegisteredObject.Stop) on this same object is
        // taking place or while calls to other objects' StopListening methods are taking
        // place. Additionally, due to the multithreaded nature of execution,
        // there exists a window in which the StopListening method might be called even
        // after a call to HostingEnvironment.UnregisterObject has completed.
        void StopListening();

    }
}
