//
// System.Configuration.ConfigurationLocation.cs
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
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
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
#if NET_2_0
using System;

namespace System.Configuration {

        public sealed class Configuration
        {
                internal Configuration ()
                {
                }

                public AppSettingsSection AppSettings {
                        get { throw new NotImplementedException (); }
                }

                public PathLevel ConfigurationPathLevel {
                        get { throw new NotImplementedException (); }
                }

                public ConnectionStringsSection ConnectionStrings {
                        get { throw new NotImplementedException (); }
                }

                public string FilePath {
                        get { throw new NotImplementedException (); }
                }

                public bool HasFile {
                        get { throw new NotImplementedException (); }
                }

                public ConfigurationLocationCollection Locations {
                        get { throw new NotImplementedException (); }
                }

                public string Path {
                        get { throw new NotImplementedException (); }
                }

                public ConfigurationSectionGroup RootSectionGroup {
                        get { throw new NotImplementedException (); }                        
                }

                public ConfigurationSectionGroupCollection SectionGroups {
                        get { throw new NotImplementedException (); }                        
                }

                public ConfigurationSectionCollection Sections {
                        get { throw new NotImplementedException (); }                        
                }

                public static Configuration GetExeConfiguration (string path, ConfigurationUserLevel level)
                {
                        throw new NotImplementedException ();
                }

                public static Configuration GetMachineConfiguration ()
                {
                        throw new NotImplementedException ();
                }

                public static Configuration GetMachineConfiguration (string path)
                {
                        throw new NotImplementedException ();
                }
                
                public static Configuration GetMachineConfiguration (string path, string server)
                {
                        throw new NotImplementedException ();
                }

                public static Configuration GetMachineConfiguration (
                                string path, string server, IntPtr user_token)
                {
                        throw new NotImplementedException ();
                }

                public static Configuration GetMachineConfiguration (
                                string path, string server, string username, string password)
                {
                        throw new NotImplementedException ();
                }

                public static Configuration GetWebConfiguration ()
                {
                        throw new NotImplementedException ();
                }

                public static Configuration GetWebConfiguration (string path)
                {
                        throw new NotImplementedException ();
                }

                public static Configuration GetWebConfiguration (string path, string site)
                {
                        throw new NotImplementedException ();
                }
                
                public static Configuration GetWebConfiguration (string path, string site, string subpath)
                {
                        throw new NotImplementedException ();
                }

                public static Configuration GetWebConfiguration (
                                string path, string site, string subpath, string server)
                {
                        throw new NotImplementedException ();
                }

                public static Configuration GetWebConfiguration (
                                string path, string site, string subpath, string server, IntPtr user_token)
                {
                        throw new NotImplementedException ();
                }

                public static Configuration GetWebConfiguration (
                                string path, string site, string subpath, string server, string username, string password)
                {
                        throw new NotImplementedException ();
                }
        }
}
#endif
