//
// System.Runtime.Remoting.Messaging.RemotingSurrogate.cs
//
// Author:    Patrik Torstensson
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Serialization;

namespace System.Runtime.Remoting.Messaging 
{
	internal class RemotingSurrogate : ISerializationSurrogate
	{
		public virtual void GetObjectData(Object obj, SerializationInfo si, StreamingContext sc)
		{               
			if (null == obj || null == si)
				throw new ArgumentNullException();

			if (RemotingServices.IsTransparentProxy (obj) )
			{
				RealProxy rp = RemotingServices.GetRealProxy (obj);
				rp.GetObjectData (si, sc);
			} else RemotingServices.GetObjectData (obj, si, sc);
		}

		public virtual Object  SetObjectData(Object obj, SerializationInfo si, StreamingContext sc, ISurrogateSelector selector)
		{ 
			throw new NotSupportedException();
		}
	}

	internal class ObjRefSurrogate : ISerializationSurrogate
	{
		public virtual void GetObjectData(Object obj, SerializationInfo si, StreamingContext sc)
		{               
			if (null == obj || null == si)
				throw new ArgumentNullException();
			
			((ObjRef) obj).GetObjectData (si, sc);

			// added to support same syntax as MS
			si.AddValue("fIsMarshalled", 0);            
		}

		public virtual Object SetObjectData(Object obj, SerializationInfo si, StreamingContext sc, ISurrogateSelector selector)
		{ 
			// ObjRef is deserialized using the IObjectReference interface
			throw new NotSupportedException ("Do not use RemotingSurrogateSelector when deserializating");
		}
	}
}