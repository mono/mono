// 
// System.Web.Services.Protocols.SoapMessage.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
//
// TODO:
//    Need to set the stream variable from the outside, or the constructor.
//

using System.IO;
using System.Web.Services;

namespace System.Web.Services.Protocols {
	public abstract class SoapMessage {

		#region Fields

		string content_type = "text/xml";
		SoapException exception = null;
		SoapHeaderCollection headers;
		SoapMessageStage stage;
		Stream stream;
		object[] inParameters;
		object[] outParameters;
		
		#endregion // Fields

		#region Constructors

		internal SoapMessage ()
		{
			headers = new SoapHeaderCollection ();
		}

		internal SoapMessage (Stream stream, SoapHeaderCollection headers)
		{
			this.headers = headers;
			this.stream = stream;
		}

		internal SoapMessage (Stream stream, SoapException exception)
		{
			this.exception = exception;
			this.stream = stream;
			headers = new SoapHeaderCollection ();
		}

		#endregion

		#region Properties

		internal object[] InParameters 
		{
			get { return inParameters; }
			set { inParameters = value; }
		}

		internal object[] OutParameters 
		{
			get { return outParameters; }
			set { outParameters = value; }
		}

		public abstract string Action 
		{
			get;
		}

		public string ContentType {
			get { return content_type; }
			set { content_type = value; }
		}

		public SoapException Exception {
			get { return exception; }
		}

		public SoapHeaderCollection Headers {
			get { return headers; }
		}

		public abstract LogicalMethodInfo MethodInfo {
			get;
		}

		public abstract bool OneWay {
			get;
		}

		public SoapMessageStage Stage {
			get { return stage; }
		}

		internal void SetStage (SoapMessageStage stage)
		{
			this.stage = stage;
		}
		
		public Stream Stream {
			get {
				return stream;
			}
		}

		public abstract string Url {
			get;
		}

		#endregion Properties

		#region Methods

		protected abstract void EnsureInStage ();
		protected abstract void EnsureOutStage ();

		protected void EnsureStage (SoapMessageStage stage) 
		{
			if ((((int) stage) & ((int) Stage)) == 0)
				throw new InvalidOperationException ("The current SoapMessageStage is not the asserted stage or stages.");
		}

		public object GetInParameterValue (int index) 
		{
			return inParameters [index];
		}

		public object GetOutParameterValue (int index) 
		{
			if (MethodInfo.IsVoid) return outParameters [index];
			else return outParameters [index + 1];
		}

		public object GetReturnValue ()
		{
			if (MethodInfo.IsVoid) return null;
			else return outParameters [0];
		}

		#endregion // Methods
	}
}
