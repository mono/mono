//
// Mono.Net.Dns.SimpleResolver
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo.mono@gmail.com)
//
// Copyright 2011 Gonzalo Paniagua Javier
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

namespace Mono.Net.Dns {
	sealed class SimpleResolver : IDisposable {
		static string [] EmptyStrings = new string [0];
		static IPAddress [] EmptyAddresses = new IPAddress [0];
		IPEndPoint [] endpoints;
		Socket client;
		Dictionary<int, SimpleResolverEventArgs> queries;
		AsyncCallback receive_cb;
		TimerCallback timeout_cb;
		bool disposed;
#if REUSE_RESPONSES
		Stack<DnsResponse> responses_avail = new Stack<DnsResponse> ();
#endif

		public SimpleResolver ()
		{
			queries = new Dictionary<int, SimpleResolverEventArgs> ();
			receive_cb = new AsyncCallback (OnReceive);
			timeout_cb = new TimerCallback (OnTimeout);
			InitFromSystem ();
			InitSocket ();
		}

		void IDisposable.Dispose ()
		{
			if (!disposed) {
				disposed = true;
				if (client != null) {
					client.Close ();
					client = null;
				}
			}
		}

		public void Close ()
		{
			((IDisposable) this).Dispose ();
		}

		void GetLocalHost (SimpleResolverEventArgs args)
		{
			//FIXME
			IPHostEntry entry = new IPHostEntry ();
			entry.HostName = "localhost";
			entry.AddressList = new IPAddress [] { IPAddress.Loopback };
			entry.Aliases = EmptyStrings;
			args.ResolverError = 0;
			args.HostEntry = entry;
			return;

/*
			List<IPEndPoint> eps = new List<IPEndPoint> ();
			foreach (NetworkInterface iface in NetworkInterface.GetAllNetworkInterfaces ()) {
				if (NetworkInterfaceType.Loopback == iface.NetworkInterfaceType)
					continue;

				foreach (IPAddress addr in iface.GetIPProperties ().DnsAddresses) {
					if (AddressFamily.InterNetworkV6 == addr.AddressFamily)
						continue;
					IPEndPoint ep = new IPEndPoint (addr, 53);
					if (eps.Contains (ep))
						continue;

					eps.Add (ep);
				}
			}
			endpoints = eps.ToArray ();
*/
		}

		// Type A query
		// Might fill in Aliases
		// -IPAddress -> return the same IPAddress
		// -"" -> Local host ip addresses (filter out IPv6 if needed)
		public bool GetHostAddressesAsync (SimpleResolverEventArgs args)
		{
			if (args == null)
				throw new ArgumentNullException ("args");

			if (args.HostName == null)
				throw new ArgumentNullException ("args.HostName is null");

			if (args.HostName.Length > 255)
				throw new ArgumentException ("args.HostName is too long");

			args.Reset (ResolverAsyncOperation.GetHostAddresses);
			string host = args.HostName;
			if (host == "") {
				GetLocalHost (args);
				return false;
			}
			IPAddress addr;
			if (IPAddress.TryParse (host, out addr)) {
				IPHostEntry entry = new IPHostEntry ();
				entry.HostName = host;
				entry.Aliases = EmptyStrings;
				entry.AddressList = new IPAddress [1] { addr };
				args.HostEntry = entry;
				return false;
			}

			SendAQuery (args, true);
			return true;
		}

		// For names -> type A Query
		// For IP addresses -> PTR + A -> will at least return itself
		//	Careful: for IP addresses with PTR, the hostname might yield different IP addresses!
		public bool GetHostEntryAsync (SimpleResolverEventArgs args)
		{
			if (args == null)
				throw new ArgumentNullException ("args");

			if (args.HostName == null)
				throw new ArgumentNullException ("args.HostName is null");

			if (args.HostName.Length > 255)
				throw new ArgumentException ("args.HostName is too long");

			args.Reset (ResolverAsyncOperation.GetHostEntry);
			string host = args.HostName;
			if (host == "") {
				GetLocalHost (args);
				return false;
			}

			IPAddress addr;
			if (IPAddress.TryParse (host, out addr)) {
				IPHostEntry entry = new IPHostEntry ();
				entry.HostName = host;
				entry.Aliases = EmptyStrings;
				entry.AddressList = new IPAddress [1] { addr };
				args.HostEntry = entry;
				args.PTRAddress = addr;
				SendPTRQuery (args, true);
				return true;
			}

			// 3. For IP addresses:
			//	3.1 Parsing IP succeeds
			//	3.2 Reverse lookup of the IP fills in HostName -> fails? HostName = IP
			//	3.3 The hostname resulting from this is used to query DNS again to get the IP addresses
			//
			// Exclude IPv6 addresses if not supported by the system
			// .Aliases is always empty
			// Length > 255
			SendAQuery (args, true);
			return true;
		}

		bool AddQuery (DnsQuery query, SimpleResolverEventArgs args)
		{
			lock (queries) {
				if (queries.ContainsKey (query.Header.ID))
					return false;
				queries [query.Header.ID] = args;
			}
			return true;
		}

		static DnsQuery GetQuery (string host, DnsQType q, DnsQClass c)
		{
			return new DnsQuery (host, q, c);
		}

		void SendAQuery (SimpleResolverEventArgs args, bool add_it)
		{
			SendAQuery (args, args.HostName, add_it);
		}

		void SendAQuery (SimpleResolverEventArgs args, string host, bool add_it)
		{
			DnsQuery query = GetQuery (host, DnsQType.A, DnsQClass.IN);
			SendQuery (args, query, add_it);
		}

		static string GetPTRName (IPAddress address)
		{
			// TODO: IPv6 PTR query?
			byte [] bytes = address.GetAddressBytes ();
			// "XXX.XXX.XXX.XXX.in-addr.arpa".Length
			StringBuilder sb = new StringBuilder (28);
			for (int i = bytes.Length - 1; i >= 0; i--) {
				sb.AppendFormat ("{0}.", bytes [i]);
			}
			sb.Append ("in-addr.arpa");
			return sb.ToString ();
		}

		void SendPTRQuery (SimpleResolverEventArgs args, bool add_it)
		{
			DnsQuery query = GetQuery (GetPTRName (args.PTRAddress), DnsQType.PTR, DnsQClass.IN);
			SendQuery (args, query, add_it);
		}

		void SendQuery (SimpleResolverEventArgs args, DnsQuery query, bool add_it)
		{
			// TODO: not sure about reusing IDs when add_it == false
			int count = 0;
			if (add_it) {
				do {
					query.Header.ID = (ushort)new Random().Next(1, 65534);
					if (count > 500)
						throw new InvalidOperationException ("Too many pending queries (or really bad luck)");
				} while (AddQuery (query, args) == false);
				args.QueryID = query.Header.ID;
			} else {
				query.Header.ID = args.QueryID;
			}
			if (args.Timer == null)
				args.Timer = new Timer (timeout_cb, args, 5000, Timeout.Infinite);
			else
				args.Timer.Change (5000, Timeout.Infinite);
			client.BeginSend (query.Packet, 0, query.Length, SocketFlags.None, null, null);
		}

		byte [] GetFreshBuffer ()
		{
#if !REUSE_RESPONSES
			return new byte [512];
#else

			DnsResponse response = null;
			lock (responses_avail) {
				if (responses_avail.Count > 0) {
					response = responses_avail.Pop ();
				}
			}
			if (response == null) {
				response = new DnsResponse ();
			} else {
				response.Reset ();
			}
			return response;
#endif
		}

		void FreeBuffer (byte [] buffer)
		{
#if REUSE_RESPONSES
			// TODO: set some limit here. Configurable?
			lock (responses_avail) {
				responses_avail.Push (response);
			}
#endif
		}

		void InitSocket ()
		{
			client = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			client.Blocking = true;
			client.Bind (new IPEndPoint (IPAddress.Any, 0));
			client.Connect (endpoints [0]);
			BeginReceive ();
		}

		void BeginReceive ()
		{
			byte [] buffer = GetFreshBuffer ();
			client.BeginReceive (buffer, 0, buffer.Length, SocketFlags.None, receive_cb, buffer);
		}

		void OnTimeout (object obj)
		{
			SimpleResolverEventArgs args = (SimpleResolverEventArgs) obj;
			SimpleResolverEventArgs args2;
			lock (queries) {
				if (!queries.TryGetValue (args.QueryID, out args2)) {
					return; // Already processed.
				}
				if (args != args2)
					throw new Exception ("Should not happen: args != args2");
				args.Retries++;
				if (args.Retries > 1) {
					// Error timeout
					args.ResolverError = ResolverError.Timeout;
					args.OnCompleted (this);
				} else {
					SendAQuery (args, false);
				}
			}
		}

		void OnReceive (IAsyncResult ares)
		{
			if (disposed)
				return;

			int nread = 0;
			EndPoint remote_ep = client.RemoteEndPoint;
			try {
				nread = client.EndReceive (ares);
			} catch (Exception e) {
				Console.Error.WriteLine (e);
			}

			BeginReceive ();

			byte [] buffer  = (byte []) ares.AsyncState;
			if (nread > 12) {
				DnsResponse response = new DnsResponse (buffer, nread);
				int id = response.Header.ID;
				SimpleResolverEventArgs args = null;
				lock (queries) {
					if (queries.TryGetValue (id, out args)) {
						queries.Remove (id);
					}
				}

				if (args != null) {
					Timer t = args.Timer;
					if (t != null)
						t.Change (Timeout.Infinite, Timeout.Infinite);

					try {
						ProcessResponse (args, response, remote_ep);
					} catch (Exception e) {
						args.ResolverError = (ResolverError) (-1);
						args.ErrorMessage = e.Message;
					}

					IPHostEntry entry = args.HostEntry;
					if (args.ResolverError != 0 && args.PTRAddress != null && entry != null && entry.HostName != null) {
						args.PTRAddress = null;
						SendAQuery (args, entry.HostName, true);
						args.Timer.Change (5000, Timeout.Infinite);
					} else {
						args.OnCompleted (this);
					}
				}
			}
			FreeBuffer (buffer);
		}

		void ProcessResponse (SimpleResolverEventArgs args, DnsResponse response, EndPoint server_ep)
		{
			DnsRCode status = response.Header.RCode;
			if (status != 0) {
				if (args.PTRAddress != null) {
					// PTR query failed -> no error, we have the IP
					return;
				}
				args.ResolverError = (ResolverError) status;
				return;
			}

			// TODO: verify IP of the server is in our list and the same one that got the query
			IPEndPoint ep = (IPEndPoint) server_ep;
			if (ep.Port != 53) {
				args.ResolverError = ResolverError.ResponseHeaderError;
				args.ErrorMessage = "Port";
				return;
			}

			DnsHeader header = response.Header;
			if (!header.IsQuery) {
				args.ResolverError = ResolverError.ResponseHeaderError;
				args.ErrorMessage = "IsQuery";
				return;
			}

			// TODO: handle Truncation. Retry with bigger buffer?

			if (header.QuestionCount > 1) {
				args.ResolverError = ResolverError.ResponseHeaderError;
				args.ErrorMessage = "QuestionCount";
				return;
			}
			ReadOnlyCollection<DnsQuestion> q = response.GetQuestions ();
			if (q.Count != 1) {
				args.ResolverError = ResolverError.ResponseHeaderError;
				args.ErrorMessage = "QuestionCount 2";
				return;
			}
			DnsQuestion question = q [0];
			/* The answer might have dot at the end, etc...
			if (String.Compare (question.Name, args.HostName) != 0) {
				args.ResolverError = ResolverError.ResponseHeaderError;
				args.ErrorMessage = "HostName - " + question.Name + " != " + args.HostName;
				return;
			}
			*/

			DnsQType t = question.Type;
			if (t != DnsQType.A && t != DnsQType.AAAA && t != DnsQType.PTR) {
				args.ResolverError = ResolverError.ResponseHeaderError;
				args.ErrorMessage = "QType " + question.Type;
				return;
			}

			if (question.Class != DnsQClass.IN) {
				args.ResolverError = ResolverError.ResponseHeaderError;
				args.ErrorMessage = "QClass " + question.Class;
				return;
			}

			ReadOnlyCollection<DnsResourceRecord> records = response.GetAnswers ();
			if (records.Count == 0) {
				if (args.PTRAddress != null) {
					// PTR query failed -> no error
					return;
				}
				args.ResolverError = ResolverError.NameError; // is this ok?
				args.ErrorMessage = "NoAnswers";
				return;
			}

			List<string> aliases = null;
			List<IPAddress> addresses = null;
			foreach (DnsResourceRecord r in records) {
				if (r.Class != DnsClass.IN)
					continue;
				if (r.Type == DnsType.A || r.Type == DnsType.AAAA) {
					if (addresses == null)
						addresses = new List<IPAddress> ();
					addresses.Add (((DnsResourceRecordIPAddress) r).Address);
				} else if (r.Type == DnsType.CNAME) {
					if (aliases == null)
						aliases = new List<string> ();
					aliases.Add (((DnsResourceRecordCName) r).CName);
				} else if (r.Type == DnsType.PTR) {
					args.HostEntry.HostName = ((DnsResourceRecordPTR) r).DName;
					args.HostEntry.Aliases = aliases == null ? EmptyStrings : aliases.ToArray ();
					args.HostEntry.AddressList = EmptyAddresses;
					return;
				}
			}

			IPHostEntry entry = args.HostEntry ?? new IPHostEntry ();
			if (entry.HostName == null && aliases != null && aliases.Count > 0) {
				entry.HostName = aliases [0];
				aliases.RemoveAt (0);
			}
			entry.Aliases = aliases == null ? EmptyStrings : aliases.ToArray ();
			entry.AddressList = addresses == null ? EmptyAddresses : addresses.ToArray ();
			args.HostEntry = entry;
			if ((question.Type == DnsQType.A || question.Type == DnsQType.AAAA) && entry.AddressList == EmptyAddresses) {
				args.ResolverError = ResolverError.NameError;
				args.ErrorMessage = "No addresses in response";
			} else if (question.Type == DnsQType.PTR && entry.HostName == null) {
				args.ResolverError = ResolverError.NameError;
				args.ErrorMessage = "No PTR in response";
			}

		}

		void InitFromSystem ()
		{
			List<IPEndPoint> eps = new List<IPEndPoint> ();
			foreach (NetworkInterface iface in NetworkInterface.GetAllNetworkInterfaces ()) {
				if (NetworkInterfaceType.Loopback == iface.NetworkInterfaceType)
					continue;

				foreach (IPAddress addr in iface.GetIPProperties ().DnsAddresses) {
					if (AddressFamily.InterNetworkV6 == addr.AddressFamily)
						continue;
					IPEndPoint ep = new IPEndPoint (addr, 53);
					if (eps.Contains (ep))
						continue;

					eps.Add (ep);
				}
			}
			endpoints = eps.ToArray ();
		}
	}
}

