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
		public static T GetRequiredAttribute <T> (this XPathNavigator nav, string name)
		{
			string value = nav.GetAttribute (name, String.Empty);
			if (String.IsNullOrEmpty (value))
				throw new InvalidOperationException (String.Format ("Required attribute '{0}' missing.", name));

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
		
		public static void FormatQueueSize (this StringBuilder sb, string indent, PriorityQueueState qs)
		{
			sb.Append (indent);
			var ti = new CacheItemPriorityQueueTestItem () {
				Operation = QueueOperation.QueueSize,
				QueueCount = qs.Queue.Count
			};
			sb.AppendFormat ("\"{0}\",", ti.Serialize ());
			sb.AppendLine ();
		}
		
		public static void FormatDisableItem (this StringBuilder sb, string indent, PriorityQueueState qs, List <CacheItem> list, int index)
		{
			CacheItem item = list [index];
			sb.Append (indent);

			var ti = new CacheItemPriorityQueueTestItem () {
				Operation = QueueOperation.Disable,
				QueueCount = qs.Queue.Count,
				ListIndex = index
			};
			if (item == null)
				ti.IsNull = true;
			else {
				ti.Guid = item.Guid.ToString ();
				ti.IsDisabled = item.Disabled;
				ti.Disable = true;
			}
			sb.AppendFormat ("\"{0}\",", ti.Serialize ());
			sb.AppendLine ();

			item.Disabled = true;
			
			qs.DisableCount++;
		}
		
		public static void FormatDequeue (this StringBuilder sb, string indent, PriorityQueueState qs)
		{
			CacheItem item = qs.Dequeue ();
			
			sb.Append (indent);
			var ti = new CacheItemPriorityQueueTestItem () {
				Operation = QueueOperation.Dequeue,
				QueueCount = qs.Queue.Count,
				OperationCount = qs.DequeueCount
			};
			if (item != null) {
				ti.Guid = item.Guid.ToString ();
				ti.IsDisabled = item.Disabled;
			} else
				ti.IsNull = true;
			
			sb.AppendFormat ("\"{0}\",", ti.Serialize ());
			sb.AppendLine ();
			
			qs.DequeueCount++;
		}

		public static void FormatPeek (this StringBuilder sb, string indent, PriorityQueueState qs)
		{
			CacheItem item = qs.Peek ();
			
			sb.Append (indent);
			var ti = new CacheItemPriorityQueueTestItem () {
				Operation = QueueOperation.Peek,
				QueueCount = qs.Queue.Count,
				OperationCount = qs.PeekCount
			};
			if (item != null) {
				ti.Guid = item.Guid.ToString ();
				ti.IsDisabled = item.Disabled;
			} else
				ti.IsNull = true;
			
			sb.AppendFormat ("\"{0}\",", ti.Serialize ());
			sb.AppendLine ();
			
			qs.PeekCount++;
		}
		
		public static void FormatEnqueue (this StringBuilder sb, string indent, PriorityQueueState qs, List <CacheItem> list, int index)
		{
			qs.Enqueue (list [index]);
			sb.Append (indent);

			var ti = new CacheItemPriorityQueueTestItem () {
				Operation = QueueOperation.Enqueue,
				QueueCount = qs.Queue.Count,
				ListIndex = index,
				Guid = qs.Peek ().Guid.ToString (),
				OperationCount = qs.EnqueueCount
			};
			
			sb.AppendFormat ("\"{0}\",", ti.Serialize ());
			sb.AppendLine ();
			
			qs.EnqueueCount++;
		}
		
		public static void FormatList (this StringBuilder sb, string indent, string listName, List <CacheItem> list)
		{
			if (list == null || list.Count == 0) {
				sb.AppendFormat (indent + "object[] {0} = {{};\n", listName);
				return;
			}

			sb.Append (indent + "// Each row contains TestCacheItem fields, in the following order:\n");
			sb.Append (indent + "// Key, AbsoluteExpiration, SlidingExpiration, Priority, LastChange, ExpiresAt, Disabled, Guid\n");
			sb.AppendFormat (indent + "object[] {0} = {{\n", listName);

			foreach (CacheItem ci in list)
				CreateNewCacheItemInstanceCode (indent + "\t", sb, ci);
			sb.Append (indent + "};\n");
		}

		static void CreateNewCacheItemInstanceCode (string indent, StringBuilder sb, CacheItem item)
		{
			sb.Append (indent);
			sb.AppendFormat ("\"{0}\", ", item.Key.Replace ("\n", "\\n").Replace ("\r", "\\r"));
			sb.AppendFormat ("\"{0}\", ", item.AbsoluteExpiration.ToString ());
			sb.AppendFormat ("\"{0}\", ", item.SlidingExpiration.ToString ());
			sb.AppendFormat ("CacheItemPriority.{0}, ", item.Priority);
			sb.AppendFormat ("\"{0}\", ", item.LastChange.ToString ());
			sb.AppendFormat ("{0}, ", item.ExpiresAt);
			sb.AppendFormat ("{0}, ", item.Disabled.ToString ().ToLowerInvariant ());
			sb.AppendFormat ("\"{0}\",\n", item.Guid.ToString ());
		}
	}
}
