//
// JScriptException.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript.Tmp
{
	using System;
	using Microsoft.Vsa;
	using System.Runtime.Serialization;

	[Serializable]
	public class JScriptException : ApplicationException, IVsaError
	{
		public JScriptException (JSError errorNumber)
		{
			throw new NotImplementedException ();
		}


		public string SourceMoniker {
			get { throw new NotImplementedException (); }
		}


		public int StartColumn {
			get { throw new NotImplementedException (); }
		}

	
		public int Column {
			get { throw new NotImplementedException (); }
		}


		public string Description {
			get { throw new NotImplementedException (); }
		}

		
		public int EndLine {
			get { throw new NotImplementedException (); }
		}


		public int EndColumn {
			get { throw new NotImplementedException (); }
		}


		public int Number {
			get { throw new NotImplementedException (); }
		}


		public int ErrorNumber {
			get { throw new NotImplementedException (); }
		}


		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}


		public int Line {
			get { throw new NotImplementedException (); }
		}


		public string LineText {
			get { throw new NotImplementedException (); }
		}


		public override string Message {
			get { throw new NotImplementedException (); }
		}


		public int Severity {
			get { throw new NotImplementedException (); }
		}


		public IVsaItem SourceItem {
			get { throw new NotImplementedException (); }
		}


		public override string StackTrace {
			get { throw new NotImplementedException (); }
		}
	}

	
	public class NoContextException : ApplicationException 
	{}
}
		