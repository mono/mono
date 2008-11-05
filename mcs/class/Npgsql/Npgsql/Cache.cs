// created on 29/11/2007

// Npgsql.NpgsqlConnectionStringBuilder.cs
//
// Author:
//	Glen Parker (glenebob@nwlink.com)
//	Ben Sagal (bensagal@gmail.com)
//	Tao Wang (dancefire@gmail.com)
//
//	Copyright (C) 2007 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
// 
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

using System;
using System.Collections.Generic;
//using System.Text;

namespace Npgsql
{
	internal class Cache<TEntity> : LinkedList<KeyValuePair<string, TEntity>>
		where TEntity : class
	{
		private int _cache_size = 20;

		/// <summary>
		/// Set Cache Size. The default value is 20.
		/// </summary>
		public int CacheSize
		{
			get { return _cache_size; }
			set
			{
				if (value < 0) { throw new ArgumentOutOfRangeException("CacheSize"); }

				_cache_size = value;

				if (this.Count > _cache_size)
				{
					lock (this)
					{
						while (_cache_size < this.Count)
						{
							RemoveLast();
						}
					}
				}
			}
		}

		/// <summary>
		/// Lookup cached entity. null will returned if not match.
		/// For both get{} and set{} apply LRU rule.
		/// </summary>
		/// <param name="key">key</param>
		/// <returns></returns>
		public TEntity this[string key]
		{
			get
			{
				lock (this)
				{
					for (LinkedListNode<KeyValuePair<string, TEntity>> node = this.First; node != null; node = node.Next)
					{
						if (node.Value.Key == key)
						{
							this.Remove(node);
							this.AddFirst(node);
							return node.Value.Value;
						}
					}
				}
				return null;
			}
			set
			{
				lock (this)
				{
					for (LinkedListNode<KeyValuePair<string, TEntity>> node = this.First; node != null; node = node.Next)
					{
						if (node.Value.Key == key)
						{
							this.Remove(node);
							this.AddFirst(node);
							return;
						}
					}
					if (this.CacheSize > 0)
					{
						this.AddFirst(new KeyValuePair<string, TEntity>(key, value));
						if (this.Count > this.CacheSize)
						{
							this.RemoveLast();
						}
					}
				}
			}
		}

		public Cache() : base() { }
		public Cache(int cacheSize) : base()
		{
			this._cache_size = cacheSize;
		}
	}
}
