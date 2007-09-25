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
using System.Text;
using System.Reflection;
using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;

using System.Data;
using System.Data.Common;
using System.Data.Odbc;

namespace System.Data.Odbc
{
	[DefaultProperty ("Driver")]
	[TypeConverter ("System.Data.Odbc.OdbcConnectionStringBuilder+OdbcConnectionStringBuilderConverter, " + Consts.AssemblySystem_Data)]
	public sealed class OdbcConnectionStringBuilder : DbConnectionStringBuilder
	{
		#region Fields
		string driver;
		string dsn;	
		bool dsnFlag = false;
		bool driverFlag = false;
		bool driverBracketFlag = false;
		#endregion //Fields

		#region Constructors
		public OdbcConnectionStringBuilder ()
		{
		}
            
            	// FIXME : Key-Value pairs returned does not match with the one returned in MS .Net
		public OdbcConnectionStringBuilder (string connectionString)
		{
			string key = "", val = "";
			if (connectionString == null) {
				base.ConnectionString = "";
			}
			else {
				string [] parameters = connectionString.Split (new char [] { ';' });
				foreach (string args in parameters) {
					if (parameters.Length == 1 && args.Trim () == "") {
						ConnectionString = "";
					} else {
						string [] arg = args.Split (new char [] { '=' }, 2);
						if (arg.Length == 2) {
							key = arg [0].Trim ();
							val = arg [1].Trim ();
							if (key == "")
								throw new ArgumentException ("Invalid value specified", key);
							if (val != "") {
								if (key == "Driver") {
									val = "{" + val + "}";
									driverBracketFlag = true;
									driverFlag = true;
								} else if (key == "Dsn")
									dsnFlag = true;
								base.Add (key.Trim (), val.Trim ());
							}
						}
					}
				}

			}
		}
		#endregion // Constructors

		#region Properties
		public override Object this [string keyword]
		{
			get {	
				if (keyword == null || keyword.Trim () == "") {
					throw new ArgumentNullException ("Keyword should not be emtpy");
				}
				if (keyword == "Driver") {
					if (base ["Driver"].ToString ().EndsWith ("}"))
						base ["Driver"] = base ["Driver"].ToString ().Remove (base ["Driver"].ToString ().Length - 1, 1);
					if (base ["Driver"].ToString ().StartsWith ("{"))
						base ["Driver"] = base ["Driver"].ToString ().Remove (0, 1);
					driverBracketFlag = false;
				}
				return base [keyword]; 
			}
			set { 
				if (keyword == null || keyword.Trim () == "") {
					throw new ArgumentNullException ("Keyword should not be emtpy");
				}
				if (keyword == "Driver") {
					value = "{" + value + "}";
					driverFlag = true;
					driverBracketFlag = true;
				} else if (keyword == "Dsn") {
					if (value == null)
						value = "";
					dsnFlag = true;
				} else if (value != null && value.ToString ().IndexOf (';') != -1) {
					value = "{" + value + "}";
				}
				base.Add (keyword, value);
			}
		}
                
		public override ICollection Keys
		{
			get { 
				return base.Keys; 
			}
		}

		[DisplayName ("Driver")]
		[RefreshProperties (RefreshProperties.All)]
		public string Driver { 
			get {
				if (ContainsKey ("Driver")) {
					driver = base ["Driver"].ToString ();
					if ((driverBracketFlag == true) && (driver.EndsWith ("}"))) 
						driver = driver.Remove (base ["Driver"].ToString ().Length - 1, 1);
					if ((driverBracketFlag == true) && (driver.StartsWith ("{"))) 
						driver = driver.Remove (0, 1);
					driverBracketFlag = false;
				}
				return driver;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("Driver");
				driver = value ;
				base ["Driver"] = "{" + driver + "}";
				driverBracketFlag = true;
				driverFlag = true;
			 }
		}
		
		[DisplayName ("Dsn")]
		[RefreshProperties (RefreshProperties.All)]
		public string Dsn { 
			get { 
				if (ContainsKey ("Dsn"))
					return base ["Dsn"].ToString ();
				else
					return dsn;
			} 
			set {
				if (value == null)
					throw new ArgumentNullException ("Dsn");
				dsn = value;
				base ["Dsn"] = dsn;
				dsnFlag = true;
			 } 
		}
		#endregion // Properties

		#region Methods
		public override bool ContainsKey (string keyword)
		{
			if (keyword == null || keyword.Trim () == "") {
				throw new ArgumentNullException ("Keyword should not be emtpy");
			}
			if ((keyword == "Driver" && driverFlag == false) || (keyword == "Dsn" && dsnFlag == false))
				return true;
			else
				return base.ContainsKey (keyword);
		}
                
		public override bool Remove (string keyword)
		{
			if (keyword == null || keyword.Trim () == "") {
				throw new ArgumentNullException ("Keyword should not be emtpy");
			}
			return base.Remove (keyword);
		}

		public override void Clear ()
		{
			base.Clear ();
		}

		public override bool TryGetValue (string keyword, out Object value)
		{
			if (keyword == null || keyword.Trim () == "") {
				throw new ArgumentNullException ("Keyword should not be emtpy");
			}
			return base.TryGetValue (keyword, out value);
		}
		#endregion // Methods	
	}
}
#endif // NET_2_0 using
