//
// System.Data.ProviderBase.DbParameterBase
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Data.Common;

namespace System.Data.ProviderBase {
	public abstract class DbParameterBase : DbParameter
	{
		#region Constructors
	
		[MonoTODO]
		protected DbParameterBase ()
		{
		}

		[MonoTODO]
		protected DbParameterBase (DbParameterBase source)
		{
		}

		#endregion // Constructors

		#region Properties

		[MonoTODO]
		protected object CoercedValue {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override ParameterDirection Direction {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override bool IsNullable {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override int Offset {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override string ParameterName {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override byte Precision {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override byte Scale {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override int Size {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override string SourceColumn {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override DataRowVersion SourceVersion {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override object Value {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override void CopyTo (DbParameter destination)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void PropertyChanging ()
		{
			throw new NotImplementedException ();
		}

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

		[MonoTODO]
		protected bool ShouldSerializeSize ()
		{
			throw new NotImplementedException ();
		}

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
		protected virtual byte ValueSize (object value)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}

#endif
