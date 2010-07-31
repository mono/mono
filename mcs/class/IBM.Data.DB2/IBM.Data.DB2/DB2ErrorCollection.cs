
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
using System.Text;

namespace IBM.Data.DB2
{
	[Serializable()]
	public class DB2ErrorCollection : CollectionBase 
	{
        
		public DB2ErrorCollection(short sqlHandleType, IntPtr sqlHandle) 
		{
			StringBuilder sqlState = new StringBuilder(10);
			StringBuilder errorMessage = new StringBuilder(1025);

			int sqlReturn;
			short recNum=1;

			do
			{
				int nativeError;
				short errorMessageLength;
				sqlReturn = DB2CLIWrapper.SQLGetDiagRec(sqlHandleType, sqlHandle, recNum++, sqlState, out nativeError, errorMessage, errorMessage.Capacity - 1, out errorMessageLength);
				if(sqlReturn == 0)
				{
					Add(new DB2Error(errorMessage.ToString(), sqlState.ToString(), nativeError));
				}
			}
			while (sqlReturn == 0);
		}
        
		public DB2Error this[int index] 
		{
			get 
			{
				return ((DB2Error)(List[index]));
			}
		}
		internal int Add(DB2Error value) 
		{
			return List.Add(value);
		}
        
		public void CopyTo(DB2Error[] array, int index) 
		{
			List.CopyTo(array, index);
		}
        
		/// <summary>
		///    <para>Returns an enumerator that can iterate through 
		///       the <see cref='d.DB2ErrorCollection'/> .</para>
		/// </summary>
		/// <returns><para>None.</para></returns>
		/// <seealso cref='System.Collections.IEnumerator'/>
		public new DB2ErrorEnumerator GetEnumerator() 
		{
			return new DB2ErrorEnumerator(this);
		}
        
		public class DB2ErrorEnumerator : object, IEnumerator 
		{
            
			private IEnumerator baseEnumerator;
            
			private IEnumerable temp;
            
			public DB2ErrorEnumerator(DB2ErrorCollection mappings) 
			{
				this.temp = ((IEnumerable)(mappings));
				this.baseEnumerator = temp.GetEnumerator();
			}
            
			public DB2Error Current 
			{
				get 
				{
					return ((DB2Error)(baseEnumerator.Current));
				}
			}
            
			object IEnumerator.Current 
			{
				get 
				{
					return baseEnumerator.Current;
				}
			}
            
			public bool MoveNext() 
			{
				return baseEnumerator.MoveNext();
			}
            
			bool IEnumerator.MoveNext() 
			{
				return baseEnumerator.MoveNext();
			}
            
			public void Reset() 
			{
				baseEnumerator.Reset();
			}
            
			void IEnumerator.Reset() 
			{
				baseEnumerator.Reset();
			}
		}
	
	}
}
