//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapDate
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.Runtime.Remoting.Metadata.W3cXsd2001 
{
	[Serializable]
        public sealed class SoapDate : ISoapXsd
	{
		[MonoTODO]
		public SoapDate()
		{
		}

		public int Sign {
			[MonoTODO]
			get { throw new NotImplementedException(); } 

			[MonoTODO]
			set { throw new NotImplementedException(); }
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
		public static SoapDate Parse (string value)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		~SoapDate()
		{
		}
	}
}
