//
// System.Data.OleDb.OleDbCommandBuilder
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// Copyright (C) Rodrigo Moya, 2002
//

using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace System.Data.OleDb
{
	/// <summary>
	/// Provides a means of automatically generating single-table commands used to reconcile changes made to a DataSet with the associated database. This class cannot be inherited.
	/// </summary>
	public sealed class OleDbCommandBuilder : Component
	{
		private OleDbDataAdapter m_adapter = null;
		
		public OleDbCommandBuilder ()
		{
		}

		public OleDbCommandBuilder (OleDbDataAdapter adapter) : this ()
		{
			m_adapter = adapter;
		}

		public OleDbDataAdapter DataAdapter
		{
			get {
				return m_adapter;
			}
			set {
				m_adapter = value;
			}
		}
	}
}
