//
// Mono.Data.SybaseClient.SybaseParameterCollection.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (monodanmorg@yahoo.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc 2002
// Copyright (C) Tim Coleman, 2002
// Copyright (C) Daniel Morgan, 2008
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

namespace Mono.Data.SybaseClient {
	[ListBindable (false)]
#if NET_2_0
	public sealed class SybaseParameterCollection : DbParameterCollection, IDataParameterCollection, IList, ICollection, IEnumerable
#else
	public sealed class SybaseParameterCollection : MarshalByRefObject, IDataParameterCollection, IList, ICollection, IEnumerable
#endif // NET_2_0
	{
		#region Fields

		ArrayList list = new ArrayList();
		TdsMetaParameterCollection metaParameters;
		SybaseCommand command;

		#endregion // Fields

		#region Constructors

		internal SybaseParameterCollection (SybaseCommand command)
		{
			this.command = command;
			metaParameters = new TdsMetaParameterCollection ();
		}

		#endregion // Constructors

		#region Properties

		public
#if NET_2_0
		override
#endif // NET_2_0
		int Count {
			get { return list.Count; }
		}

#if NET_2_0
		public override bool IsFixedSize {
			get {
				return list.IsFixedSize;
			}
		}

		public override bool IsReadOnly {
			get {
				return list.IsReadOnly;
			}
		}

		public override bool IsSynchronized {
			get {
				return list.IsSynchronized;
			}
		}

		public override object SyncRoot {
			get {
				return list.SyncRoot;
			}
		}
#else
		object IList.this [int index] {
			get { return (SybaseParameter) this [index]; }
			set { this [index] = (SybaseParameter) value; }
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

		object IDataParameterCollection.this [string index] {
			get { return this [index]; }
			set {
				if (!(value is SybaseParameter))
					throw new InvalidCastException ("Only SQLParameter objects can be used.");
				this [index] = (SybaseParameter) value;
			}
		}
#endif

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]	
		public
#if NET_2_0
		new
#endif // NET_2_0
		SybaseParameter this [int index] {
			get {
				if (index < 0 || index >= list.Count)
					throw new IndexOutOfRangeException ("The specified index is out of range.");
				return (SybaseParameter) list [index];
			}
			set {
				if (index < 0 || index >= list.Count)
					throw new IndexOutOfRangeException ("The specified index is out of range.");
				list [index] = (SybaseParameter) value;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]	
		public
#if NET_2_0
		new
#endif // NET_2_0
		SybaseParameter this [string parameterName] {
			get {
				foreach (SybaseParameter p in list)
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

#if NET_2_0
		protected override DbParameter GetParameter (int index)
		{
			return this [index];
		}

		protected override DbParameter GetParameter (string parameterName)
		{
			return this [parameterName];
		}
	
		protected override void SetParameter (int index, DbParameter value)
		{
			this [index] = (SybaseParameter) value;
		}

		protected override void SetParameter (string parameterName, DbParameter value)
		{
			this [parameterName] = (SybaseParameter) value;
		}
#endif

		internal TdsMetaParameterCollection MetaParameters {
			get { return metaParameters; }
		}

		#endregion // Properties

		#region Methods

		public
#if NET_2_0
		override
#endif // NET_2_0
		int Add (object value)
		{
			if (!(value is SybaseParameter))
				throw new InvalidCastException ("The parameter was not an SybaseParameter.");
			Add ((SybaseParameter) value);
			return IndexOf (value);
		}

		public SybaseParameter Add (SybaseParameter value)
		{
			if (value.Container != null)
				throw new ArgumentException ("The SybaseParameter specified in the value parameter is already added to this or another SybaseParameterCollection.");
			
			value.Container = this;
			list.Add (value);
			metaParameters.Add (value.MetaParameter);
			return value;
		}

#if NET_2_0
		[Obsolete ("Do not call this method.")]
#endif // NET_2_0				
		public SybaseParameter Add (string parameterName, object value)
		{
			return Add (new SybaseParameter (parameterName, value));
		}

#if NET_2_0
		public SybaseParameter AddWithValue (string parameterName, object value)
		{
			return Add (new SybaseParameter (parameterName, value));
		}
#endif // NET_2_0
		
		public SybaseParameter Add (string parameterName, SybaseType sybaseType)
		{
			return Add (new SybaseParameter (parameterName, sybaseType));
		}

		public SybaseParameter Add (string parameterName, SybaseType sybaseType, int size)
		{
			return Add (new SybaseParameter (parameterName, sybaseType, size));
		}

		public SybaseParameter Add (string parameterName, SybaseType sybaseType, int size, string sourceColumn)
		{
			return Add (new SybaseParameter (parameterName, sybaseType, size, sourceColumn));
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		void Clear()
		{
			metaParameters.Clear ();
			list.Clear ();
		}

		public
#if NET_2_0
		override
#endif // NET_2_0		
		bool Contains (object value)
		{
			if (!(value is SybaseParameter))
				throw new InvalidCastException ("The parameter was not an SybaseParameter.");
			return Contains (((SybaseParameter) value).ParameterName);
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		bool Contains (string value)
		{
			foreach (SybaseParameter p in list)
				if (p.ParameterName.Equals (value))
					return true;
			return false;
		}

#if NET_2_0
		public bool Contains (SybaseParameter value)
		{
			return (this.IndexOf(value) != -1);
		}
#endif // NET_2_0


		public
#if NET_2_0
		override
#endif // NET_2_0
		void CopyTo (Array array, int index)
		{
			list.CopyTo (array, index);
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		IEnumerator GetEnumerator()
		{
			return list.GetEnumerator ();
		}

		public
#if NET_2_0
		override
#endif // NET_2_0		
		int IndexOf (object value)
		{
			if (!(value is SybaseParameter))
				throw new InvalidCastException ("The parameter was not an SybaseParameter.");
			return IndexOf (((SybaseParameter) value).ParameterName);
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		int IndexOf (string parameterName)
		{
			return list.IndexOf (parameterName);
		}

#if NET_2_0
		public int IndexOf (SybaseParameter value)
		{
			return list.IndexOf(value);
		}
#endif // NET_2_0

		public
#if NET_2_0
		override
#endif // NET_2_0
		void Insert (int index, object value)
		{
			list.Insert (index, value);
		}

#if NET_2_0
		public void Insert (int index, SybaseParameter value)
		{
			list.Insert (index,value);
		}
#endif //NET_2_0

		public
#if NET_2_0
		override
#endif // NET_2_0
		void Remove (object value)
		{
			metaParameters.Remove (((SybaseParameter) value).MetaParameter);
			list.Remove (value);
		}

#if NET_2_0
		public void Remove (SybaseParameter value)
		{
			//both this and the above code are the same. but need to work with
			// 1.1!
			value.Container = null;
			metaParameters.Remove (value.MetaParameter);
			list.Remove (value);
		}
#endif //NET_2_0

		public
#if NET_2_0
		override
#endif // NET_2_0
		void RemoveAt (int index)
		{
			metaParameters.RemoveAt (index);
			list.RemoveAt (index);
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		void RemoveAt (string parameterName)
		{
			RemoveAt (IndexOf (parameterName));
		}

#if NET_2_0
		public override void AddRange (Array values)
		{
			if (values == null)
				throw new ArgumentNullException("The argument passed was null");
			foreach (object value in values) {
				if (!(value is SybaseParameter))
					throw new InvalidCastException ("Element in the array parameter was not an SqlParameter.");
				SybaseParameter param = (SybaseParameter) value;
				if (param.Container != null)
					throw new ArgumentException ("An SqlParameter specified in the array is already added to this or another SqlParameterCollection.");
				param.Container = this;
				list.Add (param);
				metaParameters.Add (param.MetaParameter);
			}
		}

		public void AddRange (SybaseParameter[] values)
		{
			AddRange((Array) values);
		}
		
		public void CopyTo (SybaseParameter[] array, int index)
		{
			list.CopyTo (array, index);
		}
#endif

		#endregion // Methods	
	}
}
