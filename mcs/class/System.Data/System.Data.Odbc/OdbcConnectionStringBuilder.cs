//
// System.Data.Odbc.OdbcConnectionStringBuilder
//
// Authors: 
//	  Nidhi Rawal (rawalnidhi_rawal@yahoo.com)
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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

#if NET_2_0
using System;
using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Reflection;
using System.Text;

namespace System.Data.Odbc
{
	[DefaultProperty ("Driver")]
	[TypeConverter ("System.Data.Odbc.OdbcConnectionStringBuilder+OdbcConnectionStringBuilderConverter, " + Consts.AssemblySystem_Data)]
	public sealed class OdbcConnectionStringBuilder : DbConnectionStringBuilder
	{
		#region Fields
		string driver;
		string dsn;
		#endregion //Fields

		#region Constructors

		public OdbcConnectionStringBuilder () : base (true)
		{
		}

		public OdbcConnectionStringBuilder (string connectionString) : base (true)
		{
			if (connectionString == null) {
				base.ConnectionString = string.Empty;
				return;
			}

			base.ConnectionString = connectionString;
		}

		#endregion // Constructors

		#region Properties

		public override Object this [string keyword] {
			get {
				if (keyword == null)
					throw new ArgumentNullException ("keyword");
				if (string.Compare (keyword, "Driver", StringComparison.InvariantCultureIgnoreCase) == 0)
					return Driver;
				if (string.Compare (keyword, "Dsn", StringComparison.InvariantCultureIgnoreCase) == 0)
					return Dsn;
				return base [keyword];
			}
			set {
				if (value == null) {
					Remove (keyword);
					return;
				}

				if (keyword == null)
					throw new ArgumentNullException ("keyword");

				string text_value = value.ToString ();

				if (string.Compare (keyword, "Driver", StringComparison.InvariantCultureIgnoreCase) == 0) {
					Driver = text_value;
					return;
				} else if (string.Compare (keyword, "Dsn", StringComparison.InvariantCultureIgnoreCase) == 0) {
					dsn = text_value;
				} else if (value.ToString ().IndexOf (';') != -1) {
					text_value = "{" + text_value + "}";
				}
				base [keyword] = value;
			}
		}

		public override ICollection Keys {
			get {
				List<string> keys = new List<string> ();
				keys.Add ("Dsn");
				keys.Add ("Driver");

				ICollection base_keys = base.Keys;
				foreach (string keyword in base_keys) {
					if (string.Compare (keyword, "Driver", StringComparison.InvariantCultureIgnoreCase) == 0)
						continue;
					if (string.Compare (keyword, "Dsn", StringComparison.InvariantCultureIgnoreCase) == 0)
						continue;
					keys.Add (keyword);
				}

				string [] final = new string [keys.Count];
				keys.CopyTo (final);
				return final;
			}
		}

		[DisplayName ("Driver")]
		[RefreshProperties (RefreshProperties.All)]
		public string Driver {
			get {
				if (driver == null)
					return string.Empty;
				return driver;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("Driver");
				driver = value;

				if (value.Length > 0) {
					int startBrace = value.IndexOf ('{');
					int endBrace = value.IndexOf ('}');
					if (startBrace == -1 || endBrace == -1)
						value = "{" + value + "}";
					else if (startBrace > 0 || endBrace < (value.Length - 1))
						value = "{" + value + "}";
				}
				base ["Driver"] = value;
			}
		}
		
		[DisplayName ("Dsn")]
		[RefreshProperties (RefreshProperties.All)]
		public string Dsn {
			get {
				if (dsn == null)
					return string.Empty;
				return dsn;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("Dsn");
				dsn = value;
				base ["Dsn"] = dsn;
			}
		}

		#endregion // Properties

		#region Methods

		public override bool ContainsKey (string keyword)
		{
			if (keyword == null)
				throw new ArgumentNullException ("keyword");
			if (string.Compare (keyword, "Driver", StringComparison.InvariantCultureIgnoreCase) == 0)
				return true;
			if (string.Compare (keyword, "Dsn", StringComparison.InvariantCultureIgnoreCase) == 0)
				return true;
			return base.ContainsKey (keyword);
		}

		public override bool Remove (string keyword)
		{
			if (keyword == null)
				throw new ArgumentNullException ("keyword");
			if (string.Compare (keyword, "Driver", StringComparison.InvariantCultureIgnoreCase) == 0)
				driver = string.Empty;
			else if (string.Compare (keyword, "Dsn", StringComparison.InvariantCultureIgnoreCase) == 0)
				dsn = string.Empty;
			return base.Remove (keyword);
		}

		public override void Clear ()
		{
			driver = null;
			dsn = null;
			base.Clear ();
		}

		public override bool TryGetValue (string keyword, out Object value)
		{
			if (keyword == null )
				throw new ArgumentNullException ("keyword");
			bool found = base.TryGetValue (keyword, out value);
			if (found)
				return found;
			if (string.Compare (keyword, "Driver", StringComparison.InvariantCultureIgnoreCase) == 0) {
				value = string.Empty;
				return true;
			} else if (string.Compare (keyword, "Dsn", StringComparison.InvariantCultureIgnoreCase) == 0) {
				value = string.Empty;
				return true;
			}
			return false;
		}

		#endregion // Methods
	}
}
#endif // NET_2_0 using
