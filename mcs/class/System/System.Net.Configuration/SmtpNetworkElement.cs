//
// System.Net.Configuration.SmtpNetworkElement
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc. (http://www.novell.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if NET_2_0 && CONFIGURATION_DEP

using System;
using System.Configuration;

namespace System.Net.Configuration {

        public sealed class SmtpNetworkElement : ConfigurationElement
        {
                static ConfigurationProperty defaultCredentialsProp;
                static ConfigurationProperty hostProp;
                static ConfigurationProperty passwordProp;
                static ConfigurationProperty portProp;
                static ConfigurationProperty userNameProp;
                static ConfigurationPropertyCollection properties;

                static SmtpNetworkElement ()
                {
                        defaultCredentialsProp = new ConfigurationProperty ("defaultCredentials", typeof (bool), false);
                        hostProp = new ConfigurationProperty ("host", typeof (string));
                        passwordProp = new ConfigurationProperty ("password", typeof (string));
                        portProp = new ConfigurationProperty ("port", typeof (int), 25);
                        userNameProp = new ConfigurationProperty ("userName", typeof (string));
                        properties = new ConfigurationPropertyCollection ();

                        properties.Add (defaultCredentialsProp);
                        properties.Add (hostProp);
                        properties.Add (passwordProp);
                        properties.Add (portProp);
                        properties.Add (userNameProp);

                }

                protected override void PostDeserialize ()
                {
                }

                [ConfigurationProperty ("defaultCredentials", DefaultValue = "False")]
                public bool DefaultCredentials {
                        get { return (bool) base [defaultCredentialsProp];}
                        set { base[defaultCredentialsProp] = value; }
                }

                [ConfigurationProperty ("host")]
                public string Host {
                        get { return (string) base [hostProp];}
                        set { base[hostProp] = value; }
                }

                [ConfigurationProperty ("password")]
                public string Password {
                        get { return (string) base [passwordProp];}
                        set { base[passwordProp] = value; }
                }

                [ConfigurationProperty ("port", DefaultValue = "25")]
                public int Port {
                        get { return (int) base [portProp];}
                        set { base[portProp] = value; }
                }

                [ConfigurationProperty ("userName")]
                public string UserName {
                        get { return (string) base [userNameProp];}
                        set { base[userNameProp] = value; }
                }

                protected override ConfigurationPropertyCollection Properties {
                        get { return properties; }
                }

        }

}

#endif
