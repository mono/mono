//
// System.Data.ObjectSpaces.PersistenceException
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
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

#if NET_2_0

using System.Collections;
using System.Runtime.Serialization;

namespace System.Data.ObjectSpaces {
	public class PersistenceException : ObjectException
	{
		#region Fields

		ArrayList errors;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public PersistenceException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			this.errors = new ArrayList ();
		}

		[MonoTODO]
		public PersistenceException ()
			: base ("A PersistenceException has occurred.")
		{
			this.errors = new ArrayList ();
		}

		[MonoTODO]
		public PersistenceException (PersistenceError[] errors, string s)
			: base (s)
		{
			this.errors = new ArrayList (errors);
		}

		[MonoTODO]
		public PersistenceException (PersistenceError[] errors, string s, Exception innerException)
			: base (s, innerException)
		{
			this.errors = new ArrayList (errors);
		}

		#endregion // Constructors

		#region Properties

		public ArrayList Errors {
			get { return errors; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		#endregion Methods
	}
}

#endif
