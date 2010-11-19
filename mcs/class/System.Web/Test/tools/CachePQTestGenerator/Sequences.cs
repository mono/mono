//
//  Sequences.cs
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
using System.IO;
using System.Text;
using System.Web.Caching;
using System.Xml;
using System.Xml.XPath;

namespace Tester
{
	static partial class Sequences
	{
		public static void Run (StringBuilder sb, string seqDir, string indent)
		{
			string[] files = Directory.GetFiles (seqDir, "*.seq");
			if (files == null || files.Length == 0)
				return;

			int seqNum = 0;
			Array.Sort <string> (files);
			foreach (string f in files)
				RunSequence (sb, indent, f, seqNum++);
		}

		static void RunSequence (StringBuilder sb, string initialIndent, string file, int seqNum)
		{
			Dictionary <Guid, int> cacheIndex;
			List <CacheItem> cacheItems;
			List <EDSequenceEntry> edSequence;

			LoadEDSequence (file, out cacheIndex, out cacheItems, out edSequence);

			if (edSequence == null || edSequence.Count == 0)
				return;


			string listName = String.Format ("list_{0:00000}", seqNum);
			string testsName = String.Format ("tests_{0:00000}", seqNum);
			sb.AppendLine ();
			sb.FormatList (initialIndent, listName, cacheItems);

			sb.AppendLine ();
			sb.AppendFormat ("{0}List <TestItem> {1} = new List <TestItem> () {{\n", initialIndent, testsName);

			string indent = initialIndent + "\t";
			int idx;
			var pq = new PriorityQueueState (listName, testsName);
			foreach (EDSequenceEntry entry in edSequence) {
				idx = cacheIndex [entry.Item.Guid];

				switch (entry.Type) {
					case EDSequenceEntryType.Enqueue:
						sb.FormatEnqueue (indent, pq, cacheItems, idx);
						break;

					case EDSequenceEntryType.Dequeue:
						sb.FormatDequeue (indent, pq);
						break;

					case EDSequenceEntryType.Disable:
						sb.FormatDisableItem (indent, pq, cacheItems, idx);
						break;

					case EDSequenceEntryType.Peek:
						sb.FormatPeek (indent, pq);
						break;
				}
			}

			sb.FormatQueueSize (indent, pq);

			while (pq.Queue.Count > 0)
				sb.FormatDequeue (indent, pq);
			sb.AppendFormat ("{0}}};", initialIndent);
			sb.AppendLine ();
			
			sb.SequenceMethodStart (initialIndent, file, seqNum);
 			sb.AppendFormat ("{0}RunTest ({1}, {2});", indent, testsName, listName);
			sb.AppendLine ();
			
			sb.SequenceMethodEnd (initialIndent);
		}

		static List <EDSequenceEntry> LoadEDSequence (string file, out Dictionary <Guid, int> cacheIndex, out List <CacheItem> cacheItems,
							      out List <EDSequenceEntry> edSequence)
		{
			var doc = new XPathDocument (file);
			XPathNavigator nav = doc.CreateNavigator (), current;
			XPathNodeIterator nodes = nav.Select ("/sequence/entry");
			CacheItem item;
			
			edSequence = new List <EDSequenceEntry> ();
			cacheIndex = new Dictionary <Guid, int> ();
			cacheItems = new List <CacheItem> ();
			
			while (nodes.MoveNext ()) {
				current = nodes.Current;
				item = CreateCacheItem (current, cacheIndex, cacheItems);
				edSequence.Add (new EDSequenceEntry (item, current.GetRequiredAttribute <EDSequenceEntryType> ("type")));
			}
			
			return null;
		}

		static CacheItem CreateCacheItem (XPathNavigator node, Dictionary <Guid, int> cacheIndex, List <CacheItem> cacheItems)
		{
			Guid guid = node.GetRequiredAttribute <Guid> ("guid");
			int idx;

			if (cacheIndex.TryGetValue (guid, out idx))
				return cacheItems [idx];
			
			var ret = new CacheItem ();

			ret.Key = node.GetRequiredAttribute <string> ("key");
			ret.AbsoluteExpiration = node.GetRequiredAttribute <DateTime> ("absoluteExpiration");
			ret.SlidingExpiration = node.GetRequiredAttribute <TimeSpan> ("slidingExpiration");
			ret.Priority = node.GetRequiredAttribute <CacheItemPriority> ("priority");
			ret.LastChange = node.GetRequiredAttribute <DateTime> ("lastChange");
			ret.ExpiresAt = node.GetRequiredAttribute <long> ("expiresAt");
			ret.Disabled = node.GetRequiredAttribute <bool> ("disabled");
			ret.Guid = guid;

			cacheItems.Add (ret);
			cacheIndex.Add (guid, cacheItems.Count - 1);
			
			return ret;
		}		
	}
}
