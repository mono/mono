// System.Net.Dns.cs
//
// Author: Mads Pultz (mpultz@diku.dk)
//
// (C) Mads Pultz, 2001

using System;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Threading;
using System.Runtime.CompilerServices;

namespace System.Net {
        public sealed class Dns {
                
                /// <summary>
                /// Helper class
                /// </summary>
                private sealed class DnsAsyncResult: IAsyncResult {
                        private object state;
                        private WaitHandle waitHandle;
                        private bool completedSync, completed;
                        private Worker worker;
                
                        public DnsAsyncResult(object state) {
                                this.state = state;
                                waitHandle = new ManualResetEvent(false);
                                completedSync = completed = false;
                        }       
                        public object AsyncState {
                                get { return state; }
                        }
                        public WaitHandle AsyncWaitHandle {
                                set { waitHandle = value; }
                                get { return waitHandle; }
                        }
                        public bool CompletedSynchronously {
                                get { return completedSync; }
                        }
                        public bool IsCompleted {
                                set { completed = value; }
                                get { return completed; }
                        }
                        public Worker Worker {
                                set { worker = value; }
                                get { return worker; }
                        }
                }

                /// <summary>
                /// Helper class for asynchronous calls to DNS server
                /// </summary>
                private sealed class Worker {
                        private AsyncCallback reqCallback;
                        private DnsAsyncResult reqRes;
                        private string req;
                        private IPHostEntry result;
                        
                        public Worker(string req, AsyncCallback reqCallback, DnsAsyncResult reqRes) {
                                this.req = req;
                                this.reqCallback = reqCallback;
                                this.reqRes = reqRes;
                        }
                        private void End() {
                                reqCallback(reqRes);
                                ((ManualResetEvent)reqRes.AsyncWaitHandle).Set();
                                reqRes.IsCompleted = true;
                        }
                        public void GetHostByName() {
                                lock(reqRes) {
                                        result = Dns.GetHostByName(req);
                                        End();
                                }
                        }
                        public void Resolve() {
                                lock(reqRes) {
                                        result = Dns.Resolve(req);
                                        End();
                                }
                        }
                        public IPHostEntry Result {
                                get { return result; }
                        }
                }
                
                public static IAsyncResult BeginGetHostByName(string hostName,
                	AsyncCallback requestCallback, object stateObject)
               	{
                        DnsAsyncResult requestResult = new DnsAsyncResult(stateObject);
                        Worker worker = new Worker(hostName, requestCallback, requestResult);
                        Thread child = new Thread(new ThreadStart(worker.GetHostByName));
                        child.Start();
                        return requestResult;
                }

                public static IAsyncResult BeginResolve(string hostName,
                	AsyncCallback requestCallback, object stateObject)
                {
                        DnsAsyncResult requestResult = new DnsAsyncResult(stateObject);
                        Worker worker = new Worker(hostName, requestCallback, requestResult);
                        Thread child = new Thread(new ThreadStart(worker.Resolve));
                        child.Start();
                        return requestResult;
                }
                
                public static IPHostEntry EndGetHostByName(IAsyncResult asyncResult) {
                        return ((DnsAsyncResult)asyncResult).Worker.Result;
                }

                public static IPHostEntry EndResolve(IAsyncResult asyncResult) {
                        return ((DnsAsyncResult)asyncResult).Worker.Result;
                }
                
                
                [MethodImplAttribute(MethodImplOptions.InternalCall)]
                private extern static bool GetHostByName_internal(string host, out string h_name, out string[] h_aliases, out string[] h_addr_list);
                [MethodImplAttribute(MethodImplOptions.InternalCall)]
                private extern static bool GetHostByAddr_internal(string addr, out string h_name, out string[] h_aliases, out string[] h_addr_list);
                
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
                        
                        string h_name;
                        string[] h_aliases, h_addrlist;
                        
                        bool ret = GetHostByAddr_internal(address, out h_name,
                                                          out h_aliases,
                                                          out h_addrlist);
                        if (ret == false) {
                                throw new SocketException();
                        }
                        
                        return(hostent_to_IPHostEntry(h_name, h_aliases,
                                                      h_addrlist));
                }

                public static IPHostEntry GetHostByName(string hostName) {
                        if (hostName == null) {
                                throw new ArgumentNullException();
                        }
                        
                        string h_name;
                        string[] h_aliases, h_addrlist;
                        
                        bool ret = GetHostByName_internal(hostName, out h_name,
                                                          out h_aliases,
                                                          out h_addrlist);
                        if (ret == false) {
                                throw new SocketException();
                        }

                        return(hostent_to_IPHostEntry(h_name, h_aliases,
                                                      h_addrlist));
                }
                
                /// <summary>
                /// This method returns the host name associated with the local host.
                /// </summary>
                public static string GetHostName() {
                        IPHostEntry h = GetHostByAddress("127.0.0.1");
                        return h.HostName;
                }
                
                /// <summary>
                /// This method resovles a DNS-style host name or IP
                /// address.
                /// </summary>
                /// <param name=hostName>
                /// A string containing either a DNS-style host name (e.g.
                /// www.go-mono.com) or IP address (e.g. 129.250.184.233).
                /// </param>
                public static IPHostEntry Resolve(string hostName) {
                        if (hostName == null)
                                throw new ArgumentNullException();
                        try {
                                return GetHostByAddress(hostName);
                        } catch (SocketException) {
                                return GetHostByName(hostName);
                        }
                }
        }
}

