// This source code is dual-licensed under the Apache License, version
// 2.0, and the Mozilla Public License, version 1.1.
//
// The APL v2.0:
//
//---------------------------------------------------------------------------
//   Copyright (C) 2007, 2008 LShift Ltd., Cohesive Financial
//   Technologies LLC., and Rabbit Technologies Ltd.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//---------------------------------------------------------------------------
//
// The MPL v1.1:
//
//---------------------------------------------------------------------------
//   The contents of this file are subject to the Mozilla Public License
//   Version 1.1 (the "License"); you may not use this file except in
//   compliance with the License. You may obtain a copy of the License at
//   http://www.rabbitmq.com/mpl.html
//
//   Software distributed under the License is distributed on an "AS IS"
//   basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
//   License for the specific language governing rights and limitations
//   under the License.
//
//   The Original Code is The RabbitMQ .NET Client.
//
//   The Initial Developers of the Original Code are LShift Ltd.,
//   Cohesive Financial Technologies LLC., and Rabbit Technologies Ltd.
//
//   Portions created by LShift Ltd., Cohesive Financial Technologies
//   LLC., and Rabbit Technologies Ltd. are Copyright (C) 2007, 2008
//   LShift Ltd., Cohesive Financial Technologies LLC., and Rabbit
//   Technologies Ltd.;
//
//   All Rights Reserved.
//
//   Contributor(s): ______________________________________.
//
//---------------------------------------------------------------------------
using System;
using System.Collections;
using RabbitMQ.Client.Events;

namespace RabbitMQ.Client
{
    ///<summary>Main interface to an AMQP connection.</summary>
    ///<remarks>
    ///<para>
    /// Instances of IConnection are used to create fresh
    /// sessions/channels. The ConnectionFactory class is used to
    /// construct IConnection instances. Please see the documentation
    /// for ConnectionFactory for an example of usage. Alternatively,
    /// an API tutorial can be found in the User Guide.
    ///</para>
    ///<para>
    /// Extends the IDisposable interface, so that the "using"
    /// statement can be used to scope the lifetime of a channel when
    /// appropriate.
    ///</para>
    ///</remarks>
    public interface IConnection: IDisposable
    {
        ///<summary>Raised when the connection is destroyed.</summary>
        ///<remarks>
        /// If the connection is already destroyed at the time an
        /// event handler is added to this event, the event handler
        /// will be fired immediately.
        ///</remarks>
        event ConnectionShutdownEventHandler ConnectionShutdown;

        ///<summary>Signalled when an exception occurs in a callback
        ///invoked by the connection.</summary>
        ///<remarks>
        ///This event is signalled when a ConnectionShutdown handler
        ///throws an exception. If, in future, more events appear on
        ///IConnection, then this event will be signalled whenever one
        ///of those event handlers throws an exception, as well.
        ///</remarks>
        event CallbackExceptionEventHandler CallbackException;

        ///<summary>Retrieve the endpoint this connection is connected
        ///to.</summary>
        AmqpTcpEndpoint Endpoint { get; }

        ///<summary>The IProtocol this connection is using to
        ///communicate with its peer.</summary>
        IProtocol Protocol { get; }

        ///<summary>The connection parameters used during construction
        ///of this connection.</summary>
        ConnectionParameters Parameters { get; }

        ///<summary>The maximum number of channels this connection
        ///supports (0 if unlimited).</summary>
        ushort ChannelMax { get; }

        ///<summary>The maximum frame size this connection supports (0
        ///if unlimited).</summary>
        uint FrameMax { get; }

        ///<summary>The current heartbeat setting for this connection
        ///(0 for disabled), in seconds.</summary>
        ushort Heartbeat { get; }

        ///<summary>Returns the known hosts that came back from the
        ///broker in the connection.open-ok method at connection
        ///startup time. Null until the connection is completely open
        ///and ready for use.</summary>
        AmqpTcpEndpoint[] KnownHosts { get; }

        ///<summary>Returns null if the connection is still in a state
        ///where it can be used, or the cause of its closure
        ///otherwise.</summary>
        ///<remarks>
        ///<para>
        /// Applications should use the ConnectionShutdown event to
        /// avoid race conditions. The scenario to avoid is checking
        /// CloseReason, seeing it is null (meaning the IConnection
        /// was available for use at the time of the check), and
        /// interpreting this mistakenly as a guarantee that the
        /// IConnection will remain usable for a time. Instead, the
        /// operation of interest should simply be attempted: if the
        /// IConnection is not in a usable state, an exception will be
        /// thrown (most likely OperationInterruptedException, but may
        /// vary depending on the particular operation being
        /// attempted).
        ///</para>
        ///</remarks>
        ShutdownEventArgs CloseReason { get; }

        ///<summary>Returns true if the connection is still in a state
        ///where it can be used. Identical to checking if CloseReason
        ///== null.</summary>
        bool IsOpen { get; }

        ///<summary>If true, will close the whole connection as soon
        ///as there are no channels open on it; if false, manual
        ///connection closure will be required.</summary>
        ///<remarks>
        /// Don't set AutoClose to true before opening the first
        /// channel, because the connection will be immediately closed
        /// if you do!
        ///</remarks>
        bool AutoClose { get; set; }

        ///<summary>Create and return a fresh channel, session, and
        ///model.</summary>
        IModel CreateModel();

        ///<summary>Close this connection and all its channels.</summary>
        ///<remarks>
        ///Note that all active channels, sessions, and models will be
        ///closed if this method is called. It will wait for the in-progress
        ///close operation to complete. This method will not return to the caller
        ///until the shutdown is complete. If the connection is already closed
        ///(or closing), then this method will throw AlreadyClosedException.
        ///It can also throw IOException when socket was closed unexpectedly.
        ///</remarks>
        void Close();
        
        ///<summary>Close this connection and all its channels
        ///and wait with a timeout for all the in-progress close operations
        ///to complete.
        ///</summary>
        ///<remarks>
        ///Note that all active channels, sessions, and models will be
        ///closed if this method is called. It will wait for the in-progress
        ///close operation to complete with a timeout. If the connection is 
        ///already closed (or closing), then this method will throw
        ///AlreadyClosedException.
        ///It can also throw IOException when socket was closed unexpectedly.
        ///If timeout is reached and the close operations haven't finished,
        ///then socket is forced to close.
        ///<para>
        ///To wait infinitely for the close operations to complete use
        ///Timeout.Infinite
        ///</para>
        ///</remarks>
        void Close(int timeout);
        
        ///<summary>Abort this connection and all its channels.</summary>
        ///<remarks>
        ///Note that all active channels, sessions, and models will be
        ///closed if this method is called.
        ///In comparison to normal Close() method, Abort() will not throw
        ///AlreadyClosedException or IOException during closing connection.
        ///This method waits infinitely for the in-progress close operation
        ///to complete.
        ///</remarks>
        void Abort();
        
        ///<summary>
        ///Abort this connection and all its channels and wait with a
        ///timeout for all the in-progress close operations to complete.
        ///.</summary>
        ///<remarks>
        ///This method, behaves in a similar way as method Abort() with the
        ///only difference that it explictly specifies the timeout given
        ///for all the in-progress close operations to complete.
        ///If timeout is reached and the close operations haven't finished,
        ///then socket is forced to close.
        ///<para>
        ///To wait infinitely for the close operations to complete use
        ///Timeout.Infinite
        ///</para>
        ///</remarks>
        void Abort(int timeout);
        
        ///<summary>Returns the list of ShutdownReportEntry objects that
        ///contain information about any errors reported while closing the
        ///connection in the order they appeared</summary>
        IList ShutdownReport { get; }
    }
}
