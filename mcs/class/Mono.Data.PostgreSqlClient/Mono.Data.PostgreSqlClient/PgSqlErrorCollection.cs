//
// Mono.Data.PostgreSqlClient.PgSqlError.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc 2002
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
using System.Collections;
using System.Data;
using System.Runtime.InteropServices;

namespace Mono.Data.PostgreSqlClient
{
	/// <summary>
	/// Describes an error from a SQL database.
	/// </summary>
	[MonoTODO]
	public sealed class PgSqlErrorCollection : ICollection, IEnumerable
	{
		ArrayList errorList = new ArrayList();

		internal PgSqlErrorCollection() {
		}

		internal PgSqlErrorCollection(byte theClass, int lineNumber,
			string message,	int number, string procedure,
			string server, string source, byte state) {
			
			Add (theClass, lineNumber, message,
				number, procedure,
				server, source, state);
		}

		#region Properties
                
		[MonoTODO]
		public int Count {
			get {	
				return errorList.Count;
			}			  
		}

		[MonoTODO]
		public void CopyTo(Array array,	int index) {
			throw new NotImplementedException ();
		}

		// [MonoTODO]
		bool ICollection.IsSynchronized {
			get {	
				throw new NotImplementedException ();
			}			  
		}

		// [MonoTODO]
		object ICollection.SyncRoot {
			get {	
				throw new NotImplementedException ();
			}			  
		}

		[MonoTODO]
		public IEnumerator GetEnumerator() {
			throw new NotImplementedException ();
		}
		
		// Index property (indexer)
		// [MonoTODO]
		public PgSqlError this[int index] {
			get {
				return (PgSqlError) errorList[index];
			}
		}

		#endregion

		#region Methods
		
		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException ();
		}
		#endregion

		internal void Add(PgSqlError error) {
			errorList.Add(error);
		}

		internal void Add(byte theClass, int lineNumber,
			string message,	int number, string procedure,
			string server, string source, byte state) {
			
			PgSqlError error = new PgSqlError(theClass,
				lineNumber, message,
				number, procedure,
				server, source, state);
			Add(error);
		}

		#region Destructors

		[MonoTODO]
		~PgSqlErrorCollection()
		{
			// FIXME: do the destructor - release resources
		}

		#endregion		
	}
}
