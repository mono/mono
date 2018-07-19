//------------------------------------------------------------------------------
// <copyright file="ISuspendibleRegisteredObject.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Hosting {
    using System;
    using System.Security.Permissions;

    // If the application passes HostingEnvironment.RegisterObject
    // an object which also implements this interface, that object
    // will be subscribed to listening for suspend / resume notifications.
    public interface ISuspendibleRegisteredObject : IRegisteredObject {

        // Called when ASP.NET needs to notify the application that the process is
        // being suspended. This method *must not* throw, otherwise the behavior is
        // undefined (we will probably terminate the process). This method *should
        // not* block, as after 5 seconds ASP.NET will allow rude process
        // suspension, even if not all Suspend methods have run to completion.
        //
        // If a callback is returned, it will be invoked when the process resumes
        // from suspension. The Suspend method may return null if it does not
        // wish to be notified when the process resumes.
        //
        // The Suspend method is currently only supported when running in
        // the IIS integrated mode pipeline. The ASP.NET runtime does not guarantee
        // that the Suspend method will ever fire.
        //
        // * THREAD SAFETY NOTE *
        // The Suspend method can be called at any time, including while a call
        // to another method (like IRegisteredObject.Stop) on this same object is
        // taking place, while calls to other objects' Suspend methods are taking
        // place, or even while a call to this object's Suspend method is taking
        // place. Additionally, due to the multithreaded nature of execution,
        // there exists a window in which the Suspend method might be called even
        // after a call to HostingEnvironment.UnregisterObject has completed.
        Action Suspend();

    }
}
