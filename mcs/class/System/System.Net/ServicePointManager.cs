//
// System.Net.ServicePointManager
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using System;
using System.Security.Cryptography.X509Certificates;

namespace System.Net 
{
	public class ServicePointManager
	{
		
		// Fields
		
		public const int DefaultNonPersistentConnectionLimit = 4;
		public const int DefaultPersistentConnectionLimit = 2;
		
		// Constructors
		private ServicePointManager ()
		{
		}		
		
		// Properties
		
		[MonoTODO]
		public static ICertificatePolicy CertificatePolicy {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public static int DefaultConnectionLimit {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public static int MaxServicePointIdleTime {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public static int MaxServicePoints {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		// Methods
		
		[MonoTODO]
		public static ServicePoint FindServicePoint (Uri address) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static ServicePoint FindServicePoint (string uriString, IWebProxy proxy)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static ServicePoint FindServicePoint (Uri address, IWebProxy proxy)
		{
			throw new NotImplementedException ();
		}
	}
}