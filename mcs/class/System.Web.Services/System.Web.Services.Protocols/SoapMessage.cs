// 
// System.Web.Services.Protocols.SoapMessage.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
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
		SoapHeaderCollection headers = null;
		SoapMessageStage stage;
		Stream stream;
		
		#endregion // Fields

		#region Constructors

		internal SoapMessage ()
		{
		}

		#endregion

		#region Properties

		public abstract string Action {
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

		[MonoTODO]
		public object GetInParameterValue (int index) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object GetOutParameterValue (int index) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object GetReturnValue ()
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
