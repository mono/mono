// 
// System.EnterpriseServices.RegistrationErrorInfo.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace System.EnterpriseServices {
	[Serializable]
	public sealed class RegistrationErrorInfo {

		#region Fields

		int errorCode;
		string errorString;
		string majorRef;
		string minorRef;
		string name;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public RegistrationErrorInfo (string name, string majorRef, string minorRef, int errorCode) 
		{
			this.name = name;
			this.majorRef = majorRef;
			this.minorRef = minorRef;
			this.errorCode = errorCode;
		}

		#endregion // Constructors

		#region Properties

		public int ErrorCode {
			get { return errorCode; }
		}

		public string ErrorString {
			get { return errorString; }
		}

		public string MajorRef {
			get { return majorRef; }
		}

		public string MinorRef {
			get { return minorRef; }
		}

		public string Name {
			get { return name; }
		}

		#endregion // Properties
	}
}
