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
