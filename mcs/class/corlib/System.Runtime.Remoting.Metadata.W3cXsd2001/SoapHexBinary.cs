//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapHexBinary
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.Runtime.Remoting.Metadata.W3cXsd2001 
{
	[Serializable]
        public sealed class SoapHexBinary : ISoapXsd
	{
		[MonoTODO]
		public SoapHexBinary()
		{
		}
		
		public byte [] Value {
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
		public static SoapHexBinary Parse (string value)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		~SoapHexBinary()
		{
		}
	}
}
