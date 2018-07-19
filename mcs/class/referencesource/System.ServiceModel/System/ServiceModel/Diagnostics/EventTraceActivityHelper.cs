// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal static class EventTraceActivityHelper
    {
        public static bool TryAttachActivity(Message message, EventTraceActivity activity)
        {
            if (FxTrace.Trace.IsEnd2EndActivityTracingEnabled && (message != null) && (activity != null))
            {
                if (!message.Properties.ContainsKey(EventTraceActivity.Name))
                {
                    message.Properties.Add(EventTraceActivity.Name, activity);
                    return true;
                }
            }

            return false;
        }

        public static EventTraceActivity TryExtractActivity(Message message)
        {
            return TryExtractActivity(message, false);
        }

        public static EventTraceActivity TryExtractActivity(Message message, bool createIfNotExist)
        {
            EventTraceActivity eventTraceActivity = null;

            if (message != null && message.State != MessageState.Closed)
            {
                object property;
                if (message.Properties.TryGetValue(EventTraceActivity.Name, out property))
                {
                    eventTraceActivity = property as EventTraceActivity;
                }

                if (eventTraceActivity == null)
                {
                    Guid activityId;
                    if (GetMessageId(message, out activityId))
                    {
                        eventTraceActivity = new EventTraceActivity(activityId);
                    }
                    else
                    {
                        UniqueId uid = message.Headers.RelatesTo;
                        if (uid != null)
                        {
                            if (uid.TryGetGuid(out activityId))
                            {
                                eventTraceActivity = new EventTraceActivity(activityId);
                            }
                        }
                    }

                    if (eventTraceActivity == null && createIfNotExist)
                    {
                        eventTraceActivity = new EventTraceActivity();
                    }

                    if (eventTraceActivity != null)
                    {
                        // Attach the trace activity to the message
                        message.Properties[EventTraceActivity.Name] = eventTraceActivity;
                    }
                }
            }

            return eventTraceActivity;
        }

        [Fx.Tag.SecurityNote(Critical = "This sets the ActivityId on the thread. Must not be settable from PT code unless from safe context.")]
        [SecurityCritical]
        [SecurityPermission(SecurityAction.Assert, Unrestricted = true)]
        internal static void SetOnThread(EventTraceActivity eventTraceActivity)
        {
            if (eventTraceActivity != null)
            {
                Trace.CorrelationManager.ActivityId = eventTraceActivity.ActivityId;
            }
        }

        private static bool GetMessageId(Message message, out Guid guid)
        {
            UniqueId uniqueId = message.Headers.MessageId;
            if (uniqueId == null)
            {
                guid = Guid.Empty;
                return false;
            }

            return uniqueId.TryGetGuid(out guid);
        }
    }
}
