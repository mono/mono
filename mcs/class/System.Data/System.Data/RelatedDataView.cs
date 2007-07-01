//
// System.Data.RelatedDataView
//
// Author:
//   Konstantin Triger (kostat@mainsoft.com)
//

using System;
using System.Collections;
using Mono.Data.SqlExpressions;
using System.Data.Common;

namespace System.Data
{
	/// <summary>
	/// Summary description for RelatedDataView.
	/// </summary>
	internal class RelatedDataView : DataView, IExpression
	{
		#region Fields
		
		object[] _keyValues;
		DataColumn[] _columns;

		#endregion // Fields

		#region Constructors
		internal RelatedDataView(DataColumn[] relatedColumns,object[] keyValues)
		{
			dataTable = relatedColumns[0].Table;
			rowState = DataViewRowState.CurrentRows;
			_columns = relatedColumns;
			_keyValues = keyValues;

			Open();
		}

		#endregion // Constructors

		#region Methods

		internal override IExpression FilterExpression {
			get {
				return this;
			}
		}


		#endregion // Methods

		public override bool Equals(object obj)
		{
			if (!(obj is RelatedDataView)) {
				if (base.FilterExpression == null)
					return false;
				return base.FilterExpression.Equals (obj);
			}

			RelatedDataView other = (RelatedDataView) obj;
			if (_columns.Length != other._columns.Length)
				return false;
            
			for (int i = 0; i < _columns.Length; i++)
				if (!_columns[i].Equals(other._columns [i]) ||
                    !_keyValues[i].Equals(other._keyValues [i]))
					return false;
			
			if (!other.FilterExpression.Equals (base.FilterExpression))
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			int hashCode = 0;
			for (int i = 0; i < _columns.Length; i++) {
				hashCode ^= _columns [i].GetHashCode ();
				hashCode ^= _keyValues [i].GetHashCode ();
			}

			if (base.FilterExpression != null)
				hashCode ^= base.FilterExpression.GetHashCode ();

			return hashCode;
		}

		#region IExpression Members

		public object Eval(DataRow row) {
			return EvalBoolean(row);
		}

		public bool EvalBoolean(DataRow row) {
			for (int i = 0; i < _columns.Length; i++)
				if (!row[_columns[i]].Equals(_keyValues[i]))
					return false;

			IExpression filter = base.FilterExpression;
			return filter != null ? filter.EvalBoolean(row) : true;
		}

		public bool DependsOn(DataColumn other) {
			for (int i = 0; i < _columns.Length; i++)
				if (_columns[i] == other)
					return true;

			IExpression filter = base.FilterExpression;
			return filter != null ? filter.DependsOn(other) : false;
		}
		
		void IExpression.ResetExpression()
		{
		}

		#endregion
	}
}
