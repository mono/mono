//
// System.Data.Mapping.IDomainConstraint
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

namespace System.Data.Mapping {
        public interface IDomainConstraint
        {
		#region Properties

		bool CascadeDelete { get; }
		IDomainSchema DomainSchema { get; }
		IDomainFieldJoinCollection FieldJoins { get; }
		IDomainStructure FromDomainStructure { get; }
		string Name { get; }
		IDomainStructure ToDomainStructure { get; }

		#endregion // Properties
        }
}

#endif // NET_1_2
