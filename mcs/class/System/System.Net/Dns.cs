// System.Net.Dns.cs
//
// Author: Mads Pultz (mpultz@diku.dk)
// Author: Lawrence Pit (loz@cable.a2000.nl)
//
// (C) Mads Pultz, 2001

using System;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;

namespace System.Net {
        public sealed class Dns {

		private Dns () {}

                private delegate IPHostEntry GetHostByNameCallback (string hostName);
                private delegate IPHostEntry ResolveCallback (string hostName);
                
                public static IAsyncResult BeginGetHostByName (string hostName,
                	AsyncCallback requestCallback, object stateObject)
               	{
                        if (hostName == null)
                                throw new ArgumentNullException();
			GetHostByNameCallback c = new GetHostByNameCallback (GetHostByName);
			return c.BeginInvoke (hostName, requestCallback, stateObject);
                }

                public static IAsyncResult BeginResolve (string hostName,
                	AsyncCallback requestCallback, object stateObject)
                {
                        if (hostName == null)
                                throw new ArgumentNullException();
			ResolveCallback c = new ResolveCallback (Resolve);
			return c.BeginInvoke (hostName, requestCallback, stateObject);
                }
                
                public static IPHostEntry EndGetHostByName (IAsyncResult asyncResult) {
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");
			AsyncResult async = (AsyncResult) asyncResult;
			GetHostByNameCallback cb = (GetHostByNameCallback) async.AsyncDelegate;
			asyncResult.AsyncWaitHandle.WaitOne ();
			return cb.EndInvoke(asyncResult);
                }

                public static IPHostEntry EndResolve (IAsyncResult asyncResult) {
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");
			AsyncResult async = (AsyncResult) asyncResult;
			ResolveCallback cb = (ResolveCallback) async.AsyncDelegate;
			asyncResult.AsyncWaitHandle.WaitOne ();
			return cb.EndInvoke(asyncResult);
                }
                                
                [MethodImplAttribute(MethodImplOptions.InternalCall)]
                private extern static bool GetHostByName_internal(string host, out string h_name, out string[] h_aliases, out string[] h_addr_list);

                [MethodImplAttribute(MethodImplOptions.InternalCall)]
                private extern static bool GetHostByAddr_internal(string addr, out string h_name, out string[] h_aliases, out string[] h_addr_list);
                
                [MethodImplAttribute(MethodImplOptions.InternalCall)]
                private extern static bool GetHostName_internal(out string h_name);
		
                private static IPHostEntry hostent_to_IPHostEntry(string h_name, string[] h_aliases, string[] h_addrlist) {
                        IPHostEntry he = new IPHostEntry();
                        IPAddress[] addrlist = new IPAddress[h_addrlist.Length];
                        
                        he.HostName=h_name;
                        he.Aliases=h_aliases;
                        for(int i=0; i<h_addrlist.Length; i++) {
                                addrlist[i]=IPAddress.Parse(h_addrlist[i]);
                        }
                        he.AddressList=addrlist;

                        return(he);
                }

                public static IPHostEntry GetHostByAddress(IPAddress address) {
                        if (address == null)
                                throw new ArgumentNullException();
                        return GetHostByAddress(address.ToString());
                }
                
                public static IPHostEntry GetHostByAddress(string address) {
                        if (address == null) {
                                throw new ArgumentNullException();
                        }

						// Undocumented MS behavior: when called with IF_ANY,
						// this should return the local host
						if (address.Equals ("0.0.0.0"))
							return GetHostByAddress ("127.0.0.1");

                        string h_name;
                        string[] h_aliases, h_addrlist;
                        
                        bool ret = GetHostByAddr_internal(address, out h_name,
                                                          out h_aliases,
                                                          out h_addrlist);
                        if (ret == false) {
                                throw new SocketException(11001);
                        }
                        
                        return(hostent_to_IPHostEntry(h_name, h_aliases,
                                                      h_addrlist));
                }

                public static IPHostEntry GetHostByName(string hostName) {
                        if (hostName == null)
                                throw new ArgumentNullException();

                        string h_name;
                        string[] h_aliases, h_addrlist;
                        
                        bool ret = GetHostByName_internal(hostName, out h_name,
                                                          out h_aliases,
                                                          out h_addrlist);
                        if (ret == false)
                                throw new SocketException(11001);

                        return(hostent_to_IPHostEntry(h_name, h_aliases,
                                                      h_addrlist));
                }
                
                /// <summary>
                /// This method returns the host name associated with the local host.
                /// </summary>
                public static string GetHostName() {
                        string hostName;
                        
                        bool ret = GetHostName_internal(out hostName);
                        
			if (ret == false)
                                throw new SocketException(11001);

                        return hostName;
                }
                
                /// <summary>
                /// This method resolves a DNS-style host name or IP
                /// address.
                /// </summary>
                /// <param name=hostName>
                /// A string containing either a DNS-style host name (e.g.
                /// www.go-mono.com) or IP address (e.g. 129.250.184.233).
                /// </param>
                public static IPHostEntry Resolve(string hostName) {
                        if (hostName == null)
                                throw new ArgumentNullException();

			return GetHostByName (hostName);
                }
        }
}

