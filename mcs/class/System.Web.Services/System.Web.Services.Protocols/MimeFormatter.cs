// 
// System.Web.Services.Protocols.MimeFormatter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services;

namespace System.Web.Services.Protocols {
	public abstract class MimeFormatter {

		#region Constructors

		protected MimeFormatter () 
		{
		}
		
		#endregion // Constructors

		#region Methods

		public static MimeFormatter CreateInstance (Type type, object initializer)
		{
			MimeFormatter ob = (MimeFormatter) Activator.CreateInstance (type);
			ob.Initialize (initializer);
			return ob;
		}

		public abstract object GetInitializer (LogicalMethodInfo methodInfo);

		public static object GetInitializer (Type type, LogicalMethodInfo methodInfo)
		{
			MimeFormatter ob = (MimeFormatter) Activator.CreateInstance (type);
			return ob.GetInitializer (methodInfo);
		}

		public virtual object[] GetInitializers (LogicalMethodInfo[] methodInfos)
		{
			object[] initializers = new object [methodInfos.Length];
			for (int n=0; n<methodInfos.Length; n++)
				initializers [n] = GetInitializer (methodInfos[n]);
				
			return initializers;
		}

		public static object[] GetInitializers (Type type, LogicalMethodInfo[] methodInfos)
		{
			MimeFormatter ob = (MimeFormatter) Activator.CreateInstance (type);
			return ob.GetInitializers (methodInfos);
		}

		public abstract void Initialize (object initializer);	

		#endregion // Methods
	}
}
