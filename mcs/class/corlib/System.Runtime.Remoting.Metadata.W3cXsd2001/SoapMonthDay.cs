//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapMonthDay
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.Runtime.Remoting.Metadata.W3cXsd2001 
{
	[Serializable]
        public sealed class SoapMonthDay : ISoapXsd
	{
		[MonoTODO]
		public SoapMonthDay()
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
		public static SoapMonthDay Parse (string value)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		~SoapMonthDay()
		{
		}
	}
}
