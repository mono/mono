//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapToken
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.Runtime.Remoting.Metadata.W3cXsd2001 
{
	[Serializable]
        public sealed class SoapToken : ISoapXsd 
	{
		[MonoTODO]
		public SoapToken()
		{
		}

		public string Value {
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
		public static SoapToken Parse (string value)
		{
			throw new NotImplementedException(); 
		}

		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException(); 
		}

		[MonoTODO]
		~SoapToken()
		{
		}
	}
}
