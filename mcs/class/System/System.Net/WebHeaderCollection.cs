//
// System.Net.WebHeaderCollection
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using System;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Net 
{
	[Serializable]
	[ComVisible(true)]
	public class WebHeaderCollection : NameValueCollection, ISerializable
	{
		// Constructors
		
		public WebHeaderCollection ()
			: base () 
		{
		}
		
		[MonoTODO]
		protected WebHeaderCollection (SerializationInfo serializationInfo, 
					       StreamingContext streamingContext)
		{
			throw new NotImplementedException ();
		}
		
		// Properties
		
		public virtual long ContentLength {		
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}

		// Methods
		
		[MonoTODO]
		public void Add (string header)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override void Add (string name, string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void AddWithoutValidate (string headerName, string headerValue)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override string [] GetValues (string header)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static bool IsRestricted (string headerName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void OnDeserialization (object sender)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Remove (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Set (string name, string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public byte[] ToByteArray ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string ToString ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		void ISerializable.GetObjectData (SerializationInfo serializationInfo,
		   				  StreamingContext streamingContext)
		{
			throw new NotImplementedException ();
		}		
	}
}