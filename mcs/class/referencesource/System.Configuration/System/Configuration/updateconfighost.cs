//------------------------------------------------------------------------------
// <copyright file="UpdateConfigHost.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using System.Collections.Specialized;
    using System.Configuration.Internal;
    using System.IO;

    //
    // Configuration host that intercepts calls to filename functions
    // to support SaveAs to an alternate file stream.
    //
    internal class UpdateConfigHost : DelegatingConfigHost {
        private HybridDictionary    _streams;       // oldStreamname -> StreamUpdate

        internal UpdateConfigHost(IInternalConfigHost host) {
            // Delegate to the host provided.
            Host = host;
        }

        // Add a stream to the list of streams to intercept.
        //
        // Parameters:
        //  alwaysIntercept -   If true, then don't check whether the old stream and the new stream are the same.
        //                      SaveAs() will set this to true if oldStreamname is actually referring to a stream
        //                      on a remote machine.
        internal void AddStreamname(string oldStreamname, string newStreamname, bool alwaysIntercept) {
            // 






            Debug.Assert(!String.IsNullOrEmpty(oldStreamname));
            
            if (String.IsNullOrEmpty(oldStreamname)) {
                return;
            }
            
            if (!alwaysIntercept && StringUtil.EqualsIgnoreCase(oldStreamname, newStreamname)) {
                return;
            }

            if (_streams == null) {
                _streams = new HybridDictionary(true);
            }

            _streams[oldStreamname] = new StreamUpdate(newStreamname);
        }

        // Get the new stream name for a stream if a new name exists, otherwise
        // return the original stream name.
        internal string GetNewStreamname(string oldStreamname) {
            StreamUpdate streamUpdate = GetStreamUpdate(oldStreamname, false);
            if (streamUpdate != null) {
                return streamUpdate.NewStreamname;
            }

            return oldStreamname;
        }

        //
        // Get the StreamUpdate for a stream.
        // If alwaysIntercept is true, then the StreamUpdate is 
        // always returned if it exists.
        // If alwaysIntercept is false, then only return the StreamUpdate
        // if the new stream has been successfully written to.
        //
        private StreamUpdate GetStreamUpdate(string oldStreamname, bool alwaysIntercept) {
            if (_streams == null)
                return null;

            StreamUpdate streamUpdate = (StreamUpdate) _streams[oldStreamname];
            if (streamUpdate != null && !alwaysIntercept && !streamUpdate.WriteCompleted) {
                streamUpdate = null;
            }

            return streamUpdate;
        }

        public override object GetStreamVersion(string streamName) {
            StreamUpdate streamUpdate = GetStreamUpdate(streamName, false);
            if (streamUpdate != null) {
                return InternalConfigHost.StaticGetStreamVersion(streamUpdate.NewStreamname);
            }
            else {
                return Host.GetStreamVersion(streamName);
            }
        }

        public override Stream OpenStreamForRead(string streamName) {
            StreamUpdate streamUpdate = GetStreamUpdate(streamName, false);
            if (streamUpdate != null) {
                return InternalConfigHost.StaticOpenStreamForRead(streamUpdate.NewStreamname);
            }
            else {
                return Host.OpenStreamForRead(streamName);
            }
        }

        public override Stream OpenStreamForWrite(string streamName, string templateStreamName, ref object writeContext) {
            // Always attempt to write to the new stream name if it exists.
            StreamUpdate streamUpdate = GetStreamUpdate(streamName, true);
            if (streamUpdate != null) {
                return InternalConfigHost.StaticOpenStreamForWrite(streamUpdate.NewStreamname, templateStreamName, ref writeContext, false);
            }
            else {
                return Host.OpenStreamForWrite(streamName, templateStreamName, ref writeContext);
            }
        }

        public override void WriteCompleted(string streamName, bool success, object writeContext) {
            StreamUpdate streamUpdate = GetStreamUpdate(streamName, true);
            if (streamUpdate != null) {
                InternalConfigHost.StaticWriteCompleted(streamUpdate.NewStreamname, success, writeContext, false);
                //
                // Mark the write as having successfully completed, so that subsequent calls 
                // to Read() will use the new stream name.
                //
                if (success) {
                    streamUpdate.WriteCompleted = true;
                }
            }
            else {
                Host.WriteCompleted(streamName, success, writeContext);
            }
        }

        public override bool IsConfigRecordRequired(string configPath) {
            return true;            
        }

        public override void DeleteStream(string streamName) {
            StreamUpdate streamUpdate = GetStreamUpdate(streamName, false);
            if (streamUpdate != null) {
                InternalConfigHost.StaticDeleteStream(streamUpdate.NewStreamname);
            }
            else {
                Host.DeleteStream(streamName);
            }
        }

        public override bool IsFile(string streamName) {
            StreamUpdate streamUpdate = GetStreamUpdate(streamName, false);
            if (streamUpdate != null) {
                return InternalConfigHost.StaticIsFile(streamUpdate.NewStreamname);
            }
            else {
                return Host.IsFile(streamName);
            }
        }
    }
}
