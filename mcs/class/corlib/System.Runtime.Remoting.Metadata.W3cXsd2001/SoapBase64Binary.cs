//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapBase64Binary
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.Runtime.Remoting.Metadata.W3cXsd2001 
{
	[Serializable]
        public sealed class SoapBase64Binary : ISoapXsd
	{
		[MonoTODO]
		public SoapBase64Binary()
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
		public static SoapBase64Binary Parse (string value)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		~SoapBase64Binary()
		{
		}
	}
}
