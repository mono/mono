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

namespace System.Management
{
	[Serializable]
	public class ManagementObject : ManagementBaseObject, ICloneable
	{
		private IWbemClassObjectFreeThreaded wmiClass;

		internal ManagementScope scope;

		internal ManagementPath path;

		internal ObjectGetOptions options;

		private bool putButNotGot;

		internal const string ID = "ID";

		internal const string RETURNVALUE = "RETURNVALUE";

		public override ManagementPath ClassPath
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
				if (this.PutButNotGot)
				{
					this.Get();
					this.PutButNotGot = false;
				}
				int num2 = base.wbemObject.Get_("__SERVER", 0, ref obj3, ref num, ref num1);
				if (num2 >= 0)
				{
					num2 = base.wbemObject.Get_("__NAMESPACE", 0, ref obj4, ref num, ref num1);
					if (num2 >= 0)
					{
						num2 = base.wbemObject.Get_("__CLASS", 0, ref obj5, ref num, ref num1);
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
					if (obj3 as DBNull != null)
					{
						obj = "";
					}
					else
					{
						obj = obj3;
					}
					if (obj4 as DBNull != null)
					{
						obj1 = "";
					}
					else
					{
						obj1 = obj4;
					}
					if (obj5 as DBNull != null)
					{
						obj2 = "";
					}
					else
					{
						obj2 = obj5;
					}
					ManagementPath managementPath1 = managementPath;
					managementPath1.Server = (string)obj;
					ManagementPath managementPath2 = managementPath;

					managementPath2.NamespacePath = (string)obj1;
					ManagementPath managementPath3 = managementPath;

					managementPath3.RelativePath = string.Format ("{0}/{1}", (string)obj1, (string)obj2);

					managementPath3.ClassName = (string)obj2;
				}
				catch
				{
				}
				return managementPath;
			}
		}

		internal bool IsBound
		{
			get
			{
				return this._wbemObject != null;
			}
		}

		public ObjectGetOptions Options
		{
			get
			{
				if (this.options != null)
				{
					return this.options;
				}
				else
				{
					ObjectGetOptions objectGetOption = ObjectGetOptions._Clone(null);
					ObjectGetOptions objectGetOption1 = objectGetOption;
					this.options = objectGetOption;
					return objectGetOption1;
				}
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				else
				{
					if (this.options != null)
					{
						this.options.IdentifierChanged -= new IdentifierChangedEventHandler(this.HandleIdentifierChange);
					}
					this.options = ObjectGetOptions._Clone(value, new IdentifierChangedEventHandler(this.HandleIdentifierChange));
					this.FireIdentifierChanged();
					return;
				}
			}
		}

		public virtual ManagementPath Path
		{
			get
			{
				if (this.path != null)
				{
					return this.path;
				}
				else
				{
					ManagementPath managementPath = ManagementPath._Clone(null);
					ManagementPath managementPath1 = managementPath;
					this.path = managementPath;
					return managementPath1;
				}
			}
			set
			{
				ManagementPath managementPath;
				if (value != null)
				{
					managementPath = value;
				}
				else
				{
					managementPath = new ManagementPath();
				}
				ManagementPath managementPath1 = managementPath;
				string namespacePath = managementPath1.GetNamespacePath(8);
				if (namespacePath.Length > 0 && this.scope != null && this.scope.IsDefaulted)
				{
					this.Scope = new ManagementScope(namespacePath);
				}
				if ((!(base.GetType() == typeof(ManagementObject)) || !managementPath1.IsInstance) && (!(base.GetType() == typeof(ManagementClass)) || !managementPath1.IsClass) && !managementPath1.IsEmpty)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				else
				{
					if (this.path != null)
					{
						this.path.IdentifierChanged -= new IdentifierChangedEventHandler(this.HandleIdentifierChange);
					}
					this.path = ManagementPath._Clone(value, new IdentifierChangedEventHandler(this.HandleIdentifierChange));
					this.FireIdentifierChanged();
					return;
				}
			}
		}

		internal bool PutButNotGot
		{
			get
			{
				return this.putButNotGot;
			}
			set
			{
				this.putButNotGot = value;
			}
		}

		public ManagementScope Scope
		{
			get
			{
				if (this.scope != null)
				{
					return this.scope;
				}
				else
				{
					ManagementScope managementScope = ManagementScope._Clone(null);
					ManagementScope managementScope1 = managementScope;
					this.scope = managementScope;
					return managementScope1;
				}
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				else
				{
					if (this.scope != null)
					{
						this.scope.IdentifierChanged -= new IdentifierChangedEventHandler(this.HandleIdentifierChange);
					}
					this.scope = ManagementScope._Clone(value, new IdentifierChangedEventHandler(this.HandleIdentifierChange));
					this.FireIdentifierChanged();
					return;
				}
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ManagementObject() : this((ManagementScope)null, (ManagementPath)null, (ObjectGetOptions)null)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ManagementObject(ManagementPath path) : this(null, path, null)
		{
		}

		public ManagementObject(string path) : this(null, new ManagementPath(path), null)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ManagementObject(ManagementPath path, ObjectGetOptions options) : this(null, path, options)
		{
		}

		public ManagementObject(string path, ObjectGetOptions options) : this(new ManagementPath(path), options)
		{
		}

		public ManagementObject(ManagementScope scope, ManagementPath path, ObjectGetOptions options) : base(null)
		{
			this.ManagementObjectCTOR(scope, path, options);
		}

		public ManagementObject(string scopeString, string pathString, ObjectGetOptions options) : this(new ManagementScope(scopeString), new ManagementPath(pathString), options)
		{
		}

		protected ManagementObject(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			this.ManagementObjectCTOR(null, null, null);
		}

		public override object Clone()
		{
			if (this.PutButNotGot)
			{
				this.Get();
				this.PutButNotGot = false;
			}
			IWbemClassObjectFreeThreaded wbemClassObjectFreeThreaded = null;
			int num = base.wbemObject.Clone_(out wbemClassObjectFreeThreaded);
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
			return ManagementObject.GetManagementObject(wbemClassObjectFreeThreaded, this);
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ManagementPath CopyTo(ManagementPath path)
		{
			return this.CopyTo(path, null);
		}

		public ManagementPath CopyTo(string path)
		{
			return this.CopyTo(new ManagementPath(path), null);
		}

		public ManagementPath CopyTo(string path, PutOptions options)
		{
			return this.CopyTo(new ManagementPath(path), options);
		}

		public ManagementPath CopyTo(ManagementPath path, PutOptions options)
		{
			int callStatus_;
			int num = 0;
			PutOptions putOption;
			this.Initialize(false);
			ManagementScope managementScope = new ManagementScope(path, this.scope);
			managementScope.Initialize();
			if (options != null)
			{
				putOption = options;
			}
			else
			{
				putOption = new PutOptions();
			}
			PutOptions putOption1 = putOption;
			IWbemServices wbemServices = managementScope.GetIWbemServices();
			ManagementPath namespacePath = null;
			IntPtr zero = IntPtr.Zero;
			IntPtr intPtr = IntPtr.Zero;
			IWbemCallResult objectForIUnknown = null;
			SecurityHandler securityHandler = null;
			try
			{
				securityHandler = managementScope.GetSecurityHandler();
				zero = Marshal.AllocHGlobal(IntPtr.Size);
				Marshal.WriteIntPtr(zero, IntPtr.Zero);
				if (!base.IsClass)
				{
					callStatus_ = this.scope.GetSecuredIWbemServicesHandler(wbemServices).PutInstance_(base.wbemObject, putOption1.Flags | 16, putOption1.GetContext(), zero);
				}
				else
				{
					callStatus_ = this.scope.GetSecuredIWbemServicesHandler(wbemServices).PutClass_(base.wbemObject, putOption1.Flags | 16, putOption1.GetContext(), zero);
				}
				intPtr = Marshal.ReadIntPtr(zero);
				objectForIUnknown = (IWbemCallResult)Marshal.GetObjectForIUnknown(intPtr);
				callStatus_ = objectForIUnknown.GetCallStatus_(-1, out num);
				if (callStatus_ >= 0)
				{
					callStatus_ = num;
				}
				if (callStatus_ < 0)
				{
					if (((long)callStatus_ & (long)-4096) != (long)-2147217408)
					{
						Marshal.ThrowExceptionForHR(callStatus_);
					}
					else
					{
						ManagementException.ThrowWithExtendedInfo((ManagementStatus)callStatus_);
					}
				}
				namespacePath = this.GetPath(objectForIUnknown);
				namespacePath.NamespacePath = path.GetNamespacePath(8);
			}
			finally
			{
				if (securityHandler != null)
				{
					securityHandler.Reset();
				}
				if (zero != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(zero);
				}
				if (intPtr != IntPtr.Zero)
				{
					Marshal.Release(intPtr);
				}
				if (objectForIUnknown != null)
				{
					Marshal.ReleaseComObject(objectForIUnknown);
				}
			}
			return namespacePath;
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void CopyTo(ManagementOperationObserver watcher, ManagementPath path)
		{
			this.CopyTo(watcher, path, null);
		}

		public void CopyTo(ManagementOperationObserver watcher, string path)
		{
			this.CopyTo(watcher, new ManagementPath(path), null);
		}

		public void CopyTo(ManagementOperationObserver watcher, string path, PutOptions options)
		{
			this.CopyTo(watcher, new ManagementPath(path), options);
		}

		public void CopyTo(ManagementOperationObserver watcher, ManagementPath path, PutOptions options)
		{
			int num;
			PutOptions putOption;
			if (watcher != null)
			{
				this.Initialize(false);
				ManagementScope managementScope = new ManagementScope(path, this.scope);
				managementScope.Initialize();
				if (options != null)
				{
					putOption = (PutOptions)options.Clone();
				}
				else
				{
					putOption = new PutOptions();
				}
				PutOptions putOption1 = putOption;
				if (watcher.HaveListenersForProgress)
				{
					putOption1.SendStatus = true;
				}
				WmiEventSink newPutSink = watcher.GetNewPutSink(managementScope, putOption1.Context, path.GetNamespacePath(8), base.ClassName);
				IWbemServices wbemServices = managementScope.GetIWbemServices();
				SecurityHandler securityHandler = managementScope.GetSecurityHandler();
				if (!base.IsClass)
				{
					num = managementScope.GetSecuredIWbemServicesHandler(wbemServices).PutInstanceAsync_(base.wbemObject, putOption1.Flags, putOption1.GetContext(), newPutSink.Stub);
				}
				else
				{
					num = managementScope.GetSecuredIWbemServicesHandler(wbemServices).PutClassAsync_(base.wbemObject, putOption1.Flags, putOption1.GetContext(), newPutSink.Stub);
				}
				if (securityHandler != null)
				{
					securityHandler.Reset();
				}
				if (num < 0)
				{
					watcher.RemoveSink(newPutSink);
					if (((long)num & (long)-4096) != (long)-2147217408)
					{
						Marshal.ThrowExceptionForHR(num);
					}
					else
					{
						ManagementException.ThrowWithExtendedInfo((ManagementStatus)num);
						return;
					}
				}
				return;
			}
			else
			{
				throw new ArgumentNullException("watcher");
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void Delete()
		{
			this.Delete((DeleteOptions)null);
		}

		public void Delete(DeleteOptions options)
		{
			int num;
			DeleteOptions deleteOption;
			if (this.path == null || this.path.Path.Length == 0)
			{
				throw new InvalidOperationException();
			}
			else
			{
				this.Initialize(false);
				if (options != null)
				{
					deleteOption = options;
				}
				else
				{
					deleteOption = new DeleteOptions();
				}
				DeleteOptions deleteOption1 = deleteOption;
				IWbemServices wbemServices = this.scope.GetIWbemServices();
				SecurityHandler securityHandler = null;
				try
				{
					securityHandler = this.scope.GetSecurityHandler();
					if (!base.IsClass)
					{
						num = this.scope.GetSecuredIWbemServicesHandler(wbemServices).DeleteInstance_(this.path.RelativePath, deleteOption1.Flags, deleteOption1.GetContext(), IntPtr.Zero);
					}
					else
					{
						num = this.scope.GetSecuredIWbemServicesHandler(wbemServices).DeleteClass_(this.path.RelativePath, deleteOption1.Flags, deleteOption1.GetContext(), IntPtr.Zero);
					}
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
				}
				finally
				{
					if (securityHandler != null)
					{
						securityHandler.Reset();
					}
				}
				return;
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void Delete(ManagementOperationObserver watcher)
		{
			this.Delete(watcher, null);
		}

		public void Delete(ManagementOperationObserver watcher, DeleteOptions options)
		{
			int num;
			DeleteOptions deleteOption;
			if (this.path == null || this.path.Path.Length == 0)
			{
				throw new InvalidOperationException();
			}
			else
			{
				if (watcher != null)
				{
					this.Initialize(false);
					if (options != null)
					{
						deleteOption = (DeleteOptions)options.Clone();
					}
					else
					{
						deleteOption = new DeleteOptions();
					}
					DeleteOptions deleteOption1 = deleteOption;
					if (watcher.HaveListenersForProgress)
					{
						deleteOption1.SendStatus = true;
					}
					IWbemServices wbemServices = this.scope.GetIWbemServices();
					WmiEventSink newSink = watcher.GetNewSink(this.scope, deleteOption1.Context);
					SecurityHandler securityHandler = this.scope.GetSecurityHandler();
					if (!base.IsClass)
					{
						num = this.scope.GetSecuredIWbemServicesHandler(wbemServices).DeleteInstanceAsync_(this.path.RelativePath, deleteOption1.Flags, deleteOption1.GetContext(), newSink.Stub);
					}
					else
					{
						num = this.scope.GetSecuredIWbemServicesHandler(wbemServices).DeleteClassAsync_(this.path.RelativePath, deleteOption1.Flags, deleteOption1.GetContext(), newSink.Stub);
					}
					if (securityHandler != null)
					{
						securityHandler.Reset();
					}
					if (num < 0)
					{
						watcher.RemoveSink(newSink);
						if (((long)num & (long)-4096) != (long)-2147217408)
						{
							Marshal.ThrowExceptionForHR(num);
						}
						else
						{
							ManagementException.ThrowWithExtendedInfo((ManagementStatus)num);
							return;
						}
					}
					return;
				}
				else
				{
					throw new ArgumentNullException("watcher");
				}
			}
		}

		public new void Dispose()
		{
			if (this.wmiClass != null)
			{
				this.wmiClass.Dispose();
				this.wmiClass = null;
			}
			base.Dispose();
			GC.SuppressFinalize(this);
		}

		internal void FireIdentifierChanged()
		{
			if (this.IdentifierChanged != null)
			{
				this.IdentifierChanged(this, null);
			}
		}

		public void Get()
		{
			ObjectGetOptions objectGetOption;
			IWbemClassObjectFreeThreaded wbemClassObjectFreeThreaded = null;
			this.Initialize(false);
			if (this.path == null || this.path.Path.Length == 0)
			{
				throw new InvalidOperationException();
			}
			else
			{
				if (this.options == null)
				{
					objectGetOption = new ObjectGetOptions();
				}
				else
				{
					objectGetOption = this.options;
				}
				ObjectGetOptions objectGetOption1 = objectGetOption;
				SecurityHandler securityHandler = null;
				try
				{
					securityHandler = this.scope.GetSecurityHandler();
					int object_ = this.scope.GetSecuredIWbemServicesHandler(this.scope.GetIWbemServices()).GetObject_(this.path.RelativePath, objectGetOption1.Flags, objectGetOption1.GetContext(), ref wbemClassObjectFreeThreaded, IntPtr.Zero);
					if (object_ < 0)
					{
						if (((long)object_ & (long)-4096) != (long)-2147217408)
						{
							Marshal.ThrowExceptionForHR(object_);
						}
						else
						{
							ManagementException.ThrowWithExtendedInfo((ManagementStatus)object_);
						}
					}
					base.wbemObject = wbemClassObjectFreeThreaded;
				}
				finally
				{
					if (securityHandler != null)
					{
						securityHandler.Reset();
					}
				}
				return;
			}
		}

		public void Get(ManagementOperationObserver watcher)
		{
			this.Initialize(false);
			if (this.path == null || this.path.Path.Length == 0)
			{
				throw new InvalidOperationException();
			}
			else
			{
				if (watcher != null)
				{
					IWbemServices wbemServices = this.scope.GetIWbemServices();
					ObjectGetOptions objectGetOption = ObjectGetOptions._Clone(this.options);
					WmiGetEventSink newGetSink = watcher.GetNewGetSink(this.scope, objectGetOption.Context, this);
					if (watcher.HaveListenersForProgress)
					{
						objectGetOption.SendStatus = true;
					}
					SecurityHandler securityHandler = this.scope.GetSecurityHandler();
					int objectAsync_ = this.scope.GetSecuredIWbemServicesHandler(wbemServices).GetObjectAsync_(this.path.RelativePath, objectGetOption.Flags, objectGetOption.GetContext(), newGetSink.Stub);
					if (securityHandler != null)
					{
						securityHandler.Reset();
					}
					if (objectAsync_ < 0)
					{
						watcher.RemoveSink(newGetSink);
						if (((long)objectAsync_ & (long)-4096) != (long)-2147217408)
						{
							Marshal.ThrowExceptionForHR(objectAsync_);
						}
						else
						{
							ManagementException.ThrowWithExtendedInfo((ManagementStatus)objectAsync_);
							return;
						}
					}
					return;
				}
				else
				{
					throw new ArgumentNullException("watcher");
				}
			}
		}

		internal static ManagementObject GetManagementObject(IWbemClassObjectFreeThreaded wbemObject, ManagementObject mgObj)
		{
			ManagementObject managementObject = new ManagementObject();
			managementObject.wbemObject = wbemObject;
			if (mgObj != null)
			{
				managementObject.scope = ManagementScope._Clone(mgObj.scope);
				if (mgObj.path != null)
				{
					managementObject.path = ManagementPath._Clone(mgObj.path);
				}
				if (mgObj.options != null)
				{
					managementObject.options = ObjectGetOptions._Clone(mgObj.options);
				}
			}
			return managementObject;
		}

		internal static ManagementObject GetManagementObject(IWbemClassObjectFreeThreaded wbemObject, ManagementScope scope)
		{
			ManagementObject managementObject = new ManagementObject();
			managementObject.wbemObject = wbemObject;
			managementObject.path = new ManagementPath(ManagementPath.GetManagementPath(wbemObject));
			managementObject.path.IdentifierChanged += new IdentifierChangedEventHandler(managementObject.HandleIdentifierChange);
			managementObject.scope = ManagementScope._Clone(scope, new IdentifierChangedEventHandler(managementObject.HandleIdentifierChange));
			return managementObject;
		}

		public ManagementBaseObject GetMethodParameters(string methodName)
		{
			ManagementBaseObject managementBaseObject = null;
			IWbemClassObjectFreeThreaded wbemClassObjectFreeThreaded = null;
			IWbemClassObjectFreeThreaded wbemClassObjectFreeThreaded1 = null;
			this.GetMethodParameters(methodName, out managementBaseObject, out wbemClassObjectFreeThreaded, out wbemClassObjectFreeThreaded1);
			return managementBaseObject;
		}

		private void GetMethodParameters(string methodName, out ManagementBaseObject inParameters, out IWbemClassObjectFreeThreaded inParametersClass, out IWbemClassObjectFreeThreaded outParametersClass)
		{
			inParameters = null;
			inParametersClass = null;
			outParametersClass = null;
			if (methodName != null)
			{
				this.Initialize(false);
				if (this.wmiClass == null)
				{
					ManagementPath classPath = this.ClassPath;
					if (classPath == null || !classPath.IsClass)
					{
						throw new InvalidOperationException();
					}
					else
					{
						ManagementClass managementClass = new ManagementClass(this.scope, classPath, null);
						managementClass.Path = classPath;
						managementClass.Get();
						this.wmiClass = managementClass.wbemObject;
					}
				}
				int method_ = this.wmiClass.GetMethod_(methodName, 0, out inParametersClass, out outParametersClass);
				if (method_ == -2147217406)
				{
					method_ = -2147217323;
				}
				if (method_ >= 0 && inParametersClass != null)
				{
					IWbemClassObjectFreeThreaded wbemClassObjectFreeThreaded = null;
					method_ = inParametersClass.SpawnInstance_(0, out wbemClassObjectFreeThreaded);
					if (method_ >= 0)
					{
						inParameters = new ManagementBaseObject(wbemClassObjectFreeThreaded);
					}
				}
				if (method_ < 0)
				{
					if (((long)method_ & (long)-4096) != (long)-2147217408)
					{
						Marshal.ThrowExceptionForHR(method_);
					}
					else
					{
						ManagementException.ThrowWithExtendedInfo((ManagementStatus)method_);
						return;
					}
				}
				return;
			}
			else
			{
				throw new ArgumentNullException("methodName");
			}
		}

		protected override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
		}

		private ManagementPath GetPath(IWbemCallResult callResult)
		{
			ManagementPath managementPath = null;
			try
			{
				string str = null;
				int resultString_ = callResult.GetResultString_(-1, out str);
				if (resultString_ < 0)
				{
					object propertyValue = base.GetPropertyValue("__PATH");
					if (propertyValue == null)
					{
						propertyValue = base.GetPropertyValue("__RELPATH");
						if (propertyValue != null)
						{
							managementPath = new ManagementPath(this.scope.Path.Path);
							managementPath.RelativePath = (string)propertyValue;
						}
					}
					else
					{
						managementPath = new ManagementPath((string)propertyValue);
					}
				}
				else
				{
					managementPath = new ManagementPath(this.scope.Path.Path);
					managementPath.RelativePath = str;
				}
			}
			catch
			{
			}
			if (managementPath == null)
			{
				managementPath = new ManagementPath();
			}
			return managementPath;
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ManagementObjectCollection GetRelated()
		{
			return this.GetRelated((string)null);
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ManagementObjectCollection GetRelated(string relatedClass)
		{
			return this.GetRelated(relatedClass, null, null, null, null, null, false, null);
		}

		public ManagementObjectCollection GetRelated(string relatedClass, string relationshipClass, string relationshipQualifier, string relatedQualifier, string relatedRole, string thisRole, bool classDefinitionsOnly, EnumerationOptions options)
		{
			EnumerationOptions enumerationOption;
			if (this.path == null || this.path.Path.Length == 0)
			{
				throw new InvalidOperationException();
			}
			else
			{
				this.Initialize(false);
				IEnumWbemClassObject enumWbemClassObject = null;
				if (options != null)
				{
					enumerationOption = options;
				}
				else
				{
					enumerationOption = new EnumerationOptions();
				}
				EnumerationOptions enumerationOption1 = enumerationOption;
				RelatedObjectQuery relatedObjectQuery = new RelatedObjectQuery(this.path.Path, relatedClass, relationshipClass, relationshipQualifier, relatedQualifier, relatedRole, thisRole, classDefinitionsOnly);
				enumerationOption1.EnumerateDeep = true;
				SecurityHandler securityHandler = null;
				try
				{
					securityHandler = this.scope.GetSecurityHandler();
					int num = this.scope.GetSecuredIWbemServicesHandler(this.scope.GetIWbemServices()).ExecQuery_(relatedObjectQuery.QueryLanguage, relatedObjectQuery.QueryString, enumerationOption1.Flags, enumerationOption1.GetContext(), ref enumWbemClassObject);
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
				}
				finally
				{
					if (securityHandler != null)
					{
						securityHandler.Reset();
					}
				}
				return new ManagementObjectCollection(this.scope, enumerationOption1, enumWbemClassObject);
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void GetRelated(ManagementOperationObserver watcher)
		{
			this.GetRelated(watcher, null);
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void GetRelated(ManagementOperationObserver watcher, string relatedClass)
		{
			this.GetRelated(watcher, relatedClass, null, null, null, null, null, false, null);
		}

		public void GetRelated(ManagementOperationObserver watcher, string relatedClass, string relationshipClass, string relationshipQualifier, string relatedQualifier, string relatedRole, string thisRole, bool classDefinitionsOnly, EnumerationOptions options)
		{
			EnumerationOptions enumerationOption;
			if (this.path == null || this.path.Path.Length == 0)
			{
				throw new InvalidOperationException();
			}
			else
			{
				this.Initialize(true);
				if (watcher != null)
				{
					if (options != null)
					{
						enumerationOption = (EnumerationOptions)options.Clone();
					}
					else
					{
						enumerationOption = new EnumerationOptions();
					}
					EnumerationOptions enumerationOption1 = enumerationOption;
					enumerationOption1.ReturnImmediately = false;
					if (watcher.HaveListenersForProgress)
					{
						enumerationOption1.SendStatus = true;
					}
					WmiEventSink newSink = watcher.GetNewSink(this.scope, enumerationOption1.Context);
					RelatedObjectQuery relatedObjectQuery = new RelatedObjectQuery(this.path.Path, relatedClass, relationshipClass, relationshipQualifier, relatedQualifier, relatedRole, thisRole, classDefinitionsOnly);
					enumerationOption1.EnumerateDeep = true;
					SecurityHandler securityHandler = this.scope.GetSecurityHandler();
					int num = this.scope.GetSecuredIWbemServicesHandler(this.scope.GetIWbemServices()).ExecQueryAsync_(relatedObjectQuery.QueryLanguage, relatedObjectQuery.QueryString, enumerationOption1.Flags, enumerationOption1.GetContext(), newSink.Stub);
					securityHandler.Reset();
					if (num < 0)
					{
						watcher.RemoveSink(newSink);
						if (((long)num & (long)-4096) != (long)-2147217408)
						{
							Marshal.ThrowExceptionForHR(num);
						}
						else
						{
							ManagementException.ThrowWithExtendedInfo((ManagementStatus)num);
							return;
						}
					}
					return;
				}
				else
				{
					throw new ArgumentNullException("watcher");
				}
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ManagementObjectCollection GetRelationships()
		{
			return this.GetRelationships((string)null);
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ManagementObjectCollection GetRelationships(string relationshipClass)
		{
			return this.GetRelationships(relationshipClass, null, null, false, null);
		}

		public ManagementObjectCollection GetRelationships(string relationshipClass, string relationshipQualifier, string thisRole, bool classDefinitionsOnly, EnumerationOptions options)
		{
			EnumerationOptions enumerationOption;
			if (this.path == null || this.path.Path.Length == 0)
			{
				throw new InvalidOperationException();
			}
			else
			{
				this.Initialize(false);
				IEnumWbemClassObject enumWbemClassObject = null;
				if (options != null)
				{
					enumerationOption = options;
				}
				else
				{
					enumerationOption = new EnumerationOptions();
				}
				EnumerationOptions enumerationOption1 = enumerationOption;
				RelationshipQuery relationshipQuery = new RelationshipQuery(this.path.Path, relationshipClass, relationshipQualifier, thisRole, classDefinitionsOnly);
				enumerationOption1.EnumerateDeep = true;
				SecurityHandler securityHandler = null;
				try
				{
					securityHandler = this.scope.GetSecurityHandler();
					int num = this.scope.GetSecuredIWbemServicesHandler(this.scope.GetIWbemServices()).ExecQuery_(relationshipQuery.QueryLanguage, relationshipQuery.QueryString, enumerationOption1.Flags, enumerationOption1.GetContext(), ref enumWbemClassObject);
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
				}
				finally
				{
					if (securityHandler != null)
					{
						securityHandler.Reset();
					}
				}
				return new ManagementObjectCollection(this.scope, enumerationOption1, enumWbemClassObject);
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void GetRelationships(ManagementOperationObserver watcher)
		{
			this.GetRelationships(watcher, null);
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void GetRelationships(ManagementOperationObserver watcher, string relationshipClass)
		{
			this.GetRelationships(watcher, relationshipClass, null, null, false, null);
		}

		public void GetRelationships(ManagementOperationObserver watcher, string relationshipClass, string relationshipQualifier, string thisRole, bool classDefinitionsOnly, EnumerationOptions options)
		{
			EnumerationOptions enumerationOption;
			if (this.path == null || this.path.Path.Length == 0)
			{
				throw new InvalidOperationException();
			}
			else
			{
				if (watcher != null)
				{
					this.Initialize(false);
					if (options != null)
					{
						enumerationOption = (EnumerationOptions)options.Clone();
					}
					else
					{
						enumerationOption = new EnumerationOptions();
					}
					EnumerationOptions enumerationOption1 = enumerationOption;
					enumerationOption1.ReturnImmediately = false;
					if (watcher.HaveListenersForProgress)
					{
						enumerationOption1.SendStatus = true;
					}
					WmiEventSink newSink = watcher.GetNewSink(this.scope, enumerationOption1.Context);
					RelationshipQuery relationshipQuery = new RelationshipQuery(this.path.Path, relationshipClass, relationshipQualifier, thisRole, classDefinitionsOnly);
					enumerationOption1.EnumerateDeep = true;
					SecurityHandler securityHandler = this.scope.GetSecurityHandler();
					int num = this.scope.GetSecuredIWbemServicesHandler(this.scope.GetIWbemServices()).ExecQueryAsync_(relationshipQuery.QueryLanguage, relationshipQuery.QueryString, enumerationOption1.Flags, enumerationOption1.GetContext(), newSink.Stub);
					if (securityHandler != null)
					{
						securityHandler.Reset();
					}
					if (num < 0)
					{
						watcher.RemoveSink(newSink);
						if (((long)num & (long)-4096) != (long)-2147217408)
						{
							Marshal.ThrowExceptionForHR(num);
						}
						else
						{
							ManagementException.ThrowWithExtendedInfo((ManagementStatus)num);
							return;
						}
					}
					return;
				}
				else
				{
					throw new ArgumentNullException("watcher");
				}
			}
		}

		private void HandleIdentifierChange(object sender, IdentifierChangedEventArgs e)
		{
			base.wbemObject = null;
		}

		internal void HandleObjectPut(object sender, InternalObjectPutEventArgs e)
		{
			try
			{
				if (sender as WmiEventSink != null)
				{
					((WmiEventSink)sender).InternalObjectPut -= new InternalObjectPutEventHandler(this.HandleObjectPut);
					this.putButNotGot = true;
					this.path.SetRelativePath(e.Path.RelativePath);
				}
			}
			catch
			{
			}
		}

		internal override void Initialize(bool getObject)
		{
			ManagementPath managementPath;
			bool flag = false;
			lock (this)
			{
				if (this.path == null)
				{
					this.path = new ManagementPath();
					this.path.IdentifierChanged += new IdentifierChangedEventHandler(this.HandleIdentifierChange);
				}
				if (!this.IsBound && getObject)
				{
					flag = true;
				}
				if (this.scope != null)
				{
					if (this.scope.Path == null || this.scope.Path.IsEmpty)
					{
						string namespacePath = this.path.GetNamespacePath(8);
						if (0 >= namespacePath.Length)
						{
							this.scope.Path = ManagementPath.DefaultPath;
						}
						else
						{
							this.scope.Path = new ManagementPath(namespacePath);
						}
					}
				}
				else
				{
					string str = this.path.GetNamespacePath(8);
					if (0 >= str.Length)
					{
						this.scope = new ManagementScope();
					}
					else
					{
						this.scope = new ManagementScope(str);
					}
					this.scope.IdentifierChanged += new IdentifierChangedEventHandler(this.HandleIdentifierChange);
				}
				lock (this.scope)
				{
					if (!this.scope.IsConnected)
					{
						this.scope.Initialize();
						if (getObject)
						{
							flag = true;
						}
					}
					if (flag || _wbemObject == null)
					{
						if (this.options == null)
						{
							this.options = new ObjectGetOptions();
							this.options.IdentifierChanged += new IdentifierChangedEventHandler(this.HandleIdentifierChange);
						}
						IWbemClassObjectFreeThreaded wbemClassObjectFreeThreaded = null;
						IWbemServices wbemServices = this.scope.GetIWbemServices();
						SecurityHandler securityHandler = null;
						try
						{
							securityHandler = this.scope.GetSecurityHandler();
							string str1 = null;
							string relativePath = this.path.RelativePath;
							if (relativePath.Length > 0)
							{
								str1 = relativePath;
							}
							int object_ = this.scope.GetSecuredIWbemServicesHandler(wbemServices).GetObject_(str1, this.options.Flags, this.options.GetContext(), ref wbemClassObjectFreeThreaded, IntPtr.Zero);
							if (object_ >= 0)
							{
								base.wbemObject = wbemClassObjectFreeThreaded;
								object obj = null;
								int num = 0;
								int num1 = 0;
								object_ = base.wbemObject.Get_("__PATH", 0, ref obj, ref num, ref num1);
								if (object_ >= 0)
								{
									ManagementObject managementObject = this;
									if (DBNull.Value != obj)
									{
										managementPath = new ManagementPath((string)obj);
									}
									else
									{
										managementPath = new ManagementPath();
									}
									managementObject.path = managementPath;
									this.path.IdentifierChanged += new IdentifierChangedEventHandler(this.HandleIdentifierChange);
								}
							}
							if (object_ < 0)
							{
								if (((long)object_ & (long)-4096) != (long)-2147217408)
								{
									Marshal.ThrowExceptionForHR(object_);
								}
								else
								{
									ManagementException.ThrowWithExtendedInfo((ManagementStatus)object_);
								}
							}
						}
						finally
						{
							if (securityHandler != null)
							{
								securityHandler.Reset();
							}
						}
					}
				}
			}
		}

		public object InvokeMethod(string methodName, object[] args)
		{
			ManagementBaseObject managementBaseObject = null;
			IWbemClassObjectFreeThreaded wbemClassObjectFreeThreaded = null;
			IWbemClassObjectFreeThreaded wbemClassObjectFreeThreaded1 = null;
			if (this.path == null || this.path.Path.Length == 0)
			{
				throw new InvalidOperationException();
			}
			else
			{
				if (methodName != null)
				{
					this.Initialize(false);
					this.GetMethodParameters(methodName, out managementBaseObject, out wbemClassObjectFreeThreaded, out wbemClassObjectFreeThreaded1);
					ManagementObject.MapInParameters(args, managementBaseObject, wbemClassObjectFreeThreaded);
					ManagementBaseObject managementBaseObject1 = this.InvokeMethod(methodName, managementBaseObject, null);
					object obj = ManagementObject.MapOutParameters(args, managementBaseObject1, wbemClassObjectFreeThreaded1);
					return obj;
				}
				else
				{
					throw new ArgumentNullException("methodName");
				}
			}
		}

		public void InvokeMethod(ManagementOperationObserver watcher, string methodName, object[] args)
		{
			ManagementBaseObject managementBaseObject = null;
			IWbemClassObjectFreeThreaded wbemClassObjectFreeThreaded = null;
			IWbemClassObjectFreeThreaded wbemClassObjectFreeThreaded1 = null;
			if (this.path == null || this.path.Path.Length == 0)
			{
				throw new InvalidOperationException();
			}
			else
			{
				if (watcher != null)
				{
					if (methodName != null)
					{
						this.Initialize(false);
						this.GetMethodParameters(methodName, out managementBaseObject, out wbemClassObjectFreeThreaded, out wbemClassObjectFreeThreaded1);
						ManagementObject.MapInParameters(args, managementBaseObject, wbemClassObjectFreeThreaded);
						this.InvokeMethod(watcher, methodName, managementBaseObject, null);
						return;
					}
					else
					{
						throw new ArgumentNullException("methodName");
					}
				}
				else
				{
					throw new ArgumentNullException("watcher");
				}
			}
		}

		public ManagementBaseObject InvokeMethod(string methodName, ManagementBaseObject inParameters, InvokeMethodOptions options)
		{
			InvokeMethodOptions invokeMethodOption;
			IWbemClassObjectFreeThreaded wbemClassObjectFreeThreaded;
			ManagementBaseObject managementBaseObject = null;
			if (this.path == null || this.path.Path.Length == 0)
			{
				throw new InvalidOperationException();
			}
			else
			{
				var callPath = this.path;
				if (methodName != null)
				{
					this.Initialize(false);
					if (options != null)
					{
						invokeMethodOption = options;
					}
					else
					{
						invokeMethodOption = new InvokeMethodOptions();
					}
					InvokeMethodOptions invokeMethodOption1 = invokeMethodOption;
					this.scope.GetIWbemServices();
					SecurityHandler securityHandler = null;
					try
					{
						securityHandler = this.scope.GetSecurityHandler();
						if (inParameters == null)
						{
							wbemClassObjectFreeThreaded = null;
						}
						else
						{
							wbemClassObjectFreeThreaded = inParameters.wbemObject;
						}
						if (!this.path.IsEmpty)
						{
							callPath = this.path;
						}
						IWbemClassObjectFreeThreaded wbemClassObjectFreeThreaded1 = wbemClassObjectFreeThreaded;
						IWbemClassObjectFreeThreaded wbemClassObjectFreeThreaded2 = null;
						int num = this.scope.GetSecuredIWbemServicesHandler(this.scope.GetIWbemServices()).ExecMethod_(callPath.RelativePath, methodName, invokeMethodOption1.Flags, invokeMethodOption1.GetContext(), wbemClassObjectFreeThreaded1, ref wbemClassObjectFreeThreaded2, IntPtr.Zero);
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
						if (wbemClassObjectFreeThreaded2 != null)
						{
							managementBaseObject = new ManagementBaseObject(wbemClassObjectFreeThreaded2);
						}
					}
					finally
					{
						if (securityHandler != null)
						{
							securityHandler.Reset();
						}
					}
					return managementBaseObject;
				}
				else
				{
					throw new ArgumentNullException("methodName");
				}
			}
		}

		public void InvokeMethod(ManagementOperationObserver watcher, string methodName, ManagementBaseObject inParameters, InvokeMethodOptions options)
		{
			InvokeMethodOptions invokeMethodOption;
			if (this.path == null || this.path.Path.Length == 0)
			{
				throw new InvalidOperationException();
			}
			else
			{
				if (watcher != null)
				{
					if (methodName != null)
					{
						this.Initialize(false);
						if (options != null)
						{
							invokeMethodOption = (InvokeMethodOptions)options.Clone();
						}
						else
						{
							invokeMethodOption = new InvokeMethodOptions();
						}
						InvokeMethodOptions invokeMethodOption1 = invokeMethodOption;
						if (watcher.HaveListenersForProgress)
						{
							invokeMethodOption1.SendStatus = true;
						}
						WmiEventSink newSink = watcher.GetNewSink(this.scope, invokeMethodOption1.Context);
						SecurityHandler securityHandler = this.scope.GetSecurityHandler();
						IWbemClassObjectFreeThreaded wbemClassObjectFreeThreaded = null;
						if (inParameters != null)
						{
							wbemClassObjectFreeThreaded = inParameters.wbemObject;
						}
						int num = this.scope.GetSecuredIWbemServicesHandler(this.scope.GetIWbemServices()).ExecMethodAsync_(this.path.RelativePath, methodName, invokeMethodOption1.Flags, invokeMethodOption1.GetContext(), wbemClassObjectFreeThreaded, newSink.Stub);
						if (securityHandler != null)
						{
							securityHandler.Reset();
						}
						if (num < 0)
						{
							watcher.RemoveSink(newSink);
							if (((long)num & (long)-4096) != (long)-2147217408)
							{
								Marshal.ThrowExceptionForHR(num);
							}
							else
							{
								ManagementException.ThrowWithExtendedInfo((ManagementStatus)num);
								return;
							}
						}
						return;
					}
					else
					{
						throw new ArgumentNullException("methodName");
					}
				}
				else
				{
					throw new ArgumentNullException("watcher");
				}
			}
		}

		private void ManagementObjectCTOR(ManagementScope scope, ManagementPath path, ObjectGetOptions options)
		{
			string empty = string.Empty;
			if (path != null && !path.IsEmpty)
			{
				if (!(this.GetType() == typeof(ManagementObject)) || !path.IsClass)
				{
					if (!(this.GetType() == typeof(ManagementClass)) || !path.IsInstance)
					{
						empty = path.GetNamespacePath(8);
						if (scope != null && scope.Path.NamespacePath.Length > 0)
						{
							path = new ManagementPath(path.RelativePath);
							path.NamespacePath = scope.Path.GetNamespacePath(8);
						}
						if (path.IsClass || path.IsInstance)
						{
							this.path = ManagementPath._Clone(path, new IdentifierChangedEventHandler(this.HandleIdentifierChange));
						}
						else
						{
							this.path = ManagementPath._Clone(null, new IdentifierChangedEventHandler(this.HandleIdentifierChange));
						}
					}
					else
					{
						throw new ArgumentOutOfRangeException("path");
					}
				}
				else
				{
					throw new ArgumentOutOfRangeException("path");
				}
			}
			if (options != null)
			{
				this.options = ObjectGetOptions._Clone(options, new IdentifierChangedEventHandler(this.HandleIdentifierChange));
			}
			if (scope == null)
			{
				if (empty.Length > 0)
				{
					this.scope = new ManagementScope(empty);
					this.scope.IdentifierChanged += new IdentifierChangedEventHandler(this.HandleIdentifierChange);
				}
			}
			else
			{
				this.scope = ManagementScope._Clone(scope, new IdentifierChangedEventHandler(this.HandleIdentifierChange));
			}
			this.IdentifierChanged += new IdentifierChangedEventHandler(this.HandleIdentifierChange);
			this.putButNotGot = false;
		}

		private static void MapInParameters(object[] args, ManagementBaseObject inParams, IWbemClassObjectFreeThreaded inParamsClass)
		{
			if (inParamsClass != null && args != null && 0 < (int)args.Length)
			{
				int upperBound = args.GetUpperBound(0);
				int lowerBound = args.GetLowerBound(0);
				int num = upperBound - lowerBound;
				int propertyQualifierSet_ = inParamsClass.BeginEnumeration_(64);
				if (propertyQualifierSet_ >= 0)
				{
					do
					{
						object obj = null;
						int num1 = 0;
						string str = null;
						IWbemQualifierSetFreeThreaded wbemQualifierSetFreeThreaded = null;
						propertyQualifierSet_ = inParamsClass.Next_(0, ref str, ref obj, ref num1, ref num1);
						if (propertyQualifierSet_ < 0)
						{
							continue;
						}
						if (str == null)
						{
							break;
						}
						propertyQualifierSet_ = inParamsClass.GetPropertyQualifierSet_(str, out wbemQualifierSetFreeThreaded);
						if (propertyQualifierSet_ < 0)
						{
							continue;
						}
						try
						{
							object obj1 = 0;
							wbemQualifierSetFreeThreaded.Get_("ID", 0, ref obj1, ref num1);
							int num2 = (int)obj1;
							if (0 <= num2 && num >= num2)
							{
								inParams[str] = args[lowerBound + num2];
							}
						}
						finally
						{
							wbemQualifierSetFreeThreaded.Dispose();
						}
					}
					while (propertyQualifierSet_ >= 0);
				}
				if (propertyQualifierSet_ < 0)
				{
					if (((long)propertyQualifierSet_ & (long)-4096) != (long)-2147217408)
					{
						Marshal.ThrowExceptionForHR(propertyQualifierSet_);
					}
					else
					{
						ManagementException.ThrowWithExtendedInfo((ManagementStatus)propertyQualifierSet_);
						return;
					}
				}
			}
		}

		private static object MapOutParameters(object[] args, ManagementBaseObject outParams, IWbemClassObjectFreeThreaded outParamsClass)
		{
			object item = null;
			int lowerBound = 0;
			int num = 0;
			if (outParamsClass != null)
			{
				if (args != null && 0 < (int)args.Length)
				{
					int upperBound = args.GetUpperBound(0);
					lowerBound = args.GetLowerBound(0);
					num = upperBound - lowerBound;
				}
				int propertyQualifierSet_ = outParamsClass.BeginEnumeration_(64);
				if (propertyQualifierSet_ >= 0)
				{
					do
					{
						object obj = null;
						int num1 = 0;
						string str = null;
						IWbemQualifierSetFreeThreaded wbemQualifierSetFreeThreaded = null;
						propertyQualifierSet_ = outParamsClass.Next_(0, ref str, ref obj, ref num1, ref num1);
						if (propertyQualifierSet_ < 0)
						{
							continue;
						}
						if (str == null)
						{
							break;
						}
						if (string.Compare(str, "RETURNVALUE", StringComparison.OrdinalIgnoreCase) != 0)
						{
							propertyQualifierSet_ = outParamsClass.GetPropertyQualifierSet_(str, out wbemQualifierSetFreeThreaded);
							if (propertyQualifierSet_ < 0)
							{
								continue;
							}
							try
							{
								object obj1 = 0;
								wbemQualifierSetFreeThreaded.Get_("ID", 0, ref obj1, ref num1);
								int num2 = (int)obj1;
								if (0 <= num2 && num >= num2)
								{
									args[lowerBound + num2] = outParams[str];
								}
							}
							finally
							{
								wbemQualifierSetFreeThreaded.Dispose();
							}
						}
						else
						{
							item = outParams["RETURNVALUE"];
						}
					}
					while (propertyQualifierSet_ >= 0);
				}
				if (propertyQualifierSet_ < 0)
				{
					if (((long)propertyQualifierSet_ & (long)-4096) != (long)-2147217408)
					{
						Marshal.ThrowExceptionForHR(propertyQualifierSet_);
					}
					else
					{
						ManagementException.ThrowWithExtendedInfo((ManagementStatus)propertyQualifierSet_);
					}
				}
			}
			return item;
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ManagementPath Put()
		{
			return this.Put((PutOptions)null);
		}

		public ManagementPath Put(PutOptions options)
		{
			int callStatus_;
			int num = 0;
			PutOptions putOption;
			ManagementPath path = null;
			this.Initialize(true);
			if (options != null)
			{
				putOption = options;
			}
			else
			{
				putOption = new PutOptions();
			}
			PutOptions putOption1 = putOption;
			IWbemServices wbemServices = this.scope.GetIWbemServices();
			IntPtr zero = IntPtr.Zero;
			IntPtr intPtr = IntPtr.Zero;
			IWbemCallResult objectForIUnknown = null;
			SecurityHandler securityHandler = null;
			try
			{
				securityHandler = this.scope.GetSecurityHandler();
				zero = Marshal.AllocHGlobal(IntPtr.Size);
				Marshal.WriteIntPtr(zero, IntPtr.Zero);
				if (!base.IsClass)
				{
					callStatus_ = this.scope.GetSecuredIWbemServicesHandler(wbemServices).PutInstance_(base.wbemObject, putOption1.Flags | 16, putOption1.GetContext(), zero);
				}
				else
				{
					callStatus_ = this.scope.GetSecuredIWbemServicesHandler(wbemServices).PutClass_(base.wbemObject, putOption1.Flags | 16, putOption1.GetContext(), zero);
				}
				intPtr = Marshal.ReadIntPtr(zero);
				objectForIUnknown = (IWbemCallResult)Marshal.GetObjectForIUnknown(intPtr);
				callStatus_ = objectForIUnknown.GetCallStatus_(-1, out num);
				if (callStatus_ >= 0)
				{
					callStatus_ = num;
				}
				if (callStatus_ < 0)
				{
					if (((long)callStatus_ & (long)-4096) != (long)-2147217408)
					{
						Marshal.ThrowExceptionForHR(callStatus_);
					}
					else
					{
						ManagementException.ThrowWithExtendedInfo((ManagementStatus)callStatus_);
					}
				}
				path = this.GetPath(objectForIUnknown);
			}
			finally
			{
				if (securityHandler != null)
				{
					securityHandler.Reset();
				}
				if (zero != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(zero);
				}
				if (intPtr != IntPtr.Zero)
				{
					Marshal.Release(intPtr);
				}
				if (objectForIUnknown != null)
				{
					Marshal.ReleaseComObject(objectForIUnknown);
				}
			}
			this.putButNotGot = true;
			this.path.SetRelativePath(path.RelativePath);
			return path;
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void Put(ManagementOperationObserver watcher)
		{
			this.Put(watcher, null);
		}

		public void Put(ManagementOperationObserver watcher, PutOptions options)
		{
			int num;
			PutOptions putOption;
			if (watcher != null)
			{
				this.Initialize(false);
				if (options == null)
				{
					putOption = new PutOptions();
				}
				else
				{
					putOption = (PutOptions)options.Clone();
				}
				PutOptions putOption1 = putOption;
				if (watcher.HaveListenersForProgress)
				{
					putOption1.SendStatus = true;
				}
				IWbemServices wbemServices = this.scope.GetIWbemServices();
				WmiEventSink newPutSink = watcher.GetNewPutSink(this.scope, putOption1.Context, this.scope.Path.GetNamespacePath(8), base.ClassName);
				newPutSink.InternalObjectPut += new InternalObjectPutEventHandler(this.HandleObjectPut);
				SecurityHandler securityHandler = this.scope.GetSecurityHandler();
				if (!base.IsClass)
				{
					num = this.scope.GetSecuredIWbemServicesHandler(wbemServices).PutInstanceAsync_(base.wbemObject, putOption1.Flags, putOption1.GetContext(), newPutSink.Stub);
				}
				else
				{
					num = this.scope.GetSecuredIWbemServicesHandler(wbemServices).PutClassAsync_(base.wbemObject, putOption1.Flags, putOption1.GetContext(), newPutSink.Stub);
				}
				if (securityHandler != null)
				{
					securityHandler.Reset();
				}
				if (num < 0)
				{
					newPutSink.InternalObjectPut -= new InternalObjectPutEventHandler(this.HandleObjectPut);
					watcher.RemoveSink(newPutSink);
					if (((long)num & (long)-4096) != (long)-2147217408)
					{
						Marshal.ThrowExceptionForHR(num);
					}
					else
					{
						ManagementException.ThrowWithExtendedInfo((ManagementStatus)num);
						return;
					}
				}
				return;
			}
			else
			{
				throw new ArgumentNullException("watcher");
			}
		}

		public override string ToString()
		{
			if (this.path == null)
			{
				return "";
			}
			else
			{
				return this.path.Path;
			}
		}

		internal event IdentifierChangedEventHandler IdentifierChanged;
	}
}