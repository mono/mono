using System;
using System.ComponentModel;

namespace System.Data
{
	internal class DataRelationPropertyDescriptor : PropertyDescriptor
	{
		#region Fields

		DataRelation _relation;

		#endregion // Fields

		#region Constructors

		internal DataRelationPropertyDescriptor(DataRelation relation) : base(relation.RelationName,null)
		{
			_relation = relation;
		}

		#endregion // Constructors

		#region Properties

		public override Type ComponentType 
		{ 
			get {
				return typeof(DataRowView);
			}
		}

		public override bool IsReadOnly 
		{ 
			get {
				return false;
			}
		}

		public override Type PropertyType 
		{ 
			get {
				return typeof(IBindingList);
			}
		}

		public DataRelation Relation 
		{ 
			get {
				return _relation;
			}
		}

		#endregion // Properties

		#region Methods

		public override bool CanResetValue(object obj)
		{
			return false;
		}

		public override bool Equals(object obj)
		{
			DataRelationPropertyDescriptor descriptor = obj as DataRelationPropertyDescriptor;

			if (descriptor == null) {
				return false;
			}

			return (Relation == descriptor.Relation);
		}

		public override int GetHashCode()
		{
			return _relation.GetHashCode();
		}

        public override object GetValue(object obj)
		{
			DataRowView dr = (DataRowView)obj;
			return dr.CreateChildView(Relation);
		}

		public override void ResetValue(object obj)
		{
		}

		public override void SetValue(object obj, object val)
		{
		}

		public override bool ShouldSerializeValue(object obj)
		{
			return false;
		}

		#endregion // Methods
	}
}
