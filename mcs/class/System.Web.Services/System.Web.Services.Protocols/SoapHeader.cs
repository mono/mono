// 
// System.Web.Services.Protocols.SoapHeader.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services;

namespace System.Web.Services.Protocols {
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

		public string Actor {	
			get { return actor; }
			set { actor = value; }
		}

		public bool DidUnderstand {
			get { return didUnderstand; }
			set { didUnderstand = value; }
		}

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

		public bool MustUnderstand {
			get { return mustUnderstand; }
			set { mustUnderstand = value; }
		}

		#endregion // Properties
	}
}
