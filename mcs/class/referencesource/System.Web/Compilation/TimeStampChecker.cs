//------------------------------------------------------------------------------
// <copyright file="TimeStampChecker.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Compilation {

    using System;
    using System.Collections;
    using System.IO;
    using System.Web;
    using System.Web.Hosting;
    using Debug = System.Web.Util.Debug;

    internal class TimeStampChecker {
        internal const String CallContextSlotName = "TSC";

        private Hashtable _timeStamps = new Hashtable(StringComparer.OrdinalIgnoreCase);

        private static TimeStampChecker Current {
            get {
                TimeStampChecker tsc = (TimeStampChecker)System.Runtime.Remoting.Messaging.CallContext.GetData(
                    CallContextSlotName) as TimeStampChecker;

                // Create it on demand
                if (tsc == null) {
                    tsc = new TimeStampChecker();
                    Debug.Trace("TimeStampChecker", "Creating new TimeStampChecker");
                    System.Runtime.Remoting.Messaging.CallContext.SetData(CallContextSlotName, tsc);
                }

                return tsc;
            }
        }

        internal static void AddFile(string virtualPath, string path) {
            Current.AddFileInternal(virtualPath, path);
        }

        private void AddFileInternal(string virtualPath, string path) {
            DateTime lastWriteTimeUtc = File.GetLastWriteTimeUtc(path);

            if (_timeStamps.Contains(virtualPath)) {
                DateTime storedValue = (DateTime)_timeStamps[virtualPath];

                // Already found to have changed before
                if (storedValue == DateTime.MaxValue) {
                    Debug.Trace("TimeStampChecker", "AddFileInternal: Same time stamp (" + path + ")");
                    return;
                }

                // If it's different, set it to MaxValue as marker of being invalid
                if (storedValue != lastWriteTimeUtc) {
                    _timeStamps[virtualPath] = DateTime.MaxValue;
                    Debug.Trace("TimeStampChecker", "AddFileInternal: Changed time stamp (" + path + ")");
                }
            }
            else {
                // New path: just add it
                _timeStamps[virtualPath] = lastWriteTimeUtc;
                Debug.Trace("TimeStampChecker", "AddFileInternal: New path (" + path + ")");
            }
        }

        internal static bool CheckFilesStillValid(string key, ICollection virtualPaths) {
            if (virtualPaths == null)
                return true;

            return Current.CheckFilesStillValidInternal(key, virtualPaths);
        }

        private bool CheckFilesStillValidInternal(string key, ICollection virtualPaths) {
            Debug.Trace("TimeStampChecker", "CheckFilesStillValidInternal (" + key + ")");

            foreach (string virtualPath in virtualPaths) {

                if (!_timeStamps.Contains(virtualPath))
                    continue;

                string path = HostingEnvironment.MapPath(virtualPath);

                DateTime lastWriteTimeUtc = File.GetLastWriteTimeUtc(path);
                DateTime storedValue = (DateTime)_timeStamps[virtualPath];

                // If it changed, then it's not valid
                if (lastWriteTimeUtc != storedValue) {
                    Debug.Trace("TimeStampChecker", "CheckFilesStillValidInternal: File (" + path + ") has changed!");

                    return false;
                }
            }

            Debug.Trace("TimeStampChecker", "CheckFilesStillValidInternal (" + key + ") is still valid");
            return true;
        }
    }
}

