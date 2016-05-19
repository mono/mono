using System;

namespace System.Net
{
    internal interface IRequestLifetimeTracker
    {
        void TrackRequestLifetime(long requestStartTimestamp);
    }
}
