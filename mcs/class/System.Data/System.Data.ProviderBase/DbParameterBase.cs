//
// System.Data.ProviderBase.DbParameterBase
//
// Author:
//   Sureshkumar T (tsureshkumar@novell.com)
//   Tim Coleman (tim@timcoleman.com)
//   Boris Kirzner <borisk@mainsoft.com>
//
// Copyright (C) Tim Coleman, 2003
//
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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

#if NET_2_0 || TARGET_JVM

using System.Data.Common;

namespace System.Data.ProviderBase {
	public abstract class DbParameterBase : DbParameter
	{
                #region Fields
		string _parameterName;
                ParameterDirection _direction = ParameterDirection.Input;
		int _size;
#if NET_2_0
		byte _precision;
		byte _scale;
		DataRowVersion _sourceVersion;
#endif
		object _value;
		bool _isNullable;
		int _offset;
		string _sourceColumn;
		DbParameterCollection _parent = null;

                #endregion // Fields

		#region Constructors
	
		[MonoTODO]
		protected DbParameterBase ()
		{
		}

		protected DbParameterBase (DbParameterBase source)
		{
			if (source == null) 
				throw ExceptionHelper.ArgumentNull ("source");

			source.CopyTo (this);
			ICloneable cloneable = source._value as ICloneable;
			if (cloneable != null)
				_value = cloneable.Clone ();
		}
        
		#endregion // Constructors

		#region Properties

		[MonoTODO]
		protected object CoercedValue {
			get { throw new NotImplementedException (); }
		}

		public override ParameterDirection Direction {
			get { return _direction; }
			set {
				if (_direction != value) {
					switch (value) {
							case ParameterDirection.Input:
							case ParameterDirection.Output:
							case ParameterDirection.InputOutput:
							case ParameterDirection.ReturnValue:
							{
								PropertyChanging ();
								_direction = value;
								return;
							}
					}
					throw ExceptionHelper.InvalidParameterDirection (value);
				}
			}
		}

		public override bool IsNullable {
			get { return _isNullable; }
			set { _isNullable = value; }
		}

		
		public virtual int Offset {
			get { return _offset; }
			set { _offset = value; }			
		}

		public override string ParameterName {
			get {
				if (_parameterName == null)
						return String.Empty;

				return _parameterName;
			}
			set {
				if (_parameterName != value) {
					PropertyChanging ();
					_parameterName = value;
				}
			}
		}

		public override int Size {
			get { return _size; }

			set {
				if (_size != value) {
					if (value < -1)
						throw ExceptionHelper.InvalidSizeValue (value);

					PropertyChanging ();
					_size = value;
				}
			}
		}

		
		public override string SourceColumn {
			get { 
				if (_sourceColumn == null)
					return String.Empty;

				return _sourceColumn;
			}

			set	{ _sourceColumn = value; }
		}

#if NET_2_0		
		public override DataRowVersion SourceVersion {
			get { return _sourceVersion; }
			set { _sourceVersion = value; }
		}
#endif		

		
		public override object Value {
			get { return _value; }
			set { _value = value; }
		}

		internal DbParameterCollection Parent
		{
			get { return _parent; }
			set { _parent = value; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public virtual void CopyTo (DbParameter destination)
		{
			if (destination == null)
				throw ExceptionHelper.ArgumentNull ("destination");

			DbParameterBase t = (DbParameterBase)destination;
			t._parameterName = _parameterName;			
			t._size = _size;
			t._offset = _offset;
			t._isNullable = _isNullable;
			t._sourceColumn = _sourceColumn;
			t._direction = _direction;

			if (_value is ICloneable)
                t._value = ((ICloneable) _value).Clone ();
            else
                t._value = this._value;
		}		

		public virtual void PropertyChanging ()
		{
		}

#if NET_2_0

		[MonoTODO]
		protected void ResetCoercedValue ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void ResetScale ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void SetCoercedValue ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected bool ShouldSerializePrecision ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected bool ShouldSerializeScale ()
		{
			throw new NotImplementedException ();
		}
#endif	
	
		protected bool ShouldSerializeSize ()
		{
			return (_size != 0);
		}

#if NET_2_0
		[MonoTODO]
		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual byte ValuePrecision (object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual byte ValueScale (object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual int ValueSize (object value)
		{
			throw new NotImplementedException ();
		}
#endif

		#endregion // Methods
	}
}

#endif
