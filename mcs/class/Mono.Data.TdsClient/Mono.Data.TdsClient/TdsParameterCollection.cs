//
// Mono.Data.TdsClient.TdsParameterCollection.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc 2002
// Copyright (C) Tim Coleman, 2002
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

using Mono.Data.Tds;
using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Collections;

namespace Mono.Data.TdsClient {
	[ListBindable (false)]
	public sealed class TdsParameterCollection : MarshalByRefObject, IDataParameterCollection, IList, ICollection, IEnumerable
	{
		#region Fields

		ArrayList list = new ArrayList();
		TdsMetaParameterCollection metaParameters;
		TdsCommand command;

		#endregion // Fields

		#region Constructors

		internal TdsParameterCollection (TdsCommand command)
		{
			this.command = command;
			metaParameters = new TdsMetaParameterCollection ();
		}

		#endregion // Constructors

		#region Properties

		public int Count {
			get { return list.Count; }			  
		}

		public TdsParameter this [int index] {
			get { return (TdsParameter) list [index]; }			  
			set { list [index] = (TdsParameter) value; }			  
		}

		object IDataParameterCollection.this [string parameterName] {
			get { return this[parameterName]; }
			set { 
				if (!(value is TdsParameter))
					throw new InvalidCastException ("Only SQLParameter objects can be used.");
				this [parameterName] = (TdsParameter) value;
			}
		}

		public TdsParameter this [string parameterName] {
			get {
				foreach (TdsParameter p in list)
					if (p.ParameterName.Equals (parameterName))
						return p;
				throw new IndexOutOfRangeException ("The specified name does not exist: " + parameterName);
			}	  
			set {	
				if (!Contains (parameterName))
					throw new IndexOutOfRangeException("The specified name does not exist: " + parameterName);
				this [IndexOf (parameterName)] = value;
			}			  
		}

		object IList.this [int index] {
			get { return (TdsParameter) this [index]; }
			set { this [index] = (TdsParameter) value; }
		}

		bool IList.IsFixedSize {
			get { return list.IsFixedSize; }
		}

		bool IList.IsReadOnly {
			get { return list.IsReadOnly; }
		}

		bool ICollection.IsSynchronized {
			get { return list.IsSynchronized; }
		}

		object ICollection.SyncRoot {
			get { return list.SyncRoot; }
		}

		internal TdsMetaParameterCollection MetaParameters {
			get { return metaParameters; }
		}
		
		#endregion // Properties

		#region Methods

		public int Add (object value)
		{
			if (!(value is TdsParameter))
				throw new InvalidCastException ("The parameter was not an TdsParameter.");
			Add ((TdsParameter) value);
			return IndexOf (value);
		}
		
		public TdsParameter Add (TdsParameter value)
		{
			if (value.Container != null)
				throw new ArgumentException ("The TdsParameter specified in the value parameter is already added to this or another TdsParameterCollection.");
			
			value.Container = this;
			list.Add (value);
			metaParameters.Add (value.MetaParameter);
			return value;
		}
		
		public TdsParameter Add (string parameterName, object value)
		{
			return Add (new TdsParameter (parameterName, value));
		}
		
		public TdsParameter Add (string parameterName, TdsType sybaseType)
		{
			return Add (new TdsParameter (parameterName, sybaseType));
		}

		public TdsParameter Add (string parameterName, TdsType sybaseType, int size)
		{
			return Add (new TdsParameter (parameterName, sybaseType, size));
		}

		public TdsParameter Add (string parameterName, TdsType sybaseType, int size, string sourceColumn)
		{
			return Add (new TdsParameter (parameterName, sybaseType, size, sourceColumn));
		}

		public void Clear()
		{
			metaParameters.Clear ();
			list.Clear ();
		}
		
		public bool Contains (object value)
		{
			if (!(value is TdsParameter))
				throw new InvalidCastException ("The parameter was not an TdsParameter.");
			return Contains (((TdsParameter) value).ParameterName);
		}

		public bool Contains (string value)
		{
			foreach (TdsParameter p in list)
				if (p.ParameterName.Equals (value))
					return true;
			return false;
		}

		public void CopyTo (Array array, int index)
		{
			list.CopyTo (array, index);
		}

		public IEnumerator GetEnumerator()
		{
			return list.GetEnumerator ();
		}
		
		public int IndexOf (object value)
		{
			if (!(value is TdsParameter))
				throw new InvalidCastException ("The parameter was not an TdsParameter.");
			return IndexOf (((TdsParameter) value).ParameterName);
		}
		
		public int IndexOf (string parameterName)
		{
			return list.IndexOf (parameterName);
		}

		public void Insert (int index, object value)
		{
			list.Insert (index, value);
		}

		public void Remove (object value)
		{
			metaParameters.Remove (((TdsParameter) value).MetaParameter);
			list.Remove (value);
		}

		public void RemoveAt (int index)
		{
			metaParameters.RemoveAt (index);
			list.RemoveAt (index);
		}

		public void RemoveAt (string parameterName)
		{
			RemoveAt (IndexOf (parameterName));
		}

		#endregion // Methods	
	}
}
