//
// System.Messaging
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//      Rafael Teixeira   (rafaelteixeirabr@hotmail.com)
//
// (C) 2003 Peter Van Isacker
//
using System;

namespace System.Messaging 
{
	public class Trustee 
	{
		[MonoTODO]
		public Trustee()
		{
			this.name = null;
			this.systemName = null;
			this.trusteeType = TrusteeType.Unknown;
		}
		
		[MonoTODO("What about systemName?")]
		public Trustee(string name)
		{
			this.name = name;
			this.systemName = null;
			this.trusteeType = TrusteeType.Unknown;
		}
		
		private string name;
		private string systemName;
		private TrusteeType trusteeType;
		
		public Trustee(string name, string systemName)
		{
			this.name = name;
			this.systemName = systemName;
			this.trusteeType = TrusteeType.Unknown;
		}
		
		public Trustee(string name, string systemName, TrusteeType trusteeType)
		{
			this.name = name;
			this.systemName = systemName;
			this.trusteeType = trusteeType;
		}
		
		public string Name 
		{
			get { return name; }
			set { name = value;}
		}
		
		public string SystemName 
		{
			get { return systemName; }
			set { systemName = value;}
		}

		public TrusteeType TrusteeType
		{
			get { return trusteeType; }
			set { trusteeType = value;}
		}
		
		[MonoTODO]
		~Trustee()
		{
		}
	}
}
