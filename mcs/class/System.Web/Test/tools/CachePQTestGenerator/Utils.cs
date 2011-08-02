//
//  Utils.cs
//
//  Author:
//    Marek Habersack <grendel@twistedcode.net>
//
//  Copyright (c) 2010, Marek Habersack
//
//  All rights reserved.
//
//  Redistribution and use in source and binary forms, with or without modification, are permitted
//  provided that the following conditions are met:
//
//     * Redistributions of source code must retain the above copyright notice, this list of
//       conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright notice, this list of
//       conditions and the following disclaimer in the documentation and/or other materials
//       provided with the distribution.
//     * Neither the name of Marek Habersack nor the names of its contributors may be used to
//       endorse or promote products derived from this software without specific prior written
//       permission.
//
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
//  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
//  LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
//  A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
//  CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
//  EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
//  PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
//  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
//  LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
//  NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//  SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Web.Caching;
using System.Xml;
using System.Xml.XPath;

using MonoTests.System.Web.Caching;

namespace Tester
{
	static class Utils
	{
		public static T GetOptionalAttribute <T> (this XPathNavigator nav, string name, out bool found)
		{
			string value = nav.GetAttribute (name, String.Empty);
			if (String.IsNullOrEmpty (value)) {
				found = false;
				return default(T);
			}
			found = true;
			return ConvertAttribute <T> (value);
		}
		
		public static T GetRequiredAttribute <T> (this XPathNavigator nav, string name)
		{
			string value = nav.GetAttribute (name, String.Empty);
			if (String.IsNullOrEmpty (value))
				throw new InvalidOperationException (String.Format ("Required attribute '{0}' missing.", name));
			return ConvertAttribute <T> (value);
		}

		static T ConvertAttribute <T> (string value)
		{
			if (typeof (T) == typeof (string))
				return (T)((object)value);
			
			// Special cases because we use ticks
			if (typeof (T) == typeof (DateTime))
				return (T)((object) new DateTime (Int64.Parse (value)));
			else if (typeof (T) == typeof (TimeSpan))
				return (T)((object) new TimeSpan (Int64.Parse (value)));
			
			TypeConverter cvt = TypeDescriptor.GetConverter (typeof (T));
			if (cvt == null)
				throw new InvalidOperationException (String.Format ("Type converter for type '{0}' cannot be found.", typeof (T)));

			if (!cvt.CanConvertFrom (typeof (string)))
				throw new InvalidOperationException (String.Format ("Conversion from string to type '{0}' is not supported.", typeof (T)));
			
			return (T) cvt.ConvertFrom (value);
		}
		
		public static void SequenceMethodStart (this StringBuilder sb, string indent, string fileName, int seqNum)
		{
			sb.Append ("\n" + indent);
			sb.AppendFormat ("[Test (Description=\"Generated from sequence file {0}\")]\n", Path.GetFileName (fileName));
			sb.Append (indent);
			sb.AppendFormat ("public void Sequence_{0:0000} ()\n", seqNum);
			sb.Append (indent);
			sb.Append ("{\n");
		}

		public static void SequenceMethodEnd (this StringBuilder sb, string indent)
		{
			sb.Append (indent);
			sb.Append ("}\n");
		}
		
		public static void FormatQueueSize (this StreamWriter sw, PriorityQueueState qs)
		{
			var ti = new CacheItemPriorityQueueTestItem () {
				Operation = QueueOperation.QueueSize,
				QueueCount = qs.Queue.Count
			};
			sw.WriteLine (ti.Serialize ());
		}

		static int FindQueueIndex (PriorityQueueState qs, CacheItem item)
		{
			CacheItem ci;
			
			for (int i = 0; i < qs.Queue.Count; i++) {
				ci = ((IList)qs.Queue) [i] as CacheItem;
				if (ci == null)
					continue;

				if (ci.Guid == item.Guid)
					return i;
			}
			
			throw new ApplicationException (String.Format ("Failed to find CacheItem with UUID {0} in the queue.", item.Guid));
		}
		
		public static void FormatUpdate (this StreamWriter sw, PriorityQueueState qs, List <CacheItem> list, CacheItem updatedItem, int index)
		{
			CacheItem item = list [index];
			item.ExpiresAt = updatedItem.ExpiresAt;
			int qidx = FindQueueIndex (qs, item);
			qs.Update (qidx);
			
			var ti = new CacheItemPriorityQueueTestItem () {
				Operation = QueueOperation.Update,
				QueueCount = qs.Queue.Count,
				OperationCount = qs.UpdateCount,
				ListIndex = index,
				ExpiresAt = updatedItem.ExpiresAt,
				PriorityQueueIndex = FindQueueIndex (qs, item),
				Guid = updatedItem.Guid != null ? updatedItem.Guid.ToString () : null
			};
			sw.WriteLine (ti.Serialize ());
			qs.UpdateCount++;
		}
		
		public static void FormatDisableItem (this StreamWriter sw, PriorityQueueState qs, List <CacheItem> list, int index)
		{
			CacheItem item = list [index];
			var ti = new CacheItemPriorityQueueTestItem () {
				Operation = QueueOperation.Disable,
				QueueCount = qs.Queue.Count,
				ListIndex = index,
				PriorityQueueIndex = item.PriorityQueueIndex,
				OperationCount = qs.DisableCount
			};
			if (item == null)
				ti.IsNull = true;
			else {
				ti.Guid = item.Guid.ToString ();
				ti.IsDisabled = item.Disabled;
				ti.Disable = true;
			}
			sw.WriteLine (ti.Serialize ());
			item.Disabled = true;
			qs.DisableCount++;
		}
		
		public static void FormatDequeue (this StreamWriter sw, PriorityQueueState qs)
		{
			CacheItem item = qs.Dequeue ();
			var ti = new CacheItemPriorityQueueTestItem () {
				Operation = QueueOperation.Dequeue,
				QueueCount = qs.Queue.Count,
				OperationCount = qs.DequeueCount,
				PriorityQueueIndex = item.PriorityQueueIndex
			};
			if (item != null) {
				ti.Guid = item.Guid.ToString ();
				ti.IsDisabled = item.Disabled;
			} else
				ti.IsNull = true;
			
			sw.WriteLine (ti.Serialize ());
			qs.DequeueCount++;
		}

		public static void FormatPeek (this StreamWriter sw, PriorityQueueState qs)
		{
			CacheItem item = qs.Peek ();
			var ti = new CacheItemPriorityQueueTestItem () {
				Operation = QueueOperation.Peek,
				QueueCount = qs.Queue.Count,
				OperationCount = qs.PeekCount,
				PriorityQueueIndex = item.PriorityQueueIndex
			};
			if (item != null) {
				ti.Guid = item.Guid.ToString ();
				ti.IsDisabled = item.Disabled;
			} else
				ti.IsNull = true;
			
			sw.WriteLine (ti.Serialize ());
			qs.PeekCount++;
		}
		
		public static void FormatEnqueue (this StreamWriter sw, PriorityQueueState qs, List <CacheItem> list, int index)
		{
			CacheItem item = list [index];
			qs.Enqueue (item);

			var ti = new CacheItemPriorityQueueTestItem () {
				Operation = QueueOperation.Enqueue,
				QueueCount = qs.Queue.Count,
				ListIndex = index,
				Guid = qs.Peek ().Guid.ToString (),
				OperationCount = qs.EnqueueCount,
				PriorityQueueIndex = item.PriorityQueueIndex
			};
			
			sw.WriteLine (ti.Serialize ());
			qs.EnqueueCount++;
		}
		
		public static void FormatList (this StreamWriter sw, List <CacheItem> list)
		{
			if (list == null || list.Count == 0) {
				sw.WriteLine ("# No CacheItems found!");
				return;
			}

			sw.WriteLine ("# Each row contains TestCacheItem fields, one item per line, in the following order:");
			sw.WriteLine ("# Key, AbsoluteExpiration, SlidingExpiration, Priority, LastChange, ExpiresAt, Disabled, Guid, PriorityQueueIndex");

			foreach (CacheItem ci in list)
				CreateNewCacheItemInstanceCode (sw, ci);
		}

		static void CreateNewCacheItemInstanceCode (StreamWriter sw, CacheItem item)
		{
			sw.Write ("{0},", item.Key.Replace ("\n", "\\n").Replace ("\r", "\\r").Replace (",", "&comma;"));
			sw.Write ("{0},", item.AbsoluteExpiration.Ticks);
			sw.Write ("{0},", item.SlidingExpiration.Ticks);
			sw.Write ("{0},", (int)item.Priority);
			sw.Write ("{0},", item.LastChange.Ticks);
			sw.Write ("{0},", item.ExpiresAt);
			sw.Write ("{0},", item.Disabled.ToString ().ToLowerInvariant ());
			sw.Write ("{0},", item.Guid.ToString ());
			sw.Write ("{0}", item.PriorityQueueIndex);
			sw.WriteLine ();
		}
	}
}
