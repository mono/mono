// 
// System.Web.Services.Protocols.UrlEncodedParameterWriter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.IO;
using System.Text;
using System.Web.Services;

namespace System.Web.Services.Protocols {
	public abstract class UrlEncodedParameterWriter : MimeParameterWriter {

		#region Constructors

		[MonoTODO]
		protected UrlEncodedParameterWriter () 
		{
			throw new NotImplementedException ();
		}
		
		#endregion // Constructors

		#region Properties 

		public override Encoding RequestEncoding {
			[MonoTODO]
			get { throw new NotImplementedException (); }
			[MonoTODO]
			set { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		protected void Encode (TextWriter writer, object[] values)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void Encode (TextWriter writer, string name, object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override object GetInitializer (LogicalMethodInfo methodInfo)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Initialize (object initializer)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
