//
// Microsoft.Web.Services.Messaging.SoapReceivers
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.Collections;
using Microsoft.Web.Services.Configuration;

namespace Microsoft.Web.Services.Messaging
{

	public class SoapReceivers
	{

		private static Hashtable _receivers = new Hashtable ();
	
		private SoapReceivers () { }

		public static void Add (Uri uri, SoapReceiver receiver)
		{
			Add (uri, receiver, false);
		}

		public static void Add (Uri uri, Type type)
		{
			Add (uri, type, false);
		}

		public static void Add (Uri uri, SoapReceiver receiver, bool passive)
		{
			if(uri == null) {
				throw new ArgumentNullException ("uri");
			}
			if(receiver == null) {
				throw new ArgumentNullException ("receiver");
			}
			
			lock(SyncRoot) {
				if(_receivers.Contains (uri)) {
					throw new ArgumentException ("An item with this key already exists");
				}
				
				if(passive == false) {
					ISoapTransport trans = WebServicesConfiguration.MessagingConfiguration.GetTransport (uri.Scheme);
					
					if(trans != null) {
						trans.RegisterPort (uri, receiver);
					} else {
						throw new NotSupportedException ("Transport " + uri.Scheme + " not supported.");
					}
				}

				_receivers.Add (uri, receiver);
			}
		}

		public static void Add (Uri uri, Type type, bool passive)
		{
			if(uri == null) {
				throw new ArgumentNullException ("uri");
			}
			if(type == null) {
				throw new ArgumentNullException ("type");
			}
			lock(SyncRoot) {
				if(_receivers.Contains (uri)) {
					throw new ArgumentException ("An item with this key already exists");
				}

				if(passive == false) {
					ISoapTransport trans = WebServicesConfiguration.MessagingConfiguration.GetTransport (uri.Scheme);

					if(trans != null) {
						trans.RegisterPort (uri, type);
					} else {
						throw new NotSupportedException ("Transport " + uri.Scheme + " is not supported");
					}
				}

				_receivers.Add (uri, type);
			}
		}

		public static void Clear ()
		{
			lock(SyncRoot) {
				foreach(ISoapTransport trans in WebServicesConfiguration.MessagingConfiguration.Transports) {
					trans.UnregisterAll ();
				}
				_receivers.Clear ();
			}
		}

		public static bool Contains (Uri to)
		{
			if(to == null) {
				throw new ArgumentNullException ("to");
			}
			bool retVal = false;
			lock(SyncRoot) {
				retVal = _receivers.Contains (to);
			}
			return retVal;
		}

		public static IDictionaryEnumerator GetEnumerator ()
		{
			return _receivers.GetEnumerator ();
		}

		public static object Receiver (Uri to)
		{
			return _receivers[to];
		}

		public static void Remove (Uri to)
		{
			if(to == null) {
				throw new ArgumentNullException ("to");
			}

			lock(SyncRoot) {
				if(_receivers.Contains (to) == false) {
					ISoapTransport trans = WebServicesConfiguration.MessagingConfiguration.GetTransport (to.Scheme);
					if(trans != null) {
						trans.UnregisterPort (to);
						_receivers.Remove (to);
					}
				}
			}
		}

		public static int Count {
			get { return _receivers.Values.Count; }
		}

		public static object SyncRoot {
			get { return _receivers.SyncRoot; }
		}
	}
}
