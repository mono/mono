//
// System.Data.ConflictOptions.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

namespace System.Data {
	public enum ConflictOptions 
	{
		Optimistic,
		SetAllValues,
		CompareRowVersion,
		CompareRowVersionUpdate,
		CompareRowVersionDelete,
		CompareAllValues,
		CompareAllValuesUpdate,
		CompareAllValuesDelete,
		CompareChangedValues,
		CompareChangedValuesUpdate,
		CompareChangedValuesDelete,
		OverwriteChanges,
		OverwriteChangesUpdate,
		OverwriteChangesDelete
	}
}

#endif // NET_1_2
