//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapTime
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.Runtime.Remoting.Metadata.W3cXsd2001 
{
	[Serializable]
        public sealed class SoapTime : ISoapXsd
	{
		[MonoTODO]
		public SoapTime()
		{
		}

		public DateTime Value {
			[MonoTODO]
			get { throw new NotImplementedException(); } 

			[MonoTODO]
			set { throw new NotImplementedException(); }
		}
		public static string XsdType {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public string GetXsdType()
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		public static SoapTime Parse (string value)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		~SoapTime()
		{
		}
		
	}
}
