//
// System.Data.Mapping.IDomainFieldJoinCollection
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

namespace System.Data.Mapping { 
        public interface IDomainFieldJoinCollection
        {
		#region Properties

		IDomainFieldJoin this [int index] { get; }

		#endregion // Properties

		#region Methods

		bool Contains (IDomainFieldJoin fieldJoin);
		void CopyTo (IDomainFieldJoin[] fieldJoins, int index);
		int IndexOf (IDomainFieldJoin fieldJoin);

		#endregion // Methods
        }
}

#endif // NET_1_2
