 // 
// System.Web.Services.WebMethodAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.EnterpriseServices;

namespace System.Web.Services {
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
