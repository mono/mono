//
// System.Net.Cache.HttpRequestCachePolicy.cs
//
// Authors:
//	Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2004
// (c) 2004 Novell, Inc. (http://www.novell.com)
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

using System;

namespace System.Net.Cache 
{
	public class HttpRequestCachePolicy : RequestCachePolicy
	{
		#region Fields

		DateTime cacheSyncDate;
		HttpRequestCacheLevel level = HttpRequestCacheLevel.Default;
		TimeSpan maxAge;
		TimeSpan maxStale;
		TimeSpan minFresh;

		#endregion // Fields

		#region Constructors

		public HttpRequestCachePolicy ()
		{
		}

		public HttpRequestCachePolicy (DateTime cacheSyncDate)
		{
			this.cacheSyncDate = cacheSyncDate;
		}

		public HttpRequestCachePolicy (HttpRequestCacheLevel level)
		{
			this.level = level;
		}

		public HttpRequestCachePolicy (HttpCacheAgeControl cacheAgeControl, TimeSpan ageOrFreshOrStale)
		{
			switch (cacheAgeControl) {
			case HttpCacheAgeControl.MaxAge:
				maxAge = ageOrFreshOrStale;
				break;
			case HttpCacheAgeControl.MaxStale:
				maxStale = ageOrFreshOrStale;
				break;
			case HttpCacheAgeControl.MinFresh:
				minFresh = ageOrFreshOrStale;
				break;
			default:
				throw new ArgumentException ("ageOrFreshOrStale");
			}
		}

		public HttpRequestCachePolicy (HttpCacheAgeControl cacheAgeControl, TimeSpan maxAge, TimeSpan freshOrStale)
		{
			this.maxAge = maxAge;

			switch (cacheAgeControl) {
			case HttpCacheAgeControl.MaxStale:
				maxStale = freshOrStale;
				break;
			case HttpCacheAgeControl.MinFresh:
				minFresh = freshOrStale;
				break;
			default:
				throw new ArgumentException ("freshOrStale");
			}
		}

		public HttpRequestCachePolicy (HttpCacheAgeControl cacheAgeControl, TimeSpan maxAge, TimeSpan freshOrStale, DateTime cacheSyncDate)
			: this (cacheAgeControl, maxAge, freshOrStale)
		{
			this.cacheSyncDate = cacheSyncDate;
		}

		#endregion // Constructors

		#region Properties

		public DateTime CacheSyncDate {
			get { return cacheSyncDate; }
		}

		public new HttpRequestCacheLevel Level {
			get { return level; }
		}

		public TimeSpan MaxAge {
			get { return maxAge; }
		}

		public TimeSpan MaxStale {
			get { return maxStale; }
		}

		public TimeSpan MinFresh {
			get { return minFresh; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}

