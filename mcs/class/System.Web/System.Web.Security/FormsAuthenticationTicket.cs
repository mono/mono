//
// System.Web.Security.FormsAuthenticationTicket
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// Copyright (c) 2005 Novell, Inc (http://www.novell.com)
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

using System.IO;
using System.Security.Permissions;

namespace System.Web.Security
{
	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[Serializable]
	public sealed class FormsAuthenticationTicket
	{
		int version;
		bool persistent;
		DateTime issue_date;
		DateTime expiration;
		string name;
		string cookie_path;
		string user_data;

		/*
		internal void ToStr ()
		{
			Console.WriteLine ("version: {0}", version);
			Console.WriteLine ("persistent: {0}", persistent);
			Console.WriteLine ("issue_date: {0}", issue_date);
			Console.WriteLine ("expiration: {0}", expiration);
			Console.WriteLine ("name: {0}", name);
			Console.WriteLine ("cookie_path: {0}", cookie_path);
			Console.WriteLine ("user_data: {0}", user_data);
		}
		*/

		internal byte [] ToByteArray ()
		{
			MemoryStream ms = new MemoryStream ();
			BinaryWriter writer = new BinaryWriter (ms);
			writer.Write (version);
			writer.Write (persistent);
			writer.Write (issue_date.Ticks);
			writer.Write (expiration.Ticks);
			writer.Write (name != null);
			if (name != null)
				writer.Write (name);

			writer.Write (cookie_path != null);
			if (cookie_path != null)
				writer.Write (cookie_path);

			writer.Write (user_data != null);
			if (user_data != null)
				writer.Write (user_data);

			writer.Flush ();
			return ms.ToArray ();
		}

		internal static FormsAuthenticationTicket FromByteArray (byte [] bytes)
		{
			if (bytes == null)
				throw new ArgumentNullException ("bytes");
			
			MemoryStream ms = new MemoryStream (bytes);
			BinaryReader reader = new BinaryReader (ms);
			FormsAuthenticationTicket ticket = new FormsAuthenticationTicket ();
			ticket.version = reader.ReadInt32 ();
			ticket.persistent = reader.ReadBoolean ();
			ticket.issue_date = new DateTime (reader.ReadInt64 ());
			ticket.expiration = new DateTime (reader.ReadInt64 ());
			if (reader.ReadBoolean ())
				ticket.name = reader.ReadString ();

			if (reader.ReadBoolean ())
				ticket.cookie_path = reader.ReadString ();

			if (reader.ReadBoolean ())
				ticket.user_data = reader.ReadString ();

			return ticket;
		}

		FormsAuthenticationTicket ()
		{
		}

		public FormsAuthenticationTicket (int version,
						  string name,
						  DateTime issueDate,
						  DateTime expiration,
						  bool isPersistent,
						  string userData)
		{
			this.version = version;
			this.name = name;
			this.issue_date = issueDate;
			this.expiration = expiration;
			this.persistent = isPersistent;
			this.user_data = userData;
			this.cookie_path = "/";
		}

		public FormsAuthenticationTicket (int version,
						  string name,
						  DateTime issueDate,
						  DateTime expiration,
						  bool isPersistent,
						  string userData,
						  string cookiePath)
		{
			this.version = version;
			this.name = name;
			this.issue_date = issueDate;
			this.expiration = expiration;
			this.persistent = isPersistent;
			this.user_data = userData;
			this.cookie_path = cookiePath;
		}

		public FormsAuthenticationTicket (string name, bool isPersistent, int timeout)
		{
			this.version = 1;
			this.name = name;
			this.issue_date = DateTime.Now;
			this.persistent = isPersistent;
			if (persistent)
				expiration = issue_date.AddYears (50);
			else
				expiration = issue_date.AddMinutes ((double) timeout);

			this.user_data = "";
			this.cookie_path = "/";
		}

		internal void SetDates (DateTime issue_date, DateTime expiration)
		{
			this.issue_date = issue_date;
			this.expiration = expiration;
		}
		
		internal FormsAuthenticationTicket Clone ()
		{
			return new FormsAuthenticationTicket   (version,
								name,
								issue_date,
								expiration,
								persistent,
								user_data,
								cookie_path);
		}

		public string CookiePath {
			get { return cookie_path; }
		}

		public DateTime Expiration {
			get { return expiration; }
		}

		public bool Expired {
			get { return DateTime.UtcNow > expiration.ToUniversalTime(); }
		}

		public bool IsPersistent {
			get { return persistent; }
		}

		public DateTime IssueDate {
			get { return issue_date; }
		}

		public string Name {
			get { return name; }
		}

		public string UserData {
			get { return user_data; }
		}

		public int Version {
			get { return version; }
		}
	}
}

