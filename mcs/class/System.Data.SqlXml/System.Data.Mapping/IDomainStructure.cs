//
// System.Data.Mapping.IDomainStructure
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Xml;

namespace System.Data.Mapping {
        public interface IDomainStructure
        {
		#region Properties

		IDomainSchema DomainSchema { get; }
		string Select { get; }

		#endregion // Properties

		#region Methods

		IDomainField GetDomainField (string fieldName, IXmlNamespaceResolver namespaces);

		#endregion // Methods
        }
}

#endif // NET_1_2
