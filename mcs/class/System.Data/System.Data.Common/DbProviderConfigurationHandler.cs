//
// System.Data.Common.DbProviderConfigurationHandler.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Configuration;
using System.Xml;

namespace System.Data.Common {
	public class DbProviderConfigurationHandler : IConfigurationSectionHandler
	{
		#region Constructors

		[MonoTODO]
		public DbProviderConfigurationHandler ()
		{
		}

		#endregion // Constructors

		#region Methods

		[MonoTODO]
		public virtual object Create (object parent, object configContext, XmlNode section)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}

#endif // NET_1_2
