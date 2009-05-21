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
using System.Reflection;
using System.Configuration;

namespace RabbitMQ.Client
{
    ///<summary>Concrete, predefined IProtocol instances ready for use
    ///with ConnectionFactory.</summary>
    ///<remarks>
    ///<para>
    /// Applications will in the common case use the FromEnvironment()
    /// method to search a fallback-chain of configuration sources for
    /// the IProtocol instance to use. However, in some cases, the
    /// default fallback-chain is not appropriate; in these cases,
    /// other methods such as FromConfiguration(string) or
    /// SafeLookup(string) may suffice.
    ///</para>
    ///</remarks>
    public class Protocols
    {
        // Hide the constructor - no instances of Protocols needed.
        // We'd make this class static, but for MS's .NET 1.1 compilers
        private Protocols() {}

        ///<summary>The default App.config appSettings key used by
        ///FromConfiguration and FromEnvironment. At the time of
        ///writing, "AMQP_PROTOCOL".</summary>
        public readonly static string DefaultAppSettingsKey = "AMQP_PROTOCOL";

        ///<summary>The environment variable read by
        ///FromEnvironmentVariable() and FromEnvironment(). At the
        ///time of writing, "AMQP_PROTOCOL".</summary>
        public readonly static string EnvironmentVariable = "AMQP_PROTOCOL";

        ///<summary>Protocol version 0-8 as standardised.</summary>
        public static IProtocol AMQP_0_8
        {
            get { return new RabbitMQ.Client.Framing.v0_8.Protocol(); }
        }
        ///<summary>Protocol version 0-8, as modified by QPid.</summary>
        public static IProtocol AMQP_0_8_QPID
        {
            get { return new RabbitMQ.Client.Framing.v0_8qpid.Protocol(); }
        }
        ///<summary>Protocol version 0-9 as standardised (omitting
        ///sections marked "WIP", "work in progress", including in
        ///particular the Message class of operations).</summary>
        public static IProtocol AMQP_0_9
        {
            get { return new RabbitMQ.Client.Framing.v0_9.Protocol(); }
        }

        ///<summary>Retrieve the current default protocol variant
        ///(currently AMQP_0_8)</summary>
        public static IProtocol DefaultProtocol
        {
            get { return AMQP_0_8; }
        }

        ///<summary>Low-level method for retrieving a protocol version
        ///by name (of one of the static properties on this
        ///class)</summary>
        ///<remarks>
        ///<para>
        /// Returns null if no suitable property could be found.
        ///</para>
        ///<para>
        /// In many cases, FromEnvironment() will be a more
        /// appropriate method for applications to call; this method
        /// is provided for cases where the caller wishes to know the
        /// answer to the question "does a suitable IProtocol property
        /// with this name exist, and if so, what is its value?"
        ///</para>
        ///</remarks>
        public static IProtocol Lookup(string name)
        {
            PropertyInfo pi = typeof(Protocols).GetProperty(name,
                                                            BindingFlags.Public |
                                                            BindingFlags.Static);
            if (pi == null)
            {
                return null;
            }
            return pi.GetValue(null, new object[0]) as IProtocol;
        }

        ///<summary>Retrieve a protocol version by name (of one of the
        ///static properties on this class)</summary>
        ///<remarks>
        ///<para>
        /// If the argument is null, Protocols.DefaultProtocol is
        /// used. If the protocol variant named is not found,
        /// ConfigurationException is thrown.
        ///</para>
        ///<para>
        /// In many cases, FromEnvironment() will be a more
        /// appropriate method for applications to call; this method
        /// is provided for cases where the caller wishes to know the
        /// answer to the question "does a suitable IProtocol property
        /// with this name exist, and if so, what is its value?", with
        /// the additional guarantee that if a suitable property does
        /// not exist, a ConfigurationException will be thrown.
        ///</para>
        ///</remarks>
        ///<exception cref="ConfigurationException"/>
        public static IProtocol SafeLookup(string name)
        {
            if (name != null)
            {
                IProtocol p = Lookup(name);
                if (p != null)
                {
                    return p;
                }
                else
                {
                    throw new ConfigurationException("Unsupported protocol variant name: " + name);
                }
            }
            return DefaultProtocol;
        }

        private static string ReadEnvironmentVariable()
        {
            return Environment.GetEnvironmentVariable(EnvironmentVariable);
        }

        ///<summary>Uses the process environment variable
        ///<code>EnvironmentVariable</code> to retrieve an IProtocol
        ///instance.</summary>
        ///<remarks>
        ///If the environment variable is unset,
        ///Protocols.DefaultProtocol is used. If the protocol variant
        ///named is not found, ConfigurationException is thrown.
        ///</remarks>
        ///<exception cref="ConfigurationException"/>
        public static IProtocol FromEnvironmentVariable()
        {
            return SafeLookup(ReadEnvironmentVariable());
        }

        ///<summary>Uses App.config's appSettings section to retrieve
        ///an IProtocol instance.</summary>
        ///<remarks>
        ///If the appSettings key is missing,
        ///Protocols.DefaultProtocol is used. If the protocol variant
        ///named is not found, ConfigurationException is thrown.
        ///</remarks>
        ///<exception cref="ConfigurationException"/>
        public static IProtocol FromConfiguration(string appSettingsKey)
        {
            // FIXME: ConfigurationSettings.AppSettings is
            // obsolete. Use ConfigurationManager.AppSettings once we
            // decide that supporting .NET 1.1 is no longer required.
            string name = ConfigurationSettings.AppSettings[appSettingsKey];
            return SafeLookup(name);
        }

        ///<summary>Returns FromConfiguration(DefaultAppSettingsKey).</summary>
        public static IProtocol FromConfiguration()
        {
            return FromConfiguration(DefaultAppSettingsKey);
        }

        ///<summary>Tries FromConfiguration() first, followed by
        ///FromEnvironmentVariable() if no setting was found in the
        ///App.config.</summary>
        ///<exception cref="ConfigurationException"/>
        public static IProtocol FromEnvironment(string appSettingsKey)
        {
            // FIXME: ConfigurationSettings.AppSettings is
            // obsolete. Use ConfigurationManager.AppSettings once we
            // decide that supporting .NET 1.1 is no longer required.
            string name = ConfigurationSettings.AppSettings[appSettingsKey];
            if (name == null) {
                name = ReadEnvironmentVariable();
            }
            return SafeLookup(name);
        }

        ///<summary>Returns FromEnvironment(DefaultAppSettingsKey).</summary>
        public static IProtocol FromEnvironment()
        {
            return FromEnvironment(DefaultAppSettingsKey);
        }
    }
}
