//
// System.Runtime.Versioning.FrameworkName class
//
// Authors
//	Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2009 Novell, Inc (http://novell.com)
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
using System.Text;

#if NET_4_0
namespace System.Runtime.Versioning
{
	[Serializable]
	public sealed class FrameworkName : IEquatable <FrameworkName>
	{		
		string fullName;
		int? hashCode;
		
		public string FullName {
			get {
				if (fullName == null) {
					var sb = new StringBuilder (Identifier);
					sb.Append (",Version=v");
					sb.Append (Version.ToString ());

					string profile = Profile;
					if (!String.IsNullOrEmpty (profile)) {
						sb.Append (",Profile=");
						sb.Append (profile);
					}

					fullName = sb.ToString ();
				}

				return fullName;
			}
		}
		
		public string Identifier {
			get; private set;
		}

		public string Profile {
			get; private set;
		}
		
		public Version Version {
			get; private set;
		}
		
		public FrameworkName (string frameworkName)
		{
			if (frameworkName == null)
				throw new ArgumentNullException ("frameworkName");

			if (frameworkName.Length == 0)
				throw new ArgumentException ("The parameter 'frameworkName' cannot be an empty string.", "frameworkName");

			this.Profile = String.Empty;
			ParseFrameworkName (frameworkName);
		}

		public FrameworkName (string identifier, Version version)
			: this (identifier, version, String.Empty)
		{
		}

		public FrameworkName (string identifier, Version version, string profile)
		{
			if (identifier == null)
				throw new ArgumentNullException ("identifier");

			if (version == null)
				throw new ArgumentNullException ("version");

			if (identifier.Length == 0)
				throw new ArgumentException ("The parameter 'identifier' cannot be an empty string.", "identifier");
			
			this.Identifier = identifier;
			this.Version = version;
			if (profile == null)
				this.Profile = String.Empty;
			else
				this.Profile = profile;
		}

		public bool Equals (FrameworkName other)
		{
			if (Object.ReferenceEquals (other, null))
				return false;

			return (other.Version == this.Version &&
				String.Compare (other.Identifier, this.Identifier, StringComparison.Ordinal) == 0 &&
				String.Compare (other.Profile, this.Profile, StringComparison.Ordinal) == 0);
		}

		public override bool Equals (object obj)
		{
			return Equals (obj as FrameworkName);
		}

		public override int GetHashCode ()
		{
			if (hashCode == null) {
				hashCode = Version.GetHashCode () ^ Identifier.GetHashCode ();
				string profile = Profile;
				if (profile != null)
					hashCode ^= profile.GetHashCode ();
			}
			
			return (int)hashCode;
		}

		public override string ToString ()
		{
			return FullName;
		}

		public static bool operator == (FrameworkName left, FrameworkName right)
		{
			if (((object)left) == null && ((object)right) == null)
				return true;

			if (((object)left) == null || ((object)right) == null)
				return false;

			return left.Equals (right);
		}

		public static bool operator != (FrameworkName left, FrameworkName right)
		{
			if (((object)left) == null && ((object)right) == null)
				return false;

			if (((object)left) == null || ((object)right) == null)
				return true;

			return !left.Equals (right);
		}
		
		void ParseFrameworkName (string frameworkName)
		{
			string[] parts = frameworkName.Split (',');
			int len = parts.Length;

			if (len < 2 || len > 3)
				throw new ArgumentException ("FrameworkName cannot have less than two components or more than three components.");

			bool invalid = false;
			string part;
			string[] splitPart;
			int splen;
			
			for (int i = 0; i < len; i++) {
				part = parts [i].Trim ();
				if (part.Length == 0) {
					invalid = true;
					break;
				}

				splitPart = part.Split ('=');
				splen = splitPart.Length;
				
				if (String.Compare ("version", splitPart [0], StringComparison.OrdinalIgnoreCase) == 0) {
					if (i == 0 || splen != 2) {
						invalid = true;
						break;
					}					

					try {
						char first = splitPart [1][0];
						if (first == 'v' || first == 'V')
							splitPart [1] = splitPart [1].Substring (1);
						this.Version = new Version (splitPart [1]);
					} catch (Exception ex) {
						throw new ArgumentException ("FrameworkName version component is invalid.", ex);
					}					

					continue;
				}

				if (String.Compare ("profile", splitPart [0], StringComparison.OrdinalIgnoreCase) == 0) {
					if (i == 0) {
						invalid = true;
						break;
					}

					if (splen > 1)
						Profile = String.Join ("=", splitPart, 1, splen - 1);
					
					continue;
				}

				if (i == 0) {
					Identifier = part;
					continue;
				}

				invalid = true;
				break;
			}

			if (invalid)
				throw new ArgumentException ("FrameworkName is invalid.");

			if (Version == null)
				throw new ArgumentException ("FrameworkName version component is missing.");
			
		}
	}
}
#endif