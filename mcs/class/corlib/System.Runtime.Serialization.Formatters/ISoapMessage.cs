//
// System.Runtime.Serialization.Formatters.ISoapMessage
//
// Author:
//   David Dawkins (david@dawkins.st)
//
// (C) David Dawkins
//

using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Serialization.Formatters {

	/// <summary>
	/// Interface for making SOAP method calls</summary>
	public interface ISoapMessage {

		/// <summary>
		/// Get or set the headers ("out-of-band" data) for the method call</summary>
		Header[] Headers {
			get;
			set;
		}

		/// <summary>
		/// Get or set the method name</summary>
		string MethodName {
			get;
			set;
		}

		/// <summary>
		/// Get or set the method parameter names</summary
		string[] ParamNames {
			get;
			set;
		}

		/// <summary>
		/// Get or set the method parameter types</summary
		Type[] ParamTypes {
			get;
			set;
		}

		/// <summary>
		/// Get or set the method parameter values</summary
		object[]  ParamValues {
			get;
			set;
		}

		/// <summary>
		/// Get or set the XML namespace for the location of the called object</summary
		string XmlNameSpace {
			get;
			set;
		}
	}
}
