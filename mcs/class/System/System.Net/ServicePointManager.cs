//
// System.Net.ServicePointManager
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Security.Cryptography.X509Certificates;

//
// notes:
// A service point manager manages service points (duh!).
// A service point maintains a list of connections (per scheme + authority).
// According to HttpWebRequest.ConnectionGroupName each connection group
// creates additional connections. therefor, a service point has a hashtable
// of connection groups where each value is a list of connections.
// 
// when we need to make an HttpWebRequest, we need to do the following:
// 1. find service point, given Uri and Proxy 
// 2. find connection group, given service point and group name
// 3. find free connection in connection group, or create one (if ok due to limits)
// 4. lease connection
// 5. execute request
// 6. when finished, return connection
//


namespace System.Net 
{
	class DummyPolicy : ICertificatePolicy
	{
		public bool CheckValidationResult (ServicePoint point,
						   X509Certificate certificate,
						   WebRequest request,
						   int certificateProblem)
		{
			return (certificateProblem == 0);
		}
	}
	
	public class ServicePointManager
	{
		private static HybridDictionary servicePoints = new HybridDictionary ();
		
		// Static properties
		
		private static ICertificatePolicy policy = new DummyPolicy ();
		private static int defaultConnectionLimit = DefaultPersistentConnectionLimit;
		private static int maxServicePointIdleTime = 900000; // 15 minutes
		private static int maxServicePoints = 0;
		
		// Fields
		
		public const int DefaultNonPersistentConnectionLimit = 4;
		public const int DefaultPersistentConnectionLimit = 2;
		
		// Constructors
		private ServicePointManager ()
		{
		}		
		
		// Properties
		
		public static ICertificatePolicy CertificatePolicy {
			get { return policy; }
			set { policy = value; }
		}
		
		public static int DefaultConnectionLimit {
			get { return defaultConnectionLimit; }
			set { 
				if (value <= 0)
					throw new ArgumentOutOfRangeException ("value");

				defaultConnectionLimit = value; 
			}
		}
		
		public static int MaxServicePointIdleTime {
			get { 
				return maxServicePointIdleTime;
			}
			set { 
				if (value < -2 || value > Int32.MaxValue)
					throw new ArgumentOutOfRangeException ("value");
				maxServicePointIdleTime = value;
			}
		}
		
		public static int MaxServicePoints {
			get { 
				return maxServicePoints; 
			}
			set {  
				if (value < 0)
					throw new ArgumentException ("value");				

				maxServicePoints = value;
				RecycleServicePoints ();
			}
		}
		
		// Methods
		
		public static ServicePoint FindServicePoint (Uri address) 
		{
			return FindServicePoint (address, GlobalProxySelection.Select);
		}
		
		public static ServicePoint FindServicePoint (string uriString, IWebProxy proxy)
		{
			return FindServicePoint (new Uri(uriString), proxy);
		}
		
		public static ServicePoint FindServicePoint (Uri address, IWebProxy proxy)
		{
			if (address == null)
				throw new ArgumentNullException ("address");

			RecycleServicePoints ();
			
			if (proxy != null && !proxy.IsBypassed(address)) {
				address = proxy.GetProxy (address);
				if (address.Scheme != "http" && address.Scheme != "https")
					throw new NotSupportedException ("Proxy scheme not supported.");
			} 

			address = new Uri (address.Scheme + "://" + address.Authority);
			
			ServicePoint sp = null;
			lock (servicePoints) {
				sp = servicePoints [address] as ServicePoint;
				if (sp != null)
					return sp;

				if (maxServicePoints > 0 && servicePoints.Count >= maxServicePoints)
					throw new InvalidOperationException ("maximum number of service points reached");
				sp = new ServicePoint (address, defaultConnectionLimit, maxServicePointIdleTime);
				servicePoints.Add (address, sp);
			}
			
			return sp;
		}
		
		// Internal Methods

		internal static void RecycleServicePoints ()
		{
			ArrayList toRemove = new ArrayList ();
			lock (servicePoints) {
				IDictionaryEnumerator e = servicePoints.GetEnumerator ();
				while (e.MoveNext ()) {
					ServicePoint sp = (ServicePoint) e.Value;
					if (sp.AvailableForRecycling) {
						toRemove.Add (e.Key);
					}
				}
				
				for (int i = 0; i < toRemove.Count; i++) 
					servicePoints.Remove (toRemove [i]);

				if (maxServicePoints == 0 || servicePoints.Count <= maxServicePoints)
					return;

				// get rid of the ones with the longest idle time
				SortedList list = new SortedList (servicePoints.Count);
				e = servicePoints.GetEnumerator ();
				while (e.MoveNext ()) {
					ServicePoint sp = (ServicePoint) e.Value;
					if (sp.CurrentConnections == 0) {
						while (list.ContainsKey (sp.IdleSince))
							sp.IdleSince.AddMilliseconds (1);
						list.Add (sp.IdleSince, sp.Address);
					}
				}
				
				for (int i = 0; i < list.Count && servicePoints.Count > maxServicePoints; i++)
					servicePoints.Remove (list.GetByIndex (i));
			}
		}
	}
}
