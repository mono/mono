 // 
// System.Web.Services.WebMethodAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

/* FIXME System.EnterpriseServices doesn't exist yet.
using System.EnterpriseServices;
*/

namespace System.Web.Services {
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class WebMethodAttribute : Attribute {
		#region Fields

		bool bufferResponse;
		int cacheDuration;
		string description;
		bool enableSession;
		string messageName;

		/* FIXME: TransactionOption is not defined because it is in System.EnterpriseServices
		TransactionOption transactionOption;
		*/

		#endregion // Fields

		#region Constructors

		public WebMethodAttribute ()
		{
			bufferResponse = true;
			cacheDuration = 0;
			description = String.Empty;
			enableSession = false;

			messageName = String.Empty; // FIXME

			/* FIXME
			transactionOption = TransactionOption.Disabled;
			*/
		}

		public WebMethodAttribute (bool enableSession)
			: this ()
		{
			this.enableSession = enableSession;
		}

		/* FIXME: TransactionOption is not defined, because it is in System.EnterpriseServices

		public WebMethodAttribute (bool enableSession, TransactionOption transactionOption)
		{
			this.enableSession = enableSession;
			this.transactionOption = transactionOption;
		}

		public WebMethodAttribute (bool enableSession, TransactionOption transactionOption, int cacheDuration)
			: this ()
		{
			this.cacheDuration = cacheDuration;
			this.enableSession = enableSession;
			this.transactionOption = transactionOption;
		}

		public WebMethodAttribute (bool enableSession, TransactionOption transactionOption, int cacheDuration, bool bufferResponse)
			: this ()
		{
			this.bufferResponse = bufferResponse;
			this.cacheDuration = cacheDuration;
			this.enableSession = enableSession;
			this.transactionOption = transactionOption;
		}
		*/
		
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

		/* FIXME: TransactionOption is not defined because it is in System.EnterpriseServices

		public TransactionOption TransactionOption {
			get { return transactionOption; }
			set { transactionOption = value; }
		}
		*/

		#endregion // Properties
	}
}
