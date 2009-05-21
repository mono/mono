// This source code is dual-licensed under the Apache License, version
// 2.0, and the Mozilla Public License, version 1.1.
//
// The APL v2.0:
//
//---------------------------------------------------------------------------
//   Copyright (C) 2007-2009 LShift Ltd., Cohesive Financial
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
//   The Initial Developers of the Original Code are LShift Ltd,
//   Cohesive Financial Technologies LLC, and Rabbit Technologies Ltd.
//
//   Portions created before 22-Nov-2008 00:00:00 GMT by LShift Ltd,
//   Cohesive Financial Technologies LLC, or Rabbit Technologies Ltd
//   are Copyright (C) 2007-2008 LShift Ltd, Cohesive Financial
//   Technologies LLC, and Rabbit Technologies Ltd.
//
//   Portions created by LShift Ltd are Copyright (C) 2007-2009 LShift
//   Ltd. Portions created by Cohesive Financial Technologies LLC are
//   Copyright (C) 2007-2009 Cohesive Financial Technologies
//   LLC. Portions created by Rabbit Technologies Ltd are Copyright
//   (C) 2007-2009 Rabbit Technologies Ltd.
//
//   All Rights Reserved.
//
//   Contributor(s): ______________________________________.
//
//---------------------------------------------------------------------------
using System;
using System.Threading;
using System.Collections;

using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Util;

namespace RabbitMQ.Client.Impl
{
    public class SessionManager
    {
        private readonly Hashtable m_sessionMap = new Hashtable();
        private readonly ConnectionBase m_connection;
        private ushort m_channelMax = 0;
        private bool m_autoClose = false;

        public SessionManager(ConnectionBase connection)
        {
            m_connection = connection;
        }

        public ushort ChannelMax
        {
            get
            {
                return m_channelMax;
            }
            set
            {
                if ((value < 2) && (value != 0))
                {
                    // We currently interpret channel max as
                    // *including* channel zero. We also think it
                    // doesn't make sense to forbid opening of any
                    // real usable channels, so by our ch0-including
                    // assumption, the minimum *useful* value for
                    // channel max is 2.
                    //
                    // This code is here to work around OpenAMQ
                    // 1.2c4's channel max setting of 1.
                    //
                    // FIXME: warning? or is there something more
                    // sensible we can do here?
                    m_channelMax = 2;
                }
                else
                {
                    m_channelMax = value;
                }
            }
        }

        public bool AutoClose
        {
            get
            {
                return m_autoClose;
            }
            set
            {
                m_autoClose = value;
                CheckAutoClose();
            }
        }

        public int Count
        {
            get
            {
                return m_sessionMap.Count;
            }
        }

        public ISession Lookup(int number)
        {
            lock (m_sessionMap)
            {
                return (ISession) m_sessionMap[number];
            }
        }

        public ISession Create()
        {
            lock (m_sessionMap)
            {
                int channelNumber = Allocate();
                if (channelNumber == -1)
                {
                    throw new ChannelAllocationException();
                }
                return Create(channelNumber);
            }
        }

        public ISession Create(int channelNumber)
        {
            ISession session;
            lock (m_sessionMap)
            {
                if (m_sessionMap.ContainsKey(channelNumber))
                {
                    throw new ChannelAllocationException(channelNumber);
                }
                session = new Session(m_connection, channelNumber);
                session.SessionShutdown += new SessionShutdownEventHandler(HandleSessionShutdown);
                //Console.WriteLine("SessionManager adding session "+session);
                m_sessionMap[channelNumber] = session;
            }
            return session;
        }

        ///<summary>Replace an active session slot with a new ISession
        ///implementation. Used during channel quiescing.</summary>
        ///<remarks>
        /// Make sure you pass in a channelNumber that's currently in
        /// use, as if the slot is unused, you'll get a null pointer
        /// exception.
        ///</remarks>
        public ISession Swap(int channelNumber, ISession replacement) {
            lock (m_sessionMap)
            {
                ISession previous = (ISession) m_sessionMap[channelNumber];
                previous.SessionShutdown -= new SessionShutdownEventHandler(HandleSessionShutdown);
                m_sessionMap[channelNumber] = replacement;
                replacement.SessionShutdown += new SessionShutdownEventHandler(HandleSessionShutdown);
                return previous;
            }
        }

        ///<summary>Find an unused channel number. Must be called
        ///while holding m_sessionMap lock!</summary>
        ///<remarks>
        /// Returns -1 if no unused channel numbers are available.
        ///</remarks>
        public int Allocate()
        {
            ushort maxChannels = (m_channelMax == 0) ? ushort.MaxValue : m_channelMax;
            for (int candidate = 1; candidate < maxChannels; candidate++)
            {
                if (!m_sessionMap.ContainsKey(candidate))
                {
                    return candidate;
                }
            }
            return -1;
        }

        public void HandleSessionShutdown(ISession session, ShutdownEventArgs reason)
        {
            //Console.WriteLine("SessionManager removing session "+session);
            lock (m_sessionMap)
            {
                m_sessionMap.Remove(session.ChannelNumber);
                CheckAutoClose();
            }
        }

        ///<summary>If m_autoClose and there are no active sessions
        ///remaining, Close()s the connection with reason code
        ///200.</summary>
        public void CheckAutoClose()
        {
            if (m_autoClose)
            {
                lock (m_sessionMap)
                {
                    if (m_sessionMap.Count == 0)
                    {
                        // Run this in a background thread, because
                        // usually CheckAutoClose will be called from
                        // HandleSessionShutdown above, which runs in
                        // the thread of the connection. If we were to
                        // attempt to close the connection from within
                        // the connection's thread, we would suffer a
                        // deadlock as the connection thread would be
                        // blocking waiting for its own mainloop to
                        // reply to it.
                        new Thread(new ThreadStart(AutoCloseConnection)).Start();
                    }
                }
            }
        }

        ///<summary>Called from CheckAutoClose, in a separate thread,
        ///when we decide to close the connection.</summary>
        public void AutoCloseConnection()
        {
            m_connection.Abort(200, "AutoClose", ShutdownInitiator.Library, Timeout.Infinite);
        }
    }
}
