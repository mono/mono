#if NET_2_0
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;

using QName = System.Xml.XmlQualifiedName;

namespace System.Runtime.Serialization
{
	public class XsdDataContractExporter
	{
		ExportOptions options;
		XmlSchemaSet schemas;

		public XsdDataContractExporter ()
		{
		}

		public XsdDataContractExporter (XmlSchemaSet schemas)
		{
			this.schemas = schemas;
		}

		public XmlSchemaSet Schemas {
			get { return schemas; }
		}

		public ExportOptions Options {
			get { return options; }
			set { options = value; }
		}

		public bool CanExport (IList<Type> types)
		{
			foreach (Type t in types)
				if (!CanExport (t))
					return false;
			return true;
		}

		public bool CanExport (IList<Assembly> assemblies)
		{
			foreach (Assembly a in assemblies)
				foreach (Module m in a.GetModules ())
					foreach (Type t in m.GetTypes ())
						if (!CanExport (t))
							return false;
			return true;
		}

		public bool CanExport (Type type)
		{
			throw new NotImplementedException ();
		}

		public void Export (IList<Type> types)
		{
			foreach (Type t in types)
				Export (t);
		}

		public void Export (IList<Assembly> assemblies)
		{
			foreach (Assembly a in assemblies)
				foreach (Module m in a.GetModules ())
					foreach (Type t in m.GetTypes ())
						Export (t);
		}

		public void Export (Type type)
		{
			throw new NotImplementedException ();
		}

		public static XmlQualifiedName GetRootElementName (Type type)
		{
			throw new NotImplementedException ();
		}

		public static XmlQualifiedName GetSchemaTypeName (Type type)
		{
			throw new NotImplementedException ();
		}
	}
}
#endif
