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
using System.ComponentModel;
using System.Management.Instrumentation;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Management
{
	[Serializable]
	[ToolboxItem(false)]
	public class ManagementBaseObject : Component, ICloneable, ISerializable
	{
		private static WbemContext lockOnFastProx;

		internal IWbemClassObjectFreeThreaded _wbemObject;

		private PropertyDataCollection properties;

		private PropertyDataCollection systemProperties;

		private QualifierDataCollection qualifiers;

		internal string ClassName
		{
			get
			{
				object obj = null;
				int num = 0;
				int num1 = 0;
				int num2 = this.wbemObject.Get_("__CLASS", 0, ref obj, ref num, ref num1);
				if (num2 < 0)
				{
					if (((long)num2 & (long)-4096) != (long)-2147217408)
					{
						Marshal.ThrowExceptionForHR(num2);
					}
					else
					{
						ManagementException.ThrowWithExtendedInfo((ManagementStatus)num2);
					}
				}
				if (obj as DBNull == null)
				{
					return (string)obj;
				}
				else
				{
					return string.Empty;
				}
			}
		}

		public virtual ManagementPath ClassPath
		{
			get
			{
				object obj;
				object obj1;
				object obj2;
				object obj3 = null;
				object obj4 = null;
				object obj5 = null;
				int num = 0;
				int num1 = 0;
				int num2 = this.wbemObject.Get_("__SERVER", 0, ref obj3, ref num, ref num1);
				if (num2 == 0)
				{
					num2 = this.wbemObject.Get_("__NAMESPACE", 0, ref obj4, ref num, ref num1);
					if (num2 == 0)
					{
						num2 = this.wbemObject.Get_("__CLASS", 0, ref obj5, ref num, ref num1);
					}
				}
				if (num2 < 0)
				{
					if (((long)num2 & (long)-4096) != (long)-2147217408)
					{
						Marshal.ThrowExceptionForHR(num2);
					}
					else
					{
						ManagementException.ThrowWithExtendedInfo((ManagementStatus)num2);
					}
				}
				ManagementPath managementPath = new ManagementPath();
				managementPath.Server = string.Empty;
				managementPath.NamespacePath = string.Empty;
				managementPath.ClassName = string.Empty;
				try
				{
					ManagementPath managementPath1 = managementPath;
					if (obj3 as DBNull != null)
					{
						obj = "";
					}
					else
					{
						obj = obj3;
					}
					managementPath1.Server = (string)obj;
					ManagementPath managementPath2 = managementPath;
					if (obj4 as DBNull != null)
					{
						obj1 = "";
					}
					else
					{
						obj1 = obj4;
					}
					managementPath2.NamespacePath = (string)obj1;
					ManagementPath managementPath3 = managementPath;
					if (obj5 as DBNull != null)
					{
						obj2 = "";
					}
					else
					{
						obj2 = obj5;
					}
					managementPath3.ClassName = (string)obj2;
				}
				catch
				{
				}
				return managementPath;
			}
		}

		internal bool IsClass
		{
			get
			{
				return ManagementBaseObject._IsClass(this.wbemObject);
			}
		}

		public object this[string propertyName]
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.GetPropertyValue(propertyName);
			}
			set
			{
				this.Initialize(true);
				try
				{
					this.SetPropertyValue(propertyName, value);
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					ManagementException.ThrowWithExtendedInfo(cOMException);
				}
			}
		}

		public virtual PropertyDataCollection Properties
		{
			get
			{
				if (this._wbemObject == null) {
					this.Initialize(true);
				}
				if (this.properties == null)
				{
					this.properties = new PropertyDataCollection(this, false);
				}
				return this.properties;
			}
		}

		public virtual QualifierDataCollection Qualifiers
		{
			get
			{
				this.Initialize(true);
				if (this.qualifiers == null)
				{
					this.qualifiers = new QualifierDataCollection(this);
				}
				return this.qualifiers;
			}
		}

		public virtual PropertyDataCollection SystemProperties
		{
			get
			{
				if (this._wbemObject == null) {
					this.Initialize(false);
				}
				if (this.systemProperties == null)
				{
					this.systemProperties = new PropertyDataCollection(this, true);
				}
				return this.systemProperties;
			}
		}

		public bool ObjectExits {
			get { return this._wbemObject != null; }
		}

		internal IWbemClassObjectFreeThreaded wbemObject
		{
			get
			{
			
				if (this._wbemObject == null)
				{
					this.Initialize(false);
				}

				return this._wbemObject;
			}
			set
			{
				this._wbemObject = value;
			}
		}

		static ManagementBaseObject()
		{
			WbemContext wbemContext;
			if (WMICapabilities.IsWindowsXPOrHigher())
			{
				wbemContext = null;
			}
			else
			{
				wbemContext = new WbemContext();
			}
			ManagementBaseObject.lockOnFastProx = wbemContext;
		}

		protected ManagementBaseObject(SerializationInfo info, StreamingContext context)
		{
			this._wbemObject = info.GetValue("wbemObject", typeof(IWbemClassObjectFreeThreaded)) as IWbemClassObjectFreeThreaded;
			if (this._wbemObject != null)
			{
				this.properties = null;
				this.systemProperties = null;
				this.qualifiers = null;
				return;
			}
			else
			{
				throw new SerializationException();
			}
		}

		internal ManagementBaseObject(IWbemClassObjectFreeThreaded wbemObject)
		{
			this.wbemObject = wbemObject;
			this.properties = null;
			this.systemProperties = null;
			this.qualifiers = null;
		}

		private static bool _IsClass(IWbemClassObjectFreeThreaded wbemObject)
		{
			object obj = null;
			int num = 0;
			int num1 = 0;
			int num2 = wbemObject.Get_("__GENUS", 0, ref obj, ref num, ref num1);
			if (num2 < 0)
			{
				if (((long)num2 & (long)-4096) != (long)-2147217408)
				{
					Marshal.ThrowExceptionForHR(num2);
				}
				else
				{
					ManagementException.ThrowWithExtendedInfo((ManagementStatus)num2);
				}
			}
			if (obj is int) {
				return (int)obj == 1;
			}
			return false;
		}

		public virtual object Clone()
		{
			IWbemClassObjectFreeThreaded wbemClassObjectFreeThreaded = null;
			int num = this.wbemObject.Clone_(out wbemClassObjectFreeThreaded);
			if (num < 0)
			{
				if (((long)num & (long)-4096) != (long)-2147217408)
				{
					Marshal.ThrowExceptionForHR(num);
				}
				else
				{
					ManagementException.ThrowWithExtendedInfo((ManagementStatus)num);
				}
			}
			return new ManagementBaseObject(wbemClassObjectFreeThreaded);
		}

		public bool CompareTo(ManagementBaseObject otherObject, ComparisonSettings settings)
		{
			if (otherObject != null)
			{
				bool flag = false;
				if (this.wbemObject != null)
				{
					int num = this.wbemObject.CompareTo_((int)settings, otherObject.wbemObject);
					if (0x40003 != num)
					{
						if (num != 0)
						{
							if (((long)num & (long)-4096) != (long)-2147217408)
							{
								if (num < 0)
								{
									Marshal.ThrowExceptionForHR(num);
								}
							}
							else
							{
								ManagementException.ThrowWithExtendedInfo((ManagementStatus)num);
							}
						}
						else
						{
							flag = true;
						}
					}
					else
					{
						flag = false;
					}
				}
				return flag;
			}
			else
			{
				throw new ArgumentNullException("otherObject");
			}
		}

		public new void Dispose()
		{
			if (this._wbemObject != null)
			{
				this._wbemObject.Dispose();
				this._wbemObject = null;
			}
			base.Dispose();
			GC.SuppressFinalize(this);
		}

		public override bool Equals(object obj)
		{
			bool flag;
			try
			{
				if (obj as ManagementBaseObject == null)
				{
					flag = false;
				}
				else
				{
					bool flag1 = this.CompareTo((ManagementBaseObject)obj, ComparisonSettings.IncludeAll);
					return flag1;
				}
			}
			catch (ManagementException managementException1)
			{
				ManagementException managementException = managementException1;
				if (managementException.ErrorCode != ManagementStatus.NotFound || this as ManagementObject == null || obj as ManagementObject == null)
				{
					flag = false;
				}
				else
				{
					int num = string.Compare(((ManagementObject)this).Path.Path, ((ManagementObject)obj).Path.Path, StringComparison.OrdinalIgnoreCase);
					flag = num == 0;
				}
			}
			catch
			{
				flag = false;
			}
			return flag;
		}

		internal static ManagementBaseObject GetBaseObject(IWbemClassObjectFreeThreaded wbemObject, ManagementScope scope)
		{
			ManagementBaseObject managementObject;
			if (!ManagementBaseObject._IsClass(wbemObject))
			{
				managementObject = ManagementObject.GetManagementObject(wbemObject, scope);
			}
			else
			{
				managementObject = ManagementClass.GetManagementClass(wbemObject, scope);
			}
			return managementObject;
		}

		public override int GetHashCode()
		{
			int hashCode;
			try
			{
				hashCode = this.GetText(TextFormat.Mof).GetHashCode();
			}
			catch (ManagementException managementException)
			{
				hashCode = string.Empty.GetHashCode();
			}
			catch (COMException cOMException)
			{
				hashCode = string.Empty.GetHashCode();
			}
			return hashCode;
		}

		protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			this.GetObjectData(info, context);
		}

		public object GetPropertyQualifierValue(string propertyName, string qualifierName)
		{
			return this.Properties[propertyName].Qualifiers[qualifierName].Value;
		}

		public object GetPropertyValue(string propertyName)
		{
			if (propertyName != null)
			{
				if (!propertyName.StartsWith("__", StringComparison.Ordinal))
				{
					return this.Properties[propertyName].Value;
				}
				else
				{
					return this.SystemProperties[propertyName].Value;
				}
			}
			else
			{
				throw new ArgumentNullException("propertyName");
			}
		}

		public object GetQualifierValue(string qualifierName)
		{
			return this.Qualifiers[qualifierName].Value;
		}

		public string GetText(TextFormat format)
		{
			int objectText_;
			string str = null;
			TextFormat textFormat = format;
			switch (textFormat)
			{
				case TextFormat.Mof:
				{
					objectText_ = this.wbemObject.GetObjectText_(0, out str);
					if (objectText_ < 0)
					{
						if (((long)objectText_ & (long)-4096) != (long)-2147217408)
						{
							Marshal.ThrowExceptionForHR(objectText_);
						}
						else
						{
							ManagementException.ThrowWithExtendedInfo((ManagementStatus)objectText_);
						}
					}
					return str;
				}
				case TextFormat.CimDtd20:
				case TextFormat.WmiDtd20:
				{
					IWbemObjectTextSrc wbemObjectTextSrc = (IWbemObjectTextSrc)(new WbemObjectTextSrc());
					IWbemContext wbemContext = (IWbemContext)(new WbemContext());
					object obj = true;
					wbemContext.SetValue_("IncludeQualifiers", 0, ref obj);
					wbemContext.SetValue_("IncludeClassOrigin", 0, ref obj);
					if (wbemObjectTextSrc != null)
					{
						objectText_ = wbemObjectTextSrc.GetText_(0, (IWbemClassObject_DoNotMarshal)Marshal.GetObjectForIUnknown(this.wbemObject), (uint)format, wbemContext, out str);
						if (objectText_ < 0)
						{
							if (((long)objectText_ & (long)-4096) != (long)-2147217408)
							{
								Marshal.ThrowExceptionForHR(objectText_);
							}
							else
							{
								ManagementException.ThrowWithExtendedInfo((ManagementStatus)objectText_);
							}
						}
					}
					return str;
				}
			}
			return null;
		}

		internal virtual void Initialize(bool getObject)
		{

		}

		public static explicit operator IntPtr(ManagementBaseObject managementObject)
		{
			if (managementObject != null)
			{
				return managementObject.wbemObject;
			}
			else
			{
				return IntPtr.Zero;
			}
		}

		public void SetPropertyQualifierValue(string propertyName, string qualifierName, object qualifierValue)
		{
			this.Properties[propertyName].Qualifiers[qualifierName].Value = qualifierValue;
		}

		public void SetPropertyValue(string propertyName, object propertyValue)
		{
			if (propertyName != null)
			{
				if (!propertyName.StartsWith("__", StringComparison.Ordinal))
				{
					this.Properties[propertyName].Value = propertyValue;
					return;
				}
				else
				{
					this.SystemProperties[propertyName].Value = propertyValue;
					return;
				}
			}
			else
			{
				throw new ArgumentNullException("propertyName");
			}
		}

		public void SetQualifierValue(string qualifierName, object qualifierValue)
		{
			this.Qualifiers[qualifierName].Value = qualifierValue;
		}

		[SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter=true)]
		void System.Runtime.Serialization.ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("wbemObject", this.wbemObject, typeof(IWbemClassObjectFreeThreaded));
			info.AssemblyName = typeof(ManagementBaseObject).Assembly.FullName;
			info.FullTypeName = typeof(ManagementBaseObject).ToString();
		}
	}
}