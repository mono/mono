//
// System.Runtime.Remoting.Messaging.RemotingSurrogate.cs
//
// Author:    Patrik Torstensson
//
// (C) Ximian, Inc.  http://www.ximian.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
#if !DISABLE_REMOTING
			if (RemotingServices.IsTransparentProxy (obj) )
			{
				RealProxy rp = RemotingServices.GetRealProxy (obj);
				rp.GetObjectData (si, sc);
			} else RemotingServices.GetObjectData (obj, si, sc);
#endif
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