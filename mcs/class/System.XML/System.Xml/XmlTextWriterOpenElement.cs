using System;

namespace System.Xml
{
	internal class XmlTextWriterOpenElement
	{
		#region Fields

		string name;
		bool indentingOverriden = false;

		#endregion

		#region Constructors

		public XmlTextWriterOpenElement (string name)
		{
			this.name = name;
		}

		#endregion

		#region Properties

		public string Name 
		{
			get { return name; }
		}

		public bool IndentingOverriden 
		{
			get { return indentingOverriden; }
			set { indentingOverriden = value; }
		}

		#endregion

		#region Methods

		public override string ToString ()
		{
			return name;
		}

		#endregion
	}
}
