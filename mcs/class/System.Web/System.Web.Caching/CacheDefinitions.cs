// 
// System.Web.Caching
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//
// (C) Copyright Patrik Torstensson, 2001
//
namespace System.Web.Caching
{
	/// <summary>
	/// Specifies the relative priority of items stored in the Cache.
	/// </summary>
	public enum CacheItemPriority {
		Low = 1,
		BelowNormal = 2,
		Normal = 3,
		Default = 3,
		AboveNormal = 4,
		High = 5,
		NotRemovable 
	}

	/// <summary>
	/// Specifies the rate at which the priority of items stored in the Cache are downgraded when not accessed frequently.
	/// </summary>
	public enum CacheItemPriorityDecay { 
		Default,
		Fast,
		Medium,
		Never,
		Slow
	}

	/// <summary>
	/// Specifies the reason an item was removed from the Cache.
	/// </summary>
	public enum CacheItemRemovedReason {
		Expired = 1,
		Removed = 2,
		Underused = 3,
		DependencyChanged = 4
	}

	/// <summary>
	/// Defines a callback method for notifying applications when a cached item is removed from the Cache.
	/// </summary>
	/// <param name="key">The index location for the item removed from the cache. </param>
	/// <param name="value">The Object item removed from the cache. </param>
	/// <param name="reason">The reason the item was removed from the cache, as specified by the CacheItemRemovedReason enumeration.</param>
	public delegate void CacheItemRemovedCallback(string key, object value, CacheItemRemovedReason reason);

	}
