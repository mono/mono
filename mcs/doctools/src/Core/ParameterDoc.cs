using System;
using System.Xml.Serialization;

namespace Mono.Doc.Core
{
	[XmlType(TypeName = "param")]
	public class ParameterDoc
	{
		private string name        = string.Empty;
		private string description = string.Empty;

		public ParameterDoc()
		{
		}

		public ParameterDoc(string name, string description)
		{
			if (name == null)
			{
				throw new ArgumentException("ParameterDoc name cannot be null.", "name");
			}

			if (description == null)
			{
				throw new ArgumentException("ParameterDoc description cannot be null.", "description");
			}

			this.name        = name;
			this.description = description;
		}

		[XmlAttribute(AttributeName = "name")]
		public string Name
		{
			get { return this.name;  }
			set { this.name = value; }
		}

		[XmlText]
		public string Description
		{
			get { return this.description;  }
			set { this.description = value; }
		}
	}
}
