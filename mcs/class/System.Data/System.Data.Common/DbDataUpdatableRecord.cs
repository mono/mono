//
// System.Data.Common.DbDataUpdatableRecord.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.ComponentModel;

namespace System.Data.Common {
	public class DbDataUpdatableRecord : IDataUpdatableRecord, IDataRecord, ISetTypedData, ICustomTypeDescriptor, IGetTypedData
	{
		#region Properties

		[MonoTODO]
		public virtual int FieldCount {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual object this [string x] {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual object this [int x] {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual bool Updatable {
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods


		[MonoTODO]
		public virtual bool GetBoolean (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual byte GetByte (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual long GetBytes (int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual char GetChar (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual long GetChars (int i, long dataIndex, char[] buffer, int bufferIndex, int length)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual IDataReader GetData (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string GetDataTypeName (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual DateTime GetDateTime (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual decimal GetDecimal (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual double GetDouble (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual Type GetFieldType (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual float GetFloat (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual Guid GetGuid (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual short GetInt16 (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual int GetInt32 (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual long GetInt64 (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string GetName (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual object GetObjectRef (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual int GetOrdinal (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string GetString (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual object GetValue (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual int GetValues (object[] values)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		AttributeCollection ICustomTypeDescriptor.GetAttributes ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		string ICustomTypeDescriptor.GetClassName ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		string ICustomTypeDescriptor.GetComponentName ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		TypeConverter ICustomTypeDescriptor.GetConverter ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		EventDescriptor ICustomTypeDescriptor.GetDefaultEvent ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		object ICustomTypeDescriptor.GetEditor (Type editorBaseType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		EventDescriptorCollection ICustomTypeDescriptor.GetEvents ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		EventDescriptorCollection ICustomTypeDescriptor.GetEvents (Attribute[] attributes)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties (Attribute[] attributes)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		object ICustomTypeDescriptor.GetPropertyOwner (PropertyDescriptor pd)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		int IDataUpdatableRecord.SetValues (object[] values)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool IsDBNull (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool IsSetAsDefault (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void SetBoolean (int i, bool value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void SetByte (int i, byte value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void SetBytes (int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void SetChar (int i, char value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void SetChars (int i, long dataIndex, char[] buffer, int bufferIndex, int length)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void SetDateTime (int i, DateTime value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void SetDecimal (int i, decimal value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void SetDefault (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void SetDouble (int i, double value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void SetFloat (int i, float value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void SetGuid (int i, Guid value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void SetInt16 (int i, short value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void SetInt32 (int i, int value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void SetInt64 (int i, long value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void SetObjectRef (int i, object o)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void SetString (int i, string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void SetValue (int i, object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual int SetValues (int i, object[] value)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}

#endif // NET_1_2
