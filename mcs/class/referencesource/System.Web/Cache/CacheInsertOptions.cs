//------------------------------------------------------------------------------
// <copyright file="CacheInsertOptions.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

 /*
 * CacheInsertOptions class
 *
 * Copyright (c) 2016 Microsoft Corporation
 */


namespace System.Web.Caching
{
    public class CacheInsertOptions {

        public CacheDependency Dependencies { get; set; }
        public DateTime AbsoluteExpiration { get; set; } = Cache.NoAbsoluteExpiration;
        public TimeSpan SlidingExpiration { get; set; } = Cache.NoSlidingExpiration;
        public CacheItemPriority Priority { get; set; } = CacheItemPriority.Default;
        public CacheItemRemovedCallback OnRemovedCallback { get; set; }

     }
}