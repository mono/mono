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
			sb.AppendFormat ("new TestItem {{Operation = QueueOperation.QueueSize, QueueCount = {0}}},", qs.Queue.Count);
			sb.AppendLine ();
		}
		
		public static void FormatDisableItem (this StringBuilder sb, string indent, PriorityQueueState qs, List <CacheItem> list, int index)
		{
			CacheItem item = list [index];
			sb.Append (indent);
			sb.AppendFormat ("new TestItem {{Operation = QueueOperation.Disable, QueueCount = {0}, ListIndex = {1}, ", qs.Queue.Count, index);

			if (item == null) {
				sb.Append ("IsNull = true},");
				sb.AppendLine ();
				return;
			}
			sb.AppendFormat ("Guid = \"{0}\", IsDisabled = {1}, Disable = true}},", item.Guid.ToString (), item.Disabled.ToString ().ToLowerInvariant ());
			sb.AppendLine ();

			item.Disabled = true;
			
			qs.DisableCount++;
		}
		
		public static void FormatDequeue (this StringBuilder sb, string indent, PriorityQueueState qs)
		{
			CacheItem item = qs.Dequeue ();
			
			sb.Append (indent);
			sb.AppendFormat ("new TestItem {{Operation = QueueOperation.Dequeue, QueueCount = {0}, ", qs.Queue.Count);

			if (item != null)
				sb.AppendFormat ("Guid = \"{0}\", IsDisabled = {1}, ", item.Guid.ToString (), item.Disabled.ToString ().ToLowerInvariant ());
			else
				sb.Append ("IsNull = true, ");
			sb.AppendFormat ("OperationCount = {0}}},", qs.DequeueCount);
			sb.AppendLine ();
			
			qs.DequeueCount++;
		}

		public static void FormatPeek (this StringBuilder sb, string indent, PriorityQueueState qs)
		{
			CacheItem item = qs.Peek ();
			
			sb.Append (indent);
			sb.AppendFormat ("new TestItem {{Operation = QueueOperation.Peek, QueueCount = {0}, ", qs.Queue.Count);

			if (item != null)
				sb.AppendFormat ("Guid = \"{0}\", IsDisabled = {1}, ", item.Guid.ToString (), item.Disabled.ToString ().ToLowerInvariant ());
			else
				sb.Append ("IsNull = true, ");
			
			sb.AppendFormat ("OperationCount = {0}}},", qs.PeekCount);
			sb.AppendLine ();
			
			qs.PeekCount++;
		}
		
		public static void FormatEnqueue (this StringBuilder sb, string indent, PriorityQueueState qs, List <CacheItem> list, int index)
		{
			qs.Enqueue (list [index]);
			sb.Append (indent);
			sb.AppendFormat ("new TestItem {{Operation = QueueOperation.Enqueue, QueueCount = {0}, ListIndex = {1}, Guid = \"{2}\", OperationCount = {3}}},",
					 qs.Queue.Count, index, qs.Peek ().Guid.ToString (), qs.EnqueueCount);
			sb.AppendLine ();
			
			qs.EnqueueCount++;
		}

		public static void FormatList (this StringBuilder sb, string indent, string listName, List <CacheItem> list)
		{
			if (list == null || list.Count == 0) {
				sb.AppendFormat (indent + "List <TestCacheItem> {0} = new List <TestCacheItem> ();\n", listName);
				return;
			}

			sb.AppendFormat (indent + "List <TestCacheItem> {0} = new List <TestCacheItem> {{\n", listName);

			foreach (CacheItem ci in list)
				CreateNewCacheItemInstanceCode (indent + "\t", sb, ci);
			sb.Append (indent + "};\n");
		}

		static void CreateNewCacheItemInstanceCode (string indent, StringBuilder sb, CacheItem item)
		{
			sb.Append (indent + "new TestCacheItem {");
			sb.AppendFormat ("Key = \"{0}\", ", item.Key.Replace ("\n", "\\n").Replace ("\r", "\\r"));
			sb.AppendFormat ("AbsoluteExpiration = DateTime.Parse (\"{0}\"), ", item.AbsoluteExpiration.ToString ());
			sb.AppendFormat ("SlidingExpiration = TimeSpan.Parse (\"{0}\"), ", item.SlidingExpiration.ToString ());
			sb.AppendFormat ("Priority = CacheItemPriority.{0}, ", item.Priority);
			sb.AppendFormat ("LastChange = DateTime.Parse (\"{0}\"), ", item.LastChange.ToString ());
			sb.AppendFormat ("ExpiresAt = {0}, ", item.ExpiresAt);
			sb.AppendFormat ("Disabled = {0}, ", item.Disabled.ToString ().ToLowerInvariant ());
			sb.AppendFormat ("Guid = new Guid (\"{0}\")}}, \n", item.Guid.ToString ());
		}
	}
}
