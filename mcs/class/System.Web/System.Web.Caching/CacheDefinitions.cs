// 
// System.Web.Caching
//
// Author:
//   Patrik Torstensson
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
	/// Specifies the reason an item was removed from the Cache.
	/// </summary>
	public enum CacheItemRemovedReason {
		Removed = 1,
		Expired = 2,
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
