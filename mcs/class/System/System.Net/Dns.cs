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
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;

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
		
		/// <summary>
		/// This class conforms to the C structure <c>hostent</c> and is used
		/// by the Dns class when doing native calls.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		private unsafe class Hostent {
			public string h_name;       /* official name */
			public byte** h_aliases;    /* alias list */
			public short h_addrtype;    /* address type */
			public short h_length;      /* address length */
			public byte** h_addr_list;  /* address list */
		}
		
		public static IAsyncResult BeginGetHostByName(string hostName,
                                                  AsyncCallback requestCallback,
                                                  object stateObject) {
                        DnsAsyncResult requestResult = new DnsAsyncResult(stateObject);
                        Worker worker = new Worker(hostName, requestCallback, requestResult);
			Thread child = new Thread(new ThreadStart(worker.GetHostByName));
                        child.Start();
                        return requestResult;
		}
		
		public static IAsyncResult BeginResolve(string hostName,
	                                        AsyncCallback requestCallback,
						object stateObject) {
			// TODO
			throw new NotImplementedException();
		}
		
		public static IPHostEntry EndGetHostByName(IAsyncResult asyncResult) {
			return ((DnsAsyncResult)asyncResult).Worker.Result;
		}
		
		public static IPHostEntry EndResolve(IAsyncResult asyncResult) {
			// TODO
			throw new NotImplementedException();
		}
		
		/// <param name=hostName>
		/// IP address in network byte order (e.g. Big-Endian).
		/// </param>
		/// <param name=length>
		/// Length of IP address.
		/// </param>
		/// <param name=type>
		/// Type (should be 2, equals AF_INET).
		/// </param>
		[DllImport("cygwin1", EntryPoint="gethostbyaddr")]
		private static extern IntPtr _GetHostByAddress(byte[] hostName,
							       short length,
							       short type);
		
		/// <param name=address>
		/// IP address in network byte order (e.g. Big-Endian).
		/// </param>
		private static IPHostEntry GetHostByAddress(long address) {
			short length = 4;
			if (address > uint.MaxValue)
				length = 8;
			byte[] addr = new byte[length];
			for(int i = length - 1, j = 0; i >= 0; --i, ++j) {
				byte b = (byte)(address >> i * 8);
//				Console.WriteLine(b);
				addr[j] = b;
			}
			IntPtr p = _GetHostByAddress(addr, length, 2);  // TODO: set type
			if (p == IntPtr.Zero)
				throw new SocketException();  // TODO: set error code
			Hostent h = new Hostent();
			System.Runtime.InteropServices.Marshal.PtrToStructure(p, h);
			return ToIPHostEntry(h);
		}
		
		public static IPHostEntry GetHostByAddress(IPAddress address) {
			if (address == null)
				throw new ArgumentNullException();
			return GetHostByAddress(IPAddress.HostToNetworkOrder(address.Address));
		}
		
		public static IPHostEntry GetHostByAddress(string address) {
			if (address == null)
				throw new ArgumentNullException();
			return GetHostByAddress(CreateAddress(address));
		}
		
		[DllImport("cygwin1", EntryPoint="gethostbyname")]
		private static extern IntPtr _GetHostByName(string hostName);
		
		public static IPHostEntry GetHostByName(string hostName) {
			if (hostName == null)
				throw new ArgumentNullException();
			IntPtr p = _GetHostByName(hostName);
			//	  int errNo = _h_errno;
			if (p == IntPtr.Zero)
				throw new SocketException();  // TODO: set error code
			Hostent h = new Hostent();
			System.Runtime.InteropServices.Marshal.PtrToStructure(p, h);
			return ToIPHostEntry(h);
		}
		
		/// <summary>
		/// This method returns the host name associated with the local host.
		/// </summary>
		public static string GetHostName() {
			IPHostEntry h = GetHostByAddress("127.0.0.1");
			return h.HostName;
		}
		
		/// <param name=address>
		/// IP address in Little-Endian byte order.
		/// </param>
		/// <returns>
		/// IP address in dotted notation form.
		/// </returns>
		public static string IpToString(int address) {
			address = IPAddress.HostToNetworkOrder(address);
			StringBuilder res = new StringBuilder();
			for(int i = 3; i > 0; --i) {
				byte b = (byte)(address >> i * 8);
				res.Append(b);
				res.Append('.');
			}
			res.Append((byte)address);
			return res.ToString();
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
				long addr = CreateAddress(hostName);
				if (addr > uint.MaxValue)
					throw new FormatException("Only IP version 4 addresses are supported");
				return GetHostByAddress(addr);
			} catch (FormatException) {
			  return GetHostByName(hostName);
			}
		}
		
		/// <summary>
		/// Utility method. This method converts a Hostent instance to a
		/// IPHostEntry instance.
		/// </summary>
		/// <param name=h>
		/// Object which should be mapped to a IPHostEntry instance.
		/// </param>
		private static unsafe IPHostEntry ToIPHostEntry(Hostent h) {
			IPHostEntry res = new IPHostEntry();
			
			// Set host name
			res.HostName = h.h_name;
			
			// Set IP address list
			byte** p = h.h_addr_list;
			ArrayList tmp = new ArrayList(1);
			while (*p != null) {
				tmp.Add(CreateIPAddress(*p, h.h_length));
				++p;
			}
			IPAddress[] addr_list = new IPAddress[tmp.Count];
			for(int i = 0; i < tmp.Count; ++i)
				addr_list[i] = (IPAddress)tmp[i];
			res.AddressList = addr_list;
			
			// Set IP aliases
			p = h.h_aliases;
			tmp.Clear();
			while (*p != null) {
				tmp.Add(new string((sbyte*)*p));
				++p;
			}
			string[] aliases = new string[tmp.Count];
			for(int i = 0; i < tmp.Count; ++i)
				aliases[i] = (string)tmp[i];
			res.Aliases = aliases;
			
			return res;
		}
		
		/// <summary>
		/// Utility method. Convert IP address in dotted notation
		/// to IP address.
		/// </summary>
		private static long CreateAddress(string address) {
			string[] tokens = address.Split('.');
			if (tokens.Length % 4 != 0)
				throw new FormatException("IP address has invalid length");
			long addr = 0;
			for(int i = 0, j = tokens.Length - 1; i < tokens.Length; ++i, --j) {
				try {
					addr = addr | (((long)byte.Parse(tokens[i])) << j * 8);
				} catch (OverflowException) {
					throw new FormatException("Invalid IP address format");
				}
			}
			return addr;
		}
	
		/// <summary>
		/// Utility method. This method creates a IP address.
		/// </summary>
		/// <param name=addr>
		/// IP address in network byte order (e.g. Big-Endian).
		/// </param>
		/// <param name=length>
		/// Length of IP address (4 or 8 bytes).
		/// </param>
		private static unsafe IPAddress CreateIPAddress(byte* addr, short length) {
			byte* p = addr;
			long res = 0;
			for(int i = 0, j = length - 1; i < length; ++i, --j) {
				res += *p << j * 8;
				++p;
			}
			if (res > uint.MaxValue)
				return new IPAddress(IPAddress.NetworkToHostOrder(res));
			else
				return new IPAddress(IPAddress.NetworkToHostOrder((int)res));
		}
	}
}

