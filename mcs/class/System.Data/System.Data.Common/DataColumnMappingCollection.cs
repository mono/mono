//
// System.Data.Common.DataColumnCollection
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc
//

namespace System.Data.Common
{
	/// <summary>
	/// Contains a collection of DataColumnMapping objects. This class cannot be inherited.
	/// </summary>
	public sealed class DataColumnMappingCollection :
		MarshalByRefObject, IColumnMappingCollection, IList,
		ICollection, IEnumerable
	{
		[MonoTODO]
		public DataColumnMappingCollection()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Add(object)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void AddRange(DataColumnMapping[] values)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Clear()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool Contains(object)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CopyTo(Array array, int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataColumnMapping GetByDataSetColumn(string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static DataColumnMapping GetColumnMappingBySchemaAction(
			DataColumnMappingCollection columnMappings,
			string sourceColumn,
			MissingMappingAction mappingAction)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int IndexOf(object)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int IndexOfDataSetColumn(string dataSetColumn)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Insert(int index, object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Remove(object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RemoveAt(int index)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public int Count
		{
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public DataColumnMapping this[int]
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	}
}
