// 
// System.Web.Services.Protocols.SoapMessage.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using HeaderInfo = System.Web.Services.Protocols.SoapHeaderMapping;

using System.ComponentModel;
using System.IO;
using System.Web.Services;

namespace System.Web.Services.Protocols {
	public abstract class SoapMessage {

		#region Fields

		string content_type = "text/xml";
		string content_encoding;
		SoapException exception = null;
		SoapHeaderCollection headers;
		SoapMessageStage stage;
		Stream stream;
		object[] inParameters;
		object[] outParameters;
		
		SoapProtocolVersion soapVersion;

		#endregion // Fields

		#region Constructors

		internal SoapMessage ()
		{
			headers = new SoapHeaderCollection ();
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
			set { exception = value; }
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
		
		public string ContentEncoding
		{
			get { return content_encoding; }
			set { content_encoding = value; }
		}

		internal bool IsSoap12 {
			get {
				return SoapVersion == SoapProtocolVersion.Soap12;
			}
		}

		[System.Runtime.InteropServices.ComVisible(false)]
		[DefaultValue (SoapProtocolVersion.Default)]
		public virtual SoapProtocolVersion SoapVersion {
			get { return soapVersion; }
		}
 
		internal Stream InternalStream 
		{ 
			// for getter use public stream property
			set {
				stream = value;
			}
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
			if (!MethodInfo.IsVoid && exception == null) return outParameters [0];
			else return null;
		}

		internal void SetHeaders (SoapHeaderCollection headers)
		{
			this.headers = headers;
		}

		internal void SetException (SoapException ex)
		{
			exception = ex;
		}

		internal void CollectHeaders (object target, HeaderInfo[] headers, SoapHeaderDirection direction)
		{
			Headers.Clear ();
			foreach (HeaderInfo hi in headers) 
			{
				if ((hi.Direction & direction) != 0 && !hi.Custom) 
				{
					SoapHeader headerVal = hi.GetHeaderValue (target) as SoapHeader;
					if (headerVal != null)
						Headers.Add (headerVal);
				}
			}
		}

		internal void UpdateHeaderValues (object target, HeaderInfo[] headersInfo)
		{
			foreach (SoapHeader header in Headers)
			{
				HeaderInfo hinfo = FindHeader (headersInfo, header.GetType ());
				if (hinfo != null) {
					hinfo.SetHeaderValue (target, header);
					header.DidUnderstand = !hinfo.Custom;
				}
			}
		}

		HeaderInfo FindHeader (HeaderInfo[] headersInfo, Type headerType)
		{
			HeaderInfo unknownHeaderInfo = null;
		
			foreach (HeaderInfo headerInfo in headersInfo) {
				if (headerInfo.HeaderType == headerType)
					return headerInfo;
				else if (headerInfo.Custom) 
					unknownHeaderInfo = headerInfo;
			}
			return unknownHeaderInfo;
		}

		#endregion // Methods
	}
}
