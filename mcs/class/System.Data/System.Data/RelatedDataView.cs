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
			_columns = relatedColumns;
			_keyValues = keyValues;

			UpdateIndex(true);
		}

		#endregion // Constructors

		#region Methods

		internal override IExpression FilterExpression {
			get {
				return this;
			}
		}


		#endregion // Methods

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

		#endregion
	}
}
