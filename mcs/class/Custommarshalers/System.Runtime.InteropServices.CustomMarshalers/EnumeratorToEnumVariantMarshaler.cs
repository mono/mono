//
// System.Runtime.InteropServices.CustomMarshalers.EnumeratorToEnumVariantMarshaler
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.Runtime.InteropServices.CustomMarshalers
{
        public class EnumeratorToEnumVariantMarshaler : ICustomMarshaler {
				[MonoTODO]
		public void CleanUpManagedData (object pManagedObj)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void CleanUpNativeData (IntPtr pNativeData)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public static ICustomMarshaler GetInstance (string pstrCookie)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public int GetNativeDataSize()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public IntPtr MarshalManagedToNative (object pManagedObj)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public object MarshalNativeToManaged (IntPtr pNativeData)
		{
			throw new NotImplementedException();
		}
	}
}
