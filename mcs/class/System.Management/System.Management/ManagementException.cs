//
// System.Management.AuthenticationLevel
//
// Author:
//	Bruno Lauze     (brunolauze@msn.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2015 Microsoft (http://www.microsoft.com)
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
using System;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Management
{
	[Serializable]
	public class ManagementException : SystemException
	{
		private ManagementBaseObject errorObject;

		private ManagementStatus errorCode;

		public ManagementStatus ErrorCode
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.errorCode;
			}
		}

		public ManagementBaseObject ErrorInformation
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.errorObject;
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		internal ManagementException(ManagementStatus errorCode, string msg, ManagementBaseObject errObj) : base(msg)
		{
			this.errorCode = errorCode;
			this.errorObject = errObj;
		}

		protected ManagementException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			this.errorCode = (ManagementStatus)info.GetValue("errorCode", typeof(ManagementStatus));
			this.errorObject = info.GetValue("errorObject", typeof(ManagementBaseObject)) as ManagementBaseObject;
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ManagementException() : this((ManagementStatus)(-2147217407), "", null)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ManagementException(string message) : this((ManagementStatus)(-2147217407), message, null)
		{
		}

		public ManagementException(string message, Exception innerException) : this(innerException, message, null)
		{
			if (innerException as ManagementException == null)
			{
				this.errorCode = ManagementStatus.Failed;
			}
		}

		internal ManagementException(Exception e, string msg, ManagementBaseObject errObj) : base(msg, e)
		{
			try
			{
				if (e as ManagementException == null)
				{
					if (e as COMException == null)
					{
						this.errorCode = (ManagementStatus)base.HResult;
					}
					else
					{
						this.errorCode = (ManagementStatus)((COMException)e).ErrorCode;
					}
				}
				else
				{
					this.errorCode = ((ManagementException)e).ErrorCode;
					if (this.errorObject == null)
					{
						this.errorObject = null;
					}
					else
					{
						this.errorObject = (ManagementBaseObject)((ManagementException)e).errorObject.Clone();
					}
				}
			}
			catch
			{
			}
		}

		private static string GetMessage(Exception e)
		{
			string message = null;
			if (e as COMException != null)
			{
				message = ManagementException.GetMessage((ManagementStatus)((COMException)e).ErrorCode);
			}
			if (message == null)
			{
				message = e.Message;
			}
			return message;
		}

		private static string GetMessage(ManagementStatus errorCode)
		{
			string str = null;
			IWbemStatusCodeText wbemStatusCodeText = (IWbemStatusCodeText)(new WbemStatusCodeText());
			if (wbemStatusCodeText != null)
			{
				try
				{
					int errorCodeText_ = wbemStatusCodeText.GetErrorCodeText_((int)errorCode, 0, 1, out str);
					if (errorCodeText_ != 0)
					{
						errorCodeText_ = wbemStatusCodeText.GetErrorCodeText_((int)errorCode, 0, 0, out str);
					}
				}
				catch
				{
				}
			}
			return str;
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("errorCode", this.errorCode);
			info.AddValue("errorObject", this.errorObject);
		}

		internal static void ThrowWithExtendedInfo(ManagementStatus errorCode)
		{
			ManagementBaseObject managementBaseObject = null;
			IWbemClassObjectFreeThreaded errorInfo = WbemErrorInfo.GetErrorInfo();
			if (errorInfo != null)
			{
				managementBaseObject = new ManagementBaseObject(errorInfo);
			}
			string message = ManagementException.GetMessage(errorCode);
			string item = message;
			if (message == null && managementBaseObject != null)
			{
				try
				{
					item = (string)managementBaseObject["Description"];
				}
				catch
				{
				}
			}
			throw new ManagementException(errorCode, item, managementBaseObject);
		}

		internal static void ThrowWithExtendedInfo(Exception e)
		{
			ManagementBaseObject managementBaseObject = null;
			IWbemClassObjectFreeThreaded errorInfo = WbemErrorInfo.GetErrorInfo();
			if (errorInfo != null)
			{
				managementBaseObject = new ManagementBaseObject(errorInfo);
			}
			string message = ManagementException.GetMessage(e);
			string item = message;
			if (message == null && managementBaseObject != null)
			{
				try
				{
					item = (string)managementBaseObject["Description"];
				}
				catch
				{
				}
			}
			throw new ManagementException(e, item, managementBaseObject);
		}
	}
}