//
// System.Data.SqlXml.XmlRowsetAdapter
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Data.Mapping;
using System.Xml;

namespace System.Data.SqlXml {
        public class XmlRowsetAdapter
        {
		#region Fields

		MappingSchema mapping;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public XmlRowsetAdapter ()
		{
		}

		#endregion // Constructors

		#region Properties

		public MappingSchema Mapping {
			get { return mapping; }
			set { mapping = value; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public void Fill (DataSet dataSet, string filename)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Fill (DataSet dataSet, string filename, XmlResolver resolver)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Fill (DataSet dataSet, XmlReader reader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FillSchema (DataSet dataSet)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void OnRowFilled (RowFilledEventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void OnRowFilling (RowFillingEventArgs e)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
        }
}

#endif // NET_1_2
