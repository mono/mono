//
// System.Runtime.Remoting.Messaging.IMessageSink.cs
//
// Author:
//   Piers Haken (piersh@friskit.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Remoting;

namespace System.Runtime.Remoting.Messaging
{
    public interface IMessageSink
    {
        IMessage SyncProcessMessage (IMessage msg);
        IMessageCtrl AsyncProcessMessage (IMessage msg, IMessageSink replySink);
        IMessageSink NextSink { get; }
    }
}

