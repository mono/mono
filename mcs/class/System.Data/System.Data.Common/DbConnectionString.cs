//
// System.Data.Common.DbConnectionString
//
// Authors:
//	Tim Coleman (tim@timcoleman.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) Tim Coleman, 2003
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Text;

namespace System.Data.Common {

	[Obsolete ()]
	public class DbConnectionString : DbConnectionOptions, ISerializable {

		#region Fields

		KeyRestrictionBehavior behavior;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		protected internal DbConnectionString (DbConnectionString constr)
		{
			options = constr.options;
		}

		[MonoTODO]
		public DbConnectionString (string connectionString)
			: base (connectionString)
		{
			options = new NameValueCollection ();
			ParseConnectionString (connectionString);
		}
		
		[MonoTODO]
		protected DbConnectionString (SerializationInfo si, StreamingContext sc)
		{
		}

		[MonoTODO]
		public DbConnectionString (string connectionString, string restrictions, KeyRestrictionBehavior behavior)
			: this (connectionString)
		{
			this.behavior = behavior;
		}

		#endregion // Constructors

		#region Properties

		public KeyRestrictionBehavior Behavior {
			get { return behavior; }
		}

		[MonoTODO]
		public string Restrictions {
			get { throw new NotImplementedException (); }
		}
		
		#endregion // Properties

		#region Methods

		[MonoTODO]
		public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		protected virtual string KeywordLookup (string keyname)
		{
			return keyname;
		}

		[MonoTODO]
		public virtual void PermissionDemand ()
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}

#endif
