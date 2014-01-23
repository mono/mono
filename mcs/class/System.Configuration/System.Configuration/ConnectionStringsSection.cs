//
// System.Configuration.ConnectionStringsSection.cs
//
// Author:
//      Sureshkumar T <tsureshkumar@novell.com>
//      Duncan Mak (duncan@ximian.com)
//
//
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System;
using System.Xml;

namespace System.Configuration
{
        public sealed class ConnectionStringsSection : ConfigurationSection
        {
                private static readonly ConfigurationProperty _propConnectionStrings;
                private static ConfigurationPropertyCollection _properties;

                static ConnectionStringsSection ()
                {
                        _propConnectionStrings = new ConfigurationProperty ("", typeof (ConnectionStringSettingsCollection), 
                                                                            null, ConfigurationPropertyOptions.IsDefaultCollection);
                        _properties = new ConfigurationPropertyCollection ();
                        _properties.Add (_propConnectionStrings);
                }

                public ConnectionStringsSection ()
                {
                }


		[ConfigurationProperty ("", Options = ConfigurationPropertyOptions.IsDefaultCollection)]
                public ConnectionStringSettingsCollection ConnectionStrings
                {
			get { return (ConnectionStringSettingsCollection) base [_propConnectionStrings]; }
                }

                protected internal override ConfigurationPropertyCollection Properties
                {
                        get { return _properties; }
                }

            
                protected internal override object GetRuntimeObject ()
                {
                        return base.GetRuntimeObject ();
                }
        }

}
