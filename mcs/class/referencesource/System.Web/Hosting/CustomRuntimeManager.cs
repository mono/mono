//------------------------------------------------------------------------------
// <copyright file="CustomRuntimeSuspendManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Hosting {
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Web.Util;

    // Handles calling suspend and resume methods, including issues around
    // synchronization and timeout handling.

    internal sealed class CustomRuntimeManager : ICustomRuntimeManager {

        private readonly ConcurrentDictionary<CustomRuntimeRegistration, object> _activeRegistrations = new ConcurrentDictionary<CustomRuntimeRegistration, object>();

        private List<IProcessSuspendListener> GetAllSuspendListeners() {
            List<IProcessSuspendListener> suspendListeners = new List<IProcessSuspendListener>();

            foreach (var registration in _activeRegistrations.Keys) {
                var suspendListener = registration.CustomRuntime as IProcessSuspendListener;
                if (suspendListener != null) {
                    suspendListeners.Add(suspendListener);
                }
            }

            return suspendListeners;
        }

        public ICustomRuntimeRegistrationToken Register(ICustomRuntime customRuntime) {
            CustomRuntimeRegistration registration = new CustomRuntimeRegistration(this, customRuntime);
            _activeRegistrations[registration] = null;
            return registration;
        }

        // Custom runtimes are expected to be well-behaved so will be suspended sequentially and aren't
        // subject to the 5-second timeout that normal applications are subject to.
        public Action SuspendAllCustomRuntimes() {
            var suspendListeners = GetAllSuspendListeners();
            if (suspendListeners == null || suspendListeners.Count == 0) {
                return null;
            }

            List<IProcessResumeCallback> callbacks = new List<IProcessResumeCallback>(suspendListeners.Count);
            foreach (var suspendListener in suspendListeners) {
                IProcessResumeCallback callback = null;
                try {
                    callback = suspendListener.Suspend();
                }
                catch (AppDomainUnloadedException) {
                    // There exists a race condition where a custom runtime could have been stopped (unloaded)
                    // while a call to Suspend is in progress, so AD unloads may leak out. Don't treat this
                    // as a failure; just move on.
                }

                if (callback != null) {
                    callbacks.Add(callback);
                }
            }

            return () => {
                foreach (var callback in callbacks) {
                    try {
                        callback.Resume();
                    }
                    catch (AppDomainUnloadedException) {
                        // There exists a race condition where a custom runtime could have been stopped (unloaded)
                        // while a call to Suspend is in progress, so AD unloads may leak out. Don't treat this
                        // as a failure; just move on.
                    }
                }
            };
        }

        private sealed class CustomRuntimeRegistration : ICustomRuntimeRegistrationToken {
            private readonly CustomRuntimeManager _customRuntimeManager;

            public CustomRuntimeRegistration(CustomRuntimeManager customRuntimeManager, ICustomRuntime customRuntime) {
                _customRuntimeManager = customRuntimeManager;
                CustomRuntime = customRuntime;
            }

            public ICustomRuntime CustomRuntime { get; private set; }

            public void Unregister() {
                object dummy;
                bool removed = _customRuntimeManager._activeRegistrations.TryRemove(this, out dummy);

                Debug.Assert(removed, "Entry did not exist in the dictionary; was it removed twice?");
            }
        }
    }
}
