// 
// System.Web.Services.Protocols.SoapHeader.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.ComponentModel;
using System.Xml.Serialization;

namespace System.Web.Services.Protocols {
	[SoapType (IncludeInSchema = false)]
	[XmlType (IncludeInSchema = false)]
	public abstract class SoapHeader {

		#region Fields

		string actor;
		bool didUnderstand;
		bool mustUnderstand;

		#endregion // Fields

		#region Constructors

		protected SoapHeader ()
		{
			actor = String.Empty; 
			didUnderstand = false;
			mustUnderstand = false;
		}

		#endregion // Constructors

		#region Properties

		[DefaultValue ("")]
		[SoapAttribute ("actor", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
		[XmlAttribute ("actor", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
		public string Actor {	
			get { return actor; }
			set { actor = value; }
		}

		[SoapIgnore]
		[XmlIgnore]
		public bool DidUnderstand {
			get { return didUnderstand; }
			set { didUnderstand = value; }
		}

		[DefaultValue ("0")]
		[SoapAttribute ("mustUnderstand", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
		[XmlAttribute ("mustUnderstand", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
		public string EncodedMustUnderstand {
			get { return (MustUnderstand ? "1" : "0"); }
			set {	
				if (value == "true" || value == "1") 
					MustUnderstand = true;
				else if (value == "false" || value == "0")
					MustUnderstand = false;
				else
					throw new ArgumentException ();
			}
		}

		[SoapIgnore]
		[XmlIgnore]
		public bool MustUnderstand {
			get { return mustUnderstand; }
			set { mustUnderstand = value; }
		}

		#endregion // Properties
	}
}
