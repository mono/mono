using System;

namespace WindowsApplication1.Mono
{
	/// <summary>
	/// Summary description for DataSet.
	/// </summary>
	public class DataSet
	{
		public DataSet()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public DataSet(string dataSetName);

		protected DataSet(SerializationInfo info, StreamingContext context);

		public bool CaseSensitive {get; set;}

		public string DataSetName {get; set;}

		public DataViewManager DefaultViewManager {get;}

		public bool EnforceConstraints {get; set;}

		public PropertyCollection ExtendedProperties {get;}

		public bool HasErrors {get;}

		public CultureInfo Locale {get; set;}

		public string Namespace {get; set;}

		public string Prefix {get; set;}

		public DataRelationCollection Relations {get;}

		public override ISite Site {get; set;}

		public DataTableCollection Tables {get;}


	}
}
