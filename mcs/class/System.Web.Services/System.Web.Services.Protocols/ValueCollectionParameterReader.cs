// 
// System.Web.Services.Protocols.ValueCollectionParameterReader.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Collections.Specialized;
using System.Reflection;
using System.Web;

namespace System.Web.Services.Protocols {
	public abstract class ValueCollectionParameterReader : MimeParameterReader {

		#region Constructors

		[MonoTODO]
		protected ValueCollectionParameterReader () 
		{
			throw new NotImplementedException ();
		}
		
		#endregion // Constructors

		#region Methods

		[MonoTODO]
		public override object GetInitializer (LogicalMethodInfo methodInfo)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Initialize (object o)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static bool IsSupported (LogicalMethodInfo methodInfo)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static bool IsSupported (ParameterInfo paramInfo)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected object[] Read (NameValueCollection collection)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
