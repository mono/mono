//
// Mono.Messaging
//
// Authors:
//		Michael Barker (mike@middlesoft.co.uk)
//
// (C) 2008 Michael Barker
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
using System.Text.RegularExpressions;

namespace Mono.Messaging
{
	public sealed class QueueReference
	{
		private static readonly char[] DELIM = new char[] { '\\' };
		private readonly string host;
		private readonly bool isPrivate;
		private readonly string queue;
		public static readonly string LOCALHOST = ".";
		public static readonly QueueReference DEFAULT = new QueueReference (LOCALHOST, null, false);
		private static readonly string PRIVATE_STR = "private$";

		public QueueReference (string host, string queue, bool isPrivate)
		{
			this.host = host;
			this.isPrivate = isPrivate;
			this.queue = queue;
		}
		
		public string Host {
			get { 
				if (host == LOCALHOST) 
					return "localhost";
				else
					return host;
			}
		}
		
		public string Queue {
			get { 
				if (isPrivate)
					return PRIVATE_STR + @"\" + queue;
				else
					return queue;
			}
		}
		
		public bool IsPrivate {
			get { return isPrivate; }
		}
		
		public QueueReference SetHost (string host)
		{
			return new QueueReference (host, this.queue, this.isPrivate);
		}
		
		public QueueReference SetQueue (string queue)
		{
			return new QueueReference (this.host, queue, this.isPrivate);
		}


		public override bool Equals (object other)
		{
			if (other == null)
				return false;
			else if (typeof (QueueReference) != other.GetType ())
				return false;
			else {
				QueueReference qr = (QueueReference) other;
				return Equals (qr);
			}
		}

		public bool Equals (QueueReference other)
		{
			return host == other.host 
				&& isPrivate == other.isPrivate
				&& queue == other.queue;
		}
		
		public override int GetHashCode ()
		{
			return queue == null ? 0 : queue.GetHashCode () + host.GetHashCode ();
		}
		
		public static QueueReference Parse (string path)
		{
			string trimedPath = RemoveLeadingSlashes (path);
			string[] parts = trimedPath.Split (DELIM, 3);
			
			if (parts.Length == 0) {
				throw new ArgumentException ();
			} else if (parts.Length == 1) {
				return new QueueReference (QueueReference.LOCALHOST, parts[0], false);
			} else if (parts.Length == 2) {
				return new QueueReference (parts[0], parts[1], false);
			} else {
				return new QueueReference (parts[0], parts[2], IsPrivateStr (parts[1]));
			}
		}
		
		public static bool IsPrivateStr (string s)
		{
			return PRIVATE_STR == s.ToLower ();
		}
		
		public static string RemoveLeadingSlashes (string s)
		{
			int idx = 0;
			while (idx < s.Length && (s[idx] == '\\'))
				idx++;
			return s.Substring (idx);
		}
		
		public override string ToString ()
		{
			if (IsPrivate) {
				return Host + "\\" + PRIVATE_STR + "\\" + queue;
			} else {
				return Host + "\\" + Queue;
			}
		}

		public static bool operator == (QueueReference a, QueueReference b)
		{
			return a.Equals (b);
		}
		
		public static bool operator != (QueueReference a, QueueReference b)
		{
			return !a.Equals (b);
		}
	}
}

