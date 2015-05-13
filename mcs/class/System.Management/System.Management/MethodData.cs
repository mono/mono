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
using System.Runtime.InteropServices;

namespace System.Management
{
	public class MethodData
	{
		private ManagementObject parent;

		private string methodName;

		private IWbemClassObjectFreeThreaded wmiInParams;

		private IWbemClassObjectFreeThreaded wmiOutParams;

		private QualifierDataCollection qualifiers;

		public ManagementBaseObject InParameters
		{
			get
			{
				this.RefreshMethodInfo();
				if (this.wmiInParams == null)
				{
					return null;
				}
				else
				{
					return new ManagementBaseObject(this.wmiInParams);
				}
			}
		}

		public string Name
		{
			get
			{
				if (this.methodName != null)
				{
					return this.methodName;
				}
				else
				{
					return "";
				}
			}
		}

		public string Origin
		{
			get
			{
				string empty = null;
				int methodOrigin_ = this.parent.wbemObject.GetMethodOrigin_(this.methodName, out empty);
				if (methodOrigin_ < 0)
				{
					if (methodOrigin_ != -2147217393)
					{
						if (((long)methodOrigin_ & (long)-4096) != (long)-2147217408)
						{
							Marshal.ThrowExceptionForHR(methodOrigin_);
						}
						else
						{
							ManagementException.ThrowWithExtendedInfo((ManagementStatus)methodOrigin_);
						}
					}
					else
					{
						empty = string.Empty;
					}
				}
				return empty;
			}
		}

		public ManagementBaseObject OutParameters
		{
			get
			{
				this.RefreshMethodInfo();
				if (this.wmiOutParams == null)
				{
					return null;
				}
				else
				{
					return new ManagementBaseObject(this.wmiOutParams);
				}
			}
		}

		public QualifierDataCollection Qualifiers
		{
			get
			{
				if (this.qualifiers == null)
				{
					this.qualifiers = new QualifierDataCollection(this.parent, this.methodName, QualifierType.MethodQualifier);
				}
				return this.qualifiers;
			}
		}

		internal MethodData(ManagementObject parent, string methodName)
		{
			this.parent = parent;
			this.methodName = methodName;
			this.RefreshMethodInfo();
			this.qualifiers = null;
		}

		private void RefreshMethodInfo()
		{
			int method_ = -2147217407;
			try
			{
				method_ = this.parent.wbemObject.GetMethod_(this.methodName, 0, out this.wmiInParams, out this.wmiOutParams);
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				ManagementException.ThrowWithExtendedInfo(cOMException);
			}
			if (((long)method_ & (long)-4096) != (long)-2147217408)
			{
				if (((long)method_ & (long)-2147483648) != (long)0)
				{
					Marshal.ThrowExceptionForHR(method_);
				}
				return;
			}
			else
			{
				ManagementException.ThrowWithExtendedInfo((ManagementStatus)method_);
				return;
			}
		}
	}
}