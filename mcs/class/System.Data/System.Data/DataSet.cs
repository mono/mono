// 
// System.Data/DataSet.cs
//
// Author:
//   Christopher Podurgiel <cpodurgiel@msn.com>
//   Daniel Morgan <danmorg@sc.rr.com>
//   Rodrigo Moya <rodrigo@ximian.com>
//
// (C) Ximian, Inc. 2002
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

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
				throw new NotImplementedException ();
			} 
			
			[MonoTODO]
			set {
			}
		}

		public string DataSetName {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			} 
			
			[MonoTODO]
			set {
			}
		}

		public DataViewManager DefaultViewManager {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			} 
			
			[MonoTODO]
			set {
			}
		}

		public bool EnforceConstraints {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			} 
			
			[MonoTODO]
			set {
			}
		}

		public PropertyCollection ExtendedProperties {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public bool HasErrors {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public CultureInfo Locale {
			[MonoTODO]
			get { 
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			set {
			}
		}

		public string Namespace {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			} 
			
			[MonoTODO]
			set {
			}
		}

		public string Prefix {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			} 
			
			[MonoTODO]
			set {
			}
		}

		public DataRelationCollection Relations {
			[MonoTODO]
			get{
				throw new NotImplementedException ();
			}
		}

		public override ISite Site {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			} 
			
			[MonoTODO]
			set {
			}
		}

		public DataTableCollection Tables {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		#endregion // Public Properties

		#region Public Methods

		public void AcceptChanges()
		{
			throw new NotImplementedException ();
		}

		public void Clear()
		{
			throw new NotImplementedException ();
		}

		public virtual DataSet Clone()
		{
			throw new NotImplementedException ();
		}

		public DataSet Copy()
		{
			throw new NotImplementedException ();
		}

		public DataSet GetChanges()
		{
			throw new NotImplementedException ();
		}

		
		public DataSet GetChanges(DataRowState rowStates)
		{
			throw new NotImplementedException ();
		}

		public string GetXml()
		{
			throw new NotImplementedException ();
		}

		public string GetXmlSchema()
		{
			throw new NotImplementedException ();
		}

		public virtual void RejectChanges()
		{
			throw new NotImplementedException ();
		}

		public virtual void Reset()
		{
			throw new NotImplementedException ();
		}

		public void WriteXml(Stream stream)
		{
			throw new NotImplementedException ();
		}

		public void WriteXml(string fileName)
		{
			throw new NotImplementedException ();
		}

		public void WriteXml(TextWriter writer)
		{
			throw new NotImplementedException ();
		}

		public void WriteXml(XmlWriter writer)
		{
			throw new NotImplementedException ();
		}

		public void WriteXml(Stream stream, XmlWriteMode mode)
		{
			throw new NotImplementedException ();
		}

		public void WriteXml(string fileName, XmlWriteMode mode)
		{
			throw new NotImplementedException ();
		}

		public void WriteXml(TextWriter writer,	XmlWriteMode mode)
		{
			throw new NotImplementedException ();
		}

		public void WriteXml(XmlWriter writer, XmlWriteMode mode)
		{
			throw new NotImplementedException ();
		}

		public void WriteXmlSchema(Stream stream)
		{
			throw new NotImplementedException ();
		}

		public void WriteXmlSchema(string fileName) {
		}

		public void WriteXmlSchema(TextWriter writer) {
		}

		public void WriteXmlSchema(XmlWriter writer) {
		}

		#endregion // Public Methods

		#region Public Events

		public event MergeFailedEventHandler MergeFailed;

		#endregion // Public Events

		#region Destructors

		~DataSet() {
		}

		#endregion Destructors

		#region IListSource methods
		IList IListSource.GetList ()
		{
			throw new NotImplementedException ();
		}

		bool IListSource.ContainsListCollection {
			get {
				throw new NotImplementedException ();
			}
		}
		#endregion IListSource methods
		
		#region ISupportInitialize methods
		void ISupportInitialize.BeginInit ()
		{
			throw new NotImplementedException ();
		}

		void ISupportInitialize.EndInit ()
		{
			throw new NotImplementedException ();
		}
		#endregion

		#region ISerializable
		void ISerializable.GetObjectData (SerializationInfo si, StreamingContext sc)
		{
			throw new NotImplementedException ();
		}
		#endregion
	}
}
