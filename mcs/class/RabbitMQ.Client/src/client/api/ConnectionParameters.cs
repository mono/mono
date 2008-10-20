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

namespace RabbitMQ.Client
{
    ///<summary>Supplies values to ConnectionFactory for use in
    ///constructing IConnection instances.</summary>
    public class ConnectionParameters : ICloneable
    {
        /// <summary>Default user name (value: "guest")</summary>
        public const string DefaultUser = "guest"; // PLEASE KEEP THIS MATCHING THE DOC ABOVE

        /// <summary>Default password (value: "guest")</summary>
        public const string DefaultPass = "guest"; // PLEASE KEEP THIS MATCHING THE DOC ABOVE

        /// <summary>Default virtual host (value: "/")</summary>
        public const string DefaultVHost = "/"; // PLEASE KEEP THIS MATCHING THE DOC ABOVE

        /// <summary> Default value for the desired maximum number of
        /// channels, with zero meaning unlimited (value: 0)</summary>
        public const ushort DefaultChannelMax = 0; // PLEASE KEEP THIS MATCHING THE DOC ABOVE

        /// <summary>Default value for the desired maximum frame size,
        /// with zero meaning unlimited (value: 0)</summary>
        public const uint DefaultFrameMax = 0; // PLEASE KEEP THIS MATCHING THE DOC ABOVE

        /// <summary>Default value for desired heartbeat interval, in
        /// seconds, with zero meaning none (value: 3)</summary>
        public const ushort DefaultHeartbeat = 3; // PLEASE KEEP THIS MATCHING THE DOC ABOVE

        private string m_userName = DefaultUser;
        private string m_password = DefaultPass;
        private string m_virtualHost = DefaultVHost;
        private ushort m_requestedChannelMax = DefaultChannelMax;
        private uint m_requestedFrameMax = DefaultFrameMax;
        private ushort m_requestedHeartbeat = DefaultHeartbeat;
        private AccessRequestConfig m_accessRequestConfig = AccessRequestConfig.UseDefault;

        ///<summary>Construct a fresh instance, with all fields set to
        ///their respective defaults.</summary>
        public ConnectionParameters() { }

        /// <summary>Username to use when authenticating to the server</summary>
        public string UserName
        {
            get { return m_userName; }
            set { m_userName = value; }
        }

        /// <summary>Password to use when authenticating to the server</summary>
        public string Password
        {
            get { return m_password; }
            set { m_password = value; }
        }

        /// <summary>Virtual host to access during this connection</summary>
        public string VirtualHost
        {
            get { return m_virtualHost; }
            set { m_virtualHost = value; }
        }

        /// <summary>Channel-max parameter to ask for (number of channels)</summary>
        public ushort RequestedChannelMax
        {
            get { return m_requestedChannelMax; }
            set { m_requestedChannelMax = value; }
        }

        /// <summary>Frame-max parameter to ask for (in bytes)</summary>
        public uint RequestedFrameMax
        {
            get { return m_requestedFrameMax; }
            set { m_requestedFrameMax = value; }
        }

        /// <summary>Heartbeat setting to request (in seconds)</summary>
        public ushort RequestedHeartbeat
        {
            get { return m_requestedHeartbeat; }
            set { m_requestedHeartbeat = value; }
        }

        /// <summary>Used to control whether Access.Request methods
        /// are sent to the peer or not, in conjunction with the
        /// protocol default.</summary>
        public AccessRequestConfig AccessRequestConfig
        {
            get { return m_accessRequestConfig; }
            set { m_accessRequestConfig = value; }
        }

        ///<summary>Implement ICloneable.Clone by delegating to our type-safe variant.</summary>
        object ICloneable.Clone()
        {
            return ((ConnectionParameters)this).Clone();
        }

        ///<summary>Returns a fresh ConnectionParameters with the same values as this.</summary>
        public ConnectionParameters Clone()
        {
            ConnectionParameters n = this.MemberwiseClone() as ConnectionParameters;
            return n;
        }
    }
}
