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
	public sealed class OdbcConnectionStringBuilder : DbConnectionStringBuilder
	{
		#region Fields
		Dictionary<string, object> _dictionary = null;	
		#endregion // Fields		

		#region Constructors
		public OdbcConnectionStringBuilder ()
		{
			Init();
		}
        
		[MonoTODO]       
		public OdbcConnectionStringBuilder (string connectionString)
		{
			throw new NotImplementedException();
		}

		private void Init ()
		{
			_dictionary = new Dictionary<string, object> ();
		}
		#endregion // Constructors

		#region Properties
		public override Object this [string keyword]
		{
			get
			{
				if (ContainsKey (keyword))
					return _dictionary [keyword];
				else
					throw new ArgumentException ();
			}
			set { _dictionary.Add (keyword, value); }
		}
                
		public override ICollection Keys
		{
			get { return _dictionary.Keys; }
		}
		#endregion // Properties

		#region Methods
		public override bool ContainsKey (string keyword)
		{
			return _dictionary.ContainsKey (keyword);
		}
                
		public override bool Remove (string keyword)
		{
			return _dictionary.Remove (keyword);
		}

		public override void Clear()
		{
			_dictionary.Clear ();
		}

		public override bool TryGetValue (string keyword, out Object value)
		{
			bool found = false;
			if (_dictionary.ContainsKey (keyword)) {
				found = true;
			value = this [keyword];
			}
			else {
			value = null;
			found = false;
			}
			return found;
		}
		#endregion // Methods
	}
}
#endif // NET_2_0 using