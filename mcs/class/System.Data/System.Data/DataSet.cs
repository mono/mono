// 
// System.Data/DataSet.cs
//
// Author:
//   Christopher Podurgiel <cpodurgiel@msn.com>
//   Daniel Morgan <danmorg@sc.rr.com>
//
// (C) Ximian, Inc. 2002
//

using System;
using System.Runtime.Serialization;

namespace System.Data
{
	/// <summary>
	/// an in-memory cache of data 
	/// </summary>
	[Serializable]
	public class DataSet : MarshalByValueComponent, IListSource,
		ISupportInitialize, ISerializable {
		
		#region Constructors

		[MonoTODO]
		public DataSet()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		[MonoTODO]
		public DataSet(string dataSetName) {
		}

		[MonoTODO]
		protected DataSet(SerializationInfo info, StreamingContext context) {
		}

		#endregion // Constructors

		#region Public Properties

		public bool CaseSensitive {
			[MonoTODO]
			get {
			} 
			
			[MonoTODO]
			set {
			}
		}

		public string DataSetName {
			[MonoTODO]
			get {
			} 
			
			[MonoTODO]
			set {
			}
		}

		public DataViewManager DefaultViewManager {
			[MonoTODO]
			get {
			} 
			
			[MonoTODO]
			set {
			}
		}

		public bool EnforceConstraints {
			[MonoTODO]
			get {
			} 
			
			[MonoTODO]
			set {
			}
		}

		public PropertyCollection ExtendedProperties {
			[MonoTODO]
			get {
			}
		}

		public bool HasErrors {
			[MonoTODO]
			get {
			}
		}

		public CultureInfo Locale {
			[MonoTODO]
			get { 
			}
			
			[MonoTODO]
			set {
			}
		}

		public string Namespace {
			[MonoTODO]
			get {
			} 
			
			[MonoTODO]
			set {
			}
		}

		public string Prefix {
			[MonoTODO]
			get {
			} 
			
			[MonoTODO]
			set {
			}
		}

		public DataRelationCollection Relations {
			[MonoTODO]
			get{
			}
		}

		public override ISite Site {
			[MonoTODO]
			get {
			} 
			
			[MonoTODO]
			set {
			}
		}

		public DataTableCollection Tables {
			[MonoTODO]
			get {
			}
		}

		#endregion // Public Properties

		#region Public Methods

		[Serializable]
		public void AcceptChanges() {
		}

		[Serializable]
		public void Clear() {
		}

		[Serializable]
		public virtual DataSet Clone() {
		}

		[Serializable]
		public DataSet Copy() {
		}

		[Serializable]
		public DataSet GetChanges() {
		}

		[Serializable]
		public DataSet GetChanges(DataRowState rowStates) {
		}

		[Serializable]
		public string GetXml() {
		}

		[Serializable]
		public string GetXmlSchema() {
		}

		[Serializable]
		public virtual void RejectChanges() {
		}

		[Serializable]
		public virtual void Reset() {
		}

		[Serializable]
		public void WriteXml(Stream stream) {
		}

		[Serializable]
		public void WriteXml(string fileName) {
		}

		[Serializable]
		public void WriteXml(TextWriter writer) {
		}

		[Serializable]
		public void WriteXml(XmlWriter writer) {
		}

		[Serializable]
		public void WriteXml(Stream stream, XmlWriteMode mode) {
		}

		[Serializable]
		public void WriteXml(string fileName, XmlWriteMode mode) {
		}

		[Serializable]
		public void WriteXml(TextWriter writer,	XmlWriteMode mode) {
		}

		[Serializable]
		public void WriteXml(XmlWriter writer, XmlWriteMode mode) {
		}

		[Serializable]
		public void WriteXmlSchema(Stream stream) {
		}

		[Serializable]
		public void WriteXmlSchema(string fileName) {
		}

		[Serializable]
		public void WriteXmlSchema(TextWriter writer) {
		}

		[Serializable]
		public void WriteXmlSchema(XmlWriter writer) {
		}

		#endregion // Public Methods

		#region Public Events

		[Serializable]
		public event MergeFailedEventHandler MergeFailed;

		#endregion // Public Events

		#region Destructors

		~DataSet() {
		}

		#endregion Destructors

	}
}
