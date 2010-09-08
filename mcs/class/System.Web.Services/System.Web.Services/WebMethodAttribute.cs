 // 
// System.Web.Services.WebMethodAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
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

#if !TARGET_J2EE && !MOBILE
using System.EnterpriseServices;
#endif

namespace System.Web.Services {

#if TARGET_J2EE || MOBILE
	public enum TransactionOption {Disabled , NotSupported , Required , RequiresNew , Supported }
#endif

	[AttributeUsage(AttributeTargets.Method, Inherited = true)]
	public sealed class WebMethodAttribute : Attribute {

		#region Fields

		bool bufferResponse;
		int cacheDuration;
		string description;
		bool enableSession;
		string messageName;
		TransactionOption transactionOption;

		#endregion // Fields

		#region Constructors

		public WebMethodAttribute ()
			: this (false, TransactionOption.Disabled, 0, true)
		{
		}

		public WebMethodAttribute (bool enableSession)
			: this (enableSession, TransactionOption.Disabled, 0, true)
		{
		}

		public WebMethodAttribute (bool enableSession, TransactionOption transactionOption)
			: this (enableSession, transactionOption, 0, true)
		{
		}

		public WebMethodAttribute (bool enableSession, TransactionOption transactionOption, int cacheDuration)
			: this (enableSession, transactionOption, cacheDuration, true)
		{
		}

		public WebMethodAttribute (bool enableSession, TransactionOption transactionOption, int cacheDuration, bool bufferResponse)
		{
			this.bufferResponse = bufferResponse;
			this.cacheDuration = cacheDuration;
			this.enableSession = enableSession;
			this.transactionOption = transactionOption;

			this.description = String.Empty;
			this.messageName = String.Empty;
		}
		
		#endregion // Constructors

		#region Properties

		public bool BufferResponse {
			get { return bufferResponse; }
			set { bufferResponse = value; }
		}

		public int CacheDuration {
			get { return cacheDuration; }
			set { cacheDuration = value; }
		}

		public string Description { 
			get { return description; }
			set { description = value; }
		}

		public bool EnableSession {
			get { return enableSession; }
			set { enableSession = value; }
		}

		public string MessageName {
			get { return messageName; }
			set { messageName = value; }
		}

		public TransactionOption TransactionOption {
			get { return transactionOption; }
			set { transactionOption = value; }
		}

		#endregion // Properties
	}
}
