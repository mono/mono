// 
// System.Web.Services.Description.SoapProtocolImporter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.CodeDom;
using System.Web.Services;
using System.Xml.Serialization;

namespace System.Web.Services.Description {
	public sealed class SoapProtocolImporter : ProtocolImporter {

		#region Fields

		SoapBinding soapBinding;
		SoapCodeExporter soapExporter;
		SoapSchemaImporter soapImporter;
		XmlCodeExporter xmlExporter;
		XmlSchemaImporter xmlImporter;
		
		#endregion // Fields

		#region Constructors

		[MonoTODO]	
		public SoapProtocolImporter ()
		{
			throw new NotImplementedException ();
		}
		
		#endregion // Constructors

		#region Properties

		public override string ProtocolName {
			get { return "Soap"; }
		}

		public SoapBinding SoapBinding {
			get { return soapBinding; }
		}

		public SoapCodeExporter SoapExporter {
			get { return soapExporter; }
		}

		public SoapSchemaImporter SoapImporter {
			get { return soapImporter; }
		}

		public XmlCodeExporter XmlExporter {
			get { return xmlExporter; }
		}

		public XmlSchemaImporter XmlImporter {
			get { return xmlImporter; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		protected override CodeTypeDeclaration BeginClass ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void BeginNamespace ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void EndClass ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void EndNamespace ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override CodeMemberMethod GenerateMethod ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override bool IsBindingSupported ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override bool IsOperationFlowSupported (OperationFlow flow)
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}
