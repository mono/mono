// 
// System.Web.Services.Protocols.MimeFormatter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services;

namespace System.Web.Services.Protocols {
	public abstract class MimeFormatter {

		#region Constructors

		[MonoTODO]
		protected MimeFormatter () 
		{
			throw new NotImplementedException ();
		}
		
		#endregion // Constructors

		#region Methods

		[MonoTODO]
		public static MimeFormatter CreateInstance (Type type, object initializer)
		{
			throw new NotImplementedException ();
		}

		public abstract object GetInitializer (LogicalMethodInfo methodInfo);

		[MonoTODO]
		public static object GetInitializer (Type type, LogicalMethodInfo methodInfo)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual object[] GetInitializers (LogicalMethodInfo[] methodInfos)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static object[] GetInitializers (Type type, LogicalMethodInfo methodInfos)
		{
			throw new NotImplementedException ();
		}

		public abstract void Initialize (object initializer);	

		#endregion // Methods
	}
}
