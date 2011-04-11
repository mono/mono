//
// Author(s):
//  Marek Habersack <mhabersack@novell.com>
//
// (C) 2010 Novell, Inc (http://novell.com)
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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;

namespace System.Web.Caching
{
	enum EDSequenceEntryType
	{
		Enqueue,
		Dequeue,
		Disable,
		Peek,
		Update
	}

#if DEBUG
	[Serializable]
	sealed class EDSequenceEntry
	{
		public readonly EDSequenceEntryType Type;
		public readonly CacheItem Item;

		public EDSequenceEntry ()
		{}
			
		public EDSequenceEntry (CacheItem item, EDSequenceEntryType type)
		{
			Type = type;
			Item = item;
		}
	}
#endif
	
	sealed partial class CacheItemPriorityQueue
	{
#if DEBUG
		Dictionary <Guid, CacheItem> items = new Dictionary <Guid, CacheItem> ();
		List <EDSequenceEntry> edSequence = new List <EDSequenceEntry> ();


		List <EDSequenceEntry> EDSequence {
			get {
				if (edSequence == null)
					edSequence = new List <EDSequenceEntry> ();
				return edSequence;
			}
		}
#endif
		
		[Conditional ("DEBUG")]
		void InitDebugMode ()
		{
#if DEBUG
			AppDomain.CurrentDomain.DomainUnload += OnDomainUnload;
#endif
		}

		[Conditional ("DEBUG")]
		void AddSequenceEntry (CacheItem item, EDSequenceEntryType type)
		{
#if DEBUG
			EDSequence.Add (new EDSequenceEntry (CopyItem (item), type));
#endif
		}
		
#if DEBUG
		CacheItem CopyItem (CacheItem item)
		{
			CacheItem newItem = new CacheItem ();

			// if (items.TryGetValue (item.Guid, out newItem)) {
			// 	newItem.PriorityQueueIndex = item.PriorityQueueIndex
			// }

			newItem.Key = item.Key;
			newItem.AbsoluteExpiration = item.AbsoluteExpiration;
			newItem.SlidingExpiration = item.SlidingExpiration;
			newItem.Priority = item.Priority;
			newItem.LastChange = item.LastChange;
			newItem.ExpiresAt = item.ExpiresAt;
			newItem.Disabled = item.Disabled;
			newItem.PriorityQueueIndex = item.PriorityQueueIndex;
			newItem.Guid = item.Guid;
			
			//items [newItem.Guid] = newItem;
			
			return newItem;
		}
		
		string CreateNewCacheItemInstanceCode (string indent, CacheItem item)
		{
			var sb = new StringBuilder (indent + "new CacheItem {");
			sb.AppendFormat ("Key = \"{0}\", ", item.Key.Replace ("\n", "\\n").Replace ("\r", "\\r"));
			sb.AppendFormat ("AbsoluteExpiration = DateTime.Parse (\"{0}\"), ", item.AbsoluteExpiration.ToString ());
			sb.AppendFormat ("SlidingExpiration = TimeSpan.Parse (\"{0}\"), ", item.SlidingExpiration.ToString ());
			sb.AppendFormat ("Priority = CacheItemPriority.{0}, ", item.Priority);
			sb.AppendFormat ("LastChange = DateTime.Parse (\"{0}\"), ", item.LastChange.ToString ());
			sb.AppendFormat ("ExpiresAt = {0}, ", item.ExpiresAt);
			sb.AppendFormat ("Disabled = {0}, ", item.Disabled.ToString ().ToLowerInvariant ());
			sb.AppendFormat ("PriorityQueueIndex = {0},", item.PriorityQueueIndex);
			sb.AppendFormat ("Guid = new Guid (\"{0}\")}}, \n", item.Guid.ToString ());

			return sb.ToString ();
		}
#endif
		
		[Conditional ("DEBUG")]
		public void OnItemDisable (CacheItem i)
		{
#if DEBUG
			CacheItem item;
			if (!items.TryGetValue (i.Guid, out item))
				return;

			EDSequence.Add (new EDSequenceEntry (CopyItem (i), EDSequenceEntryType.Disable));
#endif
		}

#if DEBUG
		void OnDomainUnload (object sender, EventArgs e)
		{
			if (EDSequence.Count == 0) {
				Console.Error.WriteLine ("No enqueue/dequeue sequence recorded.");
				return;
			}

			try {
				string filePath = Path.Combine (HttpRuntime.AppDomainAppPath, String.Format ("cache_pq_sequence_{0}_{1:x}.seq",
													     DateTime.UtcNow.ToString ("yyyy-MM-dd_hh-mm-ss"),
													     GetHashCode ()));
				var settings = new XmlWriterSettings ();
				settings.Indent = true;
				settings.IndentChars = "\t";
				settings.NewLineChars = "\n";
				settings.Encoding = Encoding.UTF8;

				Console.Error.WriteLine ("Saving sequence in file {0}", filePath);
				using (XmlWriter writer = XmlWriter.Create (filePath, settings)) {
					writer.WriteStartDocument (true);
					writer.WriteStartElement ("sequence");
					foreach (EDSequenceEntry entry in EDSequence) {
						writer.WriteStartElement ("entry");
						writer.WriteAttributeString ("type", entry.Type.ToString ());
						if (entry.Item == null)
							writer.WriteAttributeString ("isNull", "true");
						else {
							writer.WriteAttributeString ("key", entry.Item.Key != null ? entry.Item.Key.Replace ("\n", "\\n").Replace ("\r", "\\r") : "null");
							writer.WriteAttributeString ("absoluteExpiration", entry.Item.AbsoluteExpiration.Ticks.ToString ());
							writer.WriteAttributeString ("slidingExpiration", entry.Item.SlidingExpiration.Ticks.ToString ());
							writer.WriteAttributeString ("priority", entry.Item.Priority.ToString ());
							writer.WriteAttributeString ("lastChange", entry.Item.LastChange.Ticks.ToString ());
							writer.WriteAttributeString ("expiresAt", entry.Item.ExpiresAt.ToString ());
							writer.WriteAttributeString ("disabled", entry.Item.Disabled.ToString ());
							writer.WriteAttributeString ("guid", entry.Item.Guid.ToString ());
							writer.WriteAttributeString ("priorityQueueIndex", entry.Item.PriorityQueueIndex.ToString ());
						}
						writer.WriteEndElement ();
					}
					writer.WriteEndElement ();
					writer.Flush ();
				}
			} catch (Exception ex) {
 				Console.WriteLine ("Failed to write enqueue/dequeue sequence file. Exception was caught:\n{0}",
 						   ex);
 			}
		}		
#endif
	}
}
