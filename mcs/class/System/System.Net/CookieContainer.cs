//
// System.Net.CookieContainer
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using System;
using System.Collections;
using System.Runtime.Serialization;

namespace System.Net 
{
	[Serializable]
	public class CookieContainer
	{		
		private int count;
		private int capacity;
		private int perDomainCapacity;
		private int maxCookieSize;
				
		// ctors
		public CookieContainer () : this (DefaultCookieLimit) 
		{ 
		} 
	
		public CookieContainer (int capacity) : 
			this (capacity, DefaultPerDomainCookieLimit, DefaultCookieLengthLimit) 
		{ 
		}
		
		public CookieContainer (int capacity, int perDomainCapacity, int maxCookieSize)
		{
			this.capacity = capacity;
			this.perDomainCapacity = perDomainCapacity;
			this.maxCookieSize = maxCookieSize;
			this.count = 0;
		}

		// fields		
		
		public const int DefaultCookieLengthLimit = 4096;
		public const int DefaultCookieLimit = 300;
		public const int DefaultPerDomainCookieLimit = 20;
		
		// properties
		
		public int Count { 
			get { return count; }
		}
		
		public int Capacity {
			get { return capacity; }
			set { 
				if ((value <= 0) ||
				    (value < perDomainCapacity && perDomainCapacity != Int32.MaxValue))
					throw new ArgumentOutOfRangeException ("value");
				if (value < maxCookieSize)
					maxCookieSize = value;
				capacity = value;							
			}
		}
		
		public int MaxCookieSize {
			get { return maxCookieSize; }
			set {
				if (value <= 0)
					throw new ArgumentOutOfRangeException ("value");				
				maxCookieSize = value;
			}
		}
		
		public int PerDomainCapacity {
			get { return perDomainCapacity; }
			set {
				if ((value <= 0) ||
				    (value > DefaultCookieLimit && value != Int32.MaxValue))
					throw new ArgumentOutOfRangeException ("value");					
				if (value < perDomainCapacity)
					perDomainCapacity = value;
				perDomainCapacity = value;
			}
		}
		
		[MonoTODO]
		public void Add (Cookie cookie) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Add (CookieCollection cookies)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Add (Uri uri, Cookie cookie)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Add (Uri uri, CookieCollection cookies)
		{
			throw new NotImplementedException ();
		}		

		[MonoTODO]
		public string GetCookieHeader (Uri uri)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public CookieCollection GetCookies (Uri uri)
		{
			throw new NotImplementedException ();				
		}

		[MonoTODO]
		public void SetCookies (Uri uri, string cookieHeader)
		{
			throw new NotImplementedException ();			
		}

	} // CookieContainer

} // System.Net

