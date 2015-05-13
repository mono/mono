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
using System.CodeDom;
using System.Collections.Specialized;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Management
{
	[Serializable]
	public class ManagementClass : ManagementObject
	{
		private MethodDataCollection methods;

		public StringCollection Derivation
		{
			get
			{
				StringCollection stringCollections = new StringCollection();
				int num = 0;
				int num1 = 0;
				object obj = null;
				int num2 = base.wbemObject.Get_("__DERIVATION", 0, ref obj, ref num, ref num1);
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
				if (obj != null)
				{
					stringCollections.AddRange((string[])obj);
				}
				return stringCollections;
			}
		}

		public MethodDataCollection Methods
		{
			get
			{
				this.Initialize(true);
				if (this.methods == null)
				{
					this.methods = new MethodDataCollection(this);
				}
				return this.methods;
			}
		}

		public override ManagementPath Path
		{
			get
			{
				return base.Path;
			}
			set
			{
				if (value == null || value.IsClass || value.IsEmpty)
				{
					base.Path = value;
					return;
				}
				else
				{
					throw new ArgumentOutOfRangeException("value");
				}
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ManagementClass() : this((ManagementScope)null, (ManagementPath)null, (ObjectGetOptions)null)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ManagementClass(ManagementPath path) : this(null, path, null)
		{
		}

		public ManagementClass(string path) : this(null, new ManagementPath(path), null)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ManagementClass(ManagementPath path, ObjectGetOptions options) : this(null, path, options)
		{
		}

		public ManagementClass(string path, ObjectGetOptions options) : this(null, new ManagementPath(path), options)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ManagementClass(ManagementScope scope, ManagementPath path, ObjectGetOptions options) : base(scope, path, options)
		{
		}

		public ManagementClass(string scope, string path, ObjectGetOptions options) : base(new ManagementScope(scope), new ManagementPath(path), options)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected ManagementClass(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override object Clone()
		{
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
			return ManagementClass.GetManagementClass(wbemClassObjectFreeThreaded, this);
		}

		public ManagementObject CreateInstance()
		{
			ManagementObject managementObject = null;
			if (base.PutButNotGot)
			{
				base.Get();
				base.PutButNotGot = false;
			}
			IWbemClassObjectFreeThreaded wbemClassObjectFreeThreaded = null;
			int num = base.wbemObject.SpawnInstance_(0, out wbemClassObjectFreeThreaded);
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
			else
			{
				managementObject = ManagementObject.GetManagementObject(wbemClassObjectFreeThreaded, base.Scope);
			}
			return managementObject;
		}

		public ManagementClass Derive(string newClassName)
		{
			ManagementClass managementClass = null;
			if (newClassName != null)
			{
				ManagementPath managementPath = new ManagementPath();
				try
				{
					managementPath.ClassName = newClassName;
				}
				catch
				{
					throw new ArgumentOutOfRangeException("newClassName");
				}
				if (managementPath.IsClass)
				{
					if (base.PutButNotGot)
					{
						base.Get();
						base.PutButNotGot = false;
					}
					IWbemClassObjectFreeThreaded wbemClassObjectFreeThreaded = null;
					int num = base.wbemObject.SpawnDerivedClass_(0, out wbemClassObjectFreeThreaded);
					if (num >= 0)
					{
						object obj1 = newClassName;
						num = wbemClassObjectFreeThreaded.Put_("__CLASS", 0, ref obj1, 0);
						if (num >= 0)
						{
							managementClass = ManagementClass.GetManagementClass(wbemClassObjectFreeThreaded, this);
						}
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
					return managementClass;
				}
				else
				{
					throw new ArgumentOutOfRangeException("newClassName");
				}
			}
			else
			{
				throw new ArgumentNullException("newClassName");
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ManagementObjectCollection GetInstances()
		{
			return this.GetInstances((EnumerationOptions)null);
		}

		public ManagementObjectCollection GetInstances(EnumerationOptions options)
		{
			EnumerationOptions enumerationOption;
			if (this.Path == null || this.Path.Path == null || this.Path.Path.Length == 0)
			{
				throw new InvalidOperationException();
			}
			else
			{
				this.Initialize(false);
				IEnumWbemClassObject enumWbemClassObject = null;
				if (options == null)
				{
					enumerationOption = new EnumerationOptions();
				}
				else
				{
					enumerationOption = (EnumerationOptions)options.Clone();
				}
				EnumerationOptions enumerationOption1 = enumerationOption;
				enumerationOption1.EnsureLocatable = false;
				enumerationOption1.PrototypeOnly = false;
				SecurityHandler securityHandler = null;
				int num = 0;
				try
				{
					securityHandler = base.Scope.GetSecurityHandler();
					num = this.scope.GetSecuredIWbemServicesHandler(base.Scope.GetIWbemServices()).CreateInstanceEnum_(base.ClassName, enumerationOption1.Flags, enumerationOption1.GetContext(), ref enumWbemClassObject);
				}
				finally
				{
					if (securityHandler != null)
					{
						securityHandler.Reset();
					}
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
				return new ManagementObjectCollection(base.Scope, enumerationOption1, enumWbemClassObject);
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void GetInstances(ManagementOperationObserver watcher)
		{
			this.GetInstances(watcher, null);
		}

		public void GetInstances(ManagementOperationObserver watcher, EnumerationOptions options)
		{
			EnumerationOptions enumerationOption;
			if (watcher != null)
			{
				if (this.Path == null || this.Path.Path == null || this.Path.Path.Length == 0)
				{
					throw new InvalidOperationException();
				}
				else
				{
					this.Initialize(false);
					if (options == null)
					{
						enumerationOption = new EnumerationOptions();
					}
					else
					{
						enumerationOption = (EnumerationOptions)options.Clone();
					}
					EnumerationOptions enumerationOption1 = enumerationOption;
					enumerationOption1.EnsureLocatable = false;
					enumerationOption1.PrototypeOnly = false;
					enumerationOption1.ReturnImmediately = false;
					if (watcher.HaveListenersForProgress)
					{
						enumerationOption1.SendStatus = true;
					}
					WmiEventSink newSink = watcher.GetNewSink(base.Scope, enumerationOption1.Context);
					SecurityHandler securityHandler = base.Scope.GetSecurityHandler();
					int num = this.scope.GetSecuredIWbemServicesHandler(base.Scope.GetIWbemServices()).CreateInstanceEnumAsync_(base.ClassName, enumerationOption1.Flags, enumerationOption1.GetContext(), newSink.Stub);
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
			}
			else
			{
				throw new ArgumentNullException("watcher");
			}
		}

		internal static ManagementClass GetManagementClass(IWbemClassObjectFreeThreaded wbemObject, ManagementClass mgObj)
		{
			ManagementClass managementClass = new ManagementClass();
			managementClass.wbemObject = wbemObject;
			if (mgObj != null)
			{
				managementClass.scope = ManagementScope._Clone(mgObj.scope);
				ManagementPath path = mgObj.Path;
				if (path != null)
				{
					managementClass.path = ManagementPath._Clone(path);
				}
				object obj = null;
				int num = 0;
				int num1 = wbemObject.Get_("__CLASS", 0, ref obj, ref num, ref num);
				if (num1 < 0)
				{
					if (((long)num1 & (long)-4096) != (long)-2147217408)
					{
						Marshal.ThrowExceptionForHR(num1);
					}
					else
					{
						ManagementException.ThrowWithExtendedInfo((ManagementStatus)num1);
					}
				}
				if (obj != DBNull.Value)
				{
					managementClass.path.internalClassName = (string)obj;
				}
				ObjectGetOptions options = mgObj.Options;
				if (options != null)
				{
					managementClass.options = ObjectGetOptions._Clone(options);
				}
			}
			return managementClass;
		}

		internal static ManagementClass GetManagementClass(IWbemClassObjectFreeThreaded wbemObject, ManagementScope scope)
		{
			ManagementClass managementClass = new ManagementClass();
			managementClass.path = new ManagementPath(ManagementPath.GetManagementPath(wbemObject));
			if (scope != null)
			{
				managementClass.scope = ManagementScope._Clone(scope);
			}
			managementClass.wbemObject = wbemObject;
			return managementClass;
		}

		protected override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ManagementObjectCollection GetRelatedClasses()
		{
			return this.GetRelatedClasses((string)null);
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ManagementObjectCollection GetRelatedClasses(string relatedClass)
		{
			return this.GetRelatedClasses(relatedClass, null, null, null, null, null, null);
		}

		public ManagementObjectCollection GetRelatedClasses(string relatedClass, string relationshipClass, string relationshipQualifier, string relatedQualifier, string relatedRole, string thisRole, EnumerationOptions options)
		{
			EnumerationOptions enumerationOption;
			if (this.Path == null || this.Path.Path == null || this.Path.Path.Length == 0)
			{
				throw new InvalidOperationException();
			}
			else
			{
				this.Initialize(false);
				IEnumWbemClassObject enumWbemClassObject = null;
				if (options != null)
				{
					enumerationOption = (EnumerationOptions)options.Clone();
				}
				else
				{
					enumerationOption = new EnumerationOptions();
				}
				EnumerationOptions enumerationOption1 = enumerationOption;
				enumerationOption1.EnumerateDeep = true;
				RelatedObjectQuery relatedObjectQuery = new RelatedObjectQuery(true, this.Path.Path, relatedClass, relationshipClass, relatedQualifier, relationshipQualifier, relatedRole, thisRole);
				SecurityHandler securityHandler = null;
				int num = 0;
				try
				{
					securityHandler = base.Scope.GetSecurityHandler();
					num = this.scope.GetSecuredIWbemServicesHandler(base.Scope.GetIWbemServices()).ExecQuery_(relatedObjectQuery.QueryLanguage, relatedObjectQuery.QueryString, enumerationOption1.Flags, enumerationOption1.GetContext(), ref enumWbemClassObject);
				}
				finally
				{
					if (securityHandler != null)
					{
						securityHandler.Reset();
					}
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
				return new ManagementObjectCollection(base.Scope, enumerationOption1, enumWbemClassObject);
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void GetRelatedClasses(ManagementOperationObserver watcher)
		{
			this.GetRelatedClasses(watcher, null);
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void GetRelatedClasses(ManagementOperationObserver watcher, string relatedClass)
		{
			this.GetRelatedClasses(watcher, relatedClass, null, null, null, null, null, null);
		}

		public void GetRelatedClasses(ManagementOperationObserver watcher, string relatedClass, string relationshipClass, string relationshipQualifier, string relatedQualifier, string relatedRole, string thisRole, EnumerationOptions options)
		{
			EnumerationOptions enumerationOption;
			if (this.Path == null || this.Path.Path == null || this.Path.Path.Length == 0)
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
					enumerationOption1.EnumerateDeep = true;
					enumerationOption1.ReturnImmediately = false;
					if (watcher.HaveListenersForProgress)
					{
						enumerationOption1.SendStatus = true;
					}
					WmiEventSink newSink = watcher.GetNewSink(base.Scope, enumerationOption1.Context);
					RelatedObjectQuery relatedObjectQuery = new RelatedObjectQuery(true, this.Path.Path, relatedClass, relationshipClass, relatedQualifier, relationshipQualifier, relatedRole, thisRole);
					SecurityHandler securityHandler = base.Scope.GetSecurityHandler();
					int num = this.scope.GetSecuredIWbemServicesHandler(base.Scope.GetIWbemServices()).ExecQueryAsync_(relatedObjectQuery.QueryLanguage, relatedObjectQuery.QueryString, enumerationOption1.Flags, enumerationOption1.GetContext(), newSink.Stub);
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

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ManagementObjectCollection GetRelationshipClasses()
		{
			return this.GetRelationshipClasses((string)null);
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ManagementObjectCollection GetRelationshipClasses(string relationshipClass)
		{
			return this.GetRelationshipClasses(relationshipClass, null, null, null);
		}

		public ManagementObjectCollection GetRelationshipClasses(string relationshipClass, string relationshipQualifier, string thisRole, EnumerationOptions options)
		{
			EnumerationOptions enumerationOption;
			if (this.Path == null || this.Path.Path == null || this.Path.Path.Length == 0)
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
				enumerationOption1.EnumerateDeep = true;
				RelationshipQuery relationshipQuery = new RelationshipQuery(true, this.Path.Path, relationshipClass, relationshipQualifier, thisRole);
				SecurityHandler securityHandler = null;
				int num = 0;
				try
				{
					securityHandler = base.Scope.GetSecurityHandler();
					num = this.scope.GetSecuredIWbemServicesHandler(base.Scope.GetIWbemServices()).ExecQuery_(relationshipQuery.QueryLanguage, relationshipQuery.QueryString, enumerationOption1.Flags, enumerationOption1.GetContext(), ref enumWbemClassObject);
				}
				finally
				{
					if (securityHandler != null)
					{
						securityHandler.Reset();
					}
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
				return new ManagementObjectCollection(base.Scope, enumerationOption1, enumWbemClassObject);
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void GetRelationshipClasses(ManagementOperationObserver watcher)
		{
			this.GetRelationshipClasses(watcher, null);
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void GetRelationshipClasses(ManagementOperationObserver watcher, string relationshipClass)
		{
			this.GetRelationshipClasses(watcher, relationshipClass, null, null, null);
		}

		public void GetRelationshipClasses(ManagementOperationObserver watcher, string relationshipClass, string relationshipQualifier, string thisRole, EnumerationOptions options)
		{
			EnumerationOptions enumerationOption;
			if (this.Path == null || this.Path.Path == null || this.Path.Path.Length == 0)
			{
				throw new InvalidOperationException();
			}
			else
			{
				if (watcher != null)
				{
					this.Initialize(true);
					if (options != null)
					{
						enumerationOption = (EnumerationOptions)options.Clone();
					}
					else
					{
						enumerationOption = new EnumerationOptions();
					}
					EnumerationOptions enumerationOption1 = enumerationOption;
					enumerationOption1.EnumerateDeep = true;
					enumerationOption1.ReturnImmediately = false;
					if (watcher.HaveListenersForProgress)
					{
						enumerationOption1.SendStatus = true;
					}
					WmiEventSink newSink = watcher.GetNewSink(base.Scope, enumerationOption1.Context);
					RelationshipQuery relationshipQuery = new RelationshipQuery(true, this.Path.Path, relationshipClass, relationshipQualifier, thisRole);
					SecurityHandler securityHandler = base.Scope.GetSecurityHandler();
					int num = this.scope.GetSecuredIWbemServicesHandler(base.Scope.GetIWbemServices()).ExecQueryAsync_(relationshipQuery.QueryLanguage, relationshipQuery.QueryString, enumerationOption1.Flags, enumerationOption1.GetContext(), newSink.Stub);
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

		public CodeTypeDeclaration GetStronglyTypedClassCode(bool includeSystemClassInClassDef, bool systemPropertyClass)
		{
			base.Get();
			ManagementClassGenerator managementClassGenerator = new ManagementClassGenerator(this);
			return managementClassGenerator.GenerateCode(includeSystemClassInClassDef, systemPropertyClass);
		}

		public bool GetStronglyTypedClassCode(CodeLanguage lang, string filePath, string classNamespace)
		{
			base.Get();
			ManagementClassGenerator managementClassGenerator = new ManagementClassGenerator(this);
			return managementClassGenerator.GenerateCode(lang, filePath, classNamespace);
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ManagementObjectCollection GetSubclasses()
		{
			return this.GetSubclasses((EnumerationOptions)null);
		}

		public ManagementObjectCollection GetSubclasses(EnumerationOptions options)
		{
			EnumerationOptions enumerationOption;
			if (this.Path != null)
			{
				this.Initialize(false);
				IEnumWbemClassObject enumWbemClassObject = null;
				if (options == null)
				{
					enumerationOption = new EnumerationOptions();
				}
				else
				{
					enumerationOption = (EnumerationOptions)options.Clone();
				}
				EnumerationOptions enumerationOption1 = enumerationOption;
				enumerationOption1.EnsureLocatable = false;
				enumerationOption1.PrototypeOnly = false;
				SecurityHandler securityHandler = null;
				int num = 0;
				try
				{
					securityHandler = base.Scope.GetSecurityHandler();
					num = this.scope.GetSecuredIWbemServicesHandler(base.Scope.GetIWbemServices()).CreateClassEnum_(base.ClassName, enumerationOption1.Flags, enumerationOption1.GetContext(), ref enumWbemClassObject);
				}
				finally
				{
					if (securityHandler != null)
					{
						securityHandler.Reset();
					}
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
				return new ManagementObjectCollection(base.Scope, enumerationOption1, enumWbemClassObject);
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void GetSubclasses(ManagementOperationObserver watcher)
		{
			this.GetSubclasses(watcher, null);
		}

		public void GetSubclasses(ManagementOperationObserver watcher, EnumerationOptions options)
		{
			EnumerationOptions enumerationOption;
			if (watcher != null)
			{
				if (this.Path != null)
				{
					this.Initialize(false);
					if (options == null)
					{
						enumerationOption = new EnumerationOptions();
					}
					else
					{
						enumerationOption = (EnumerationOptions)options.Clone();
					}
					EnumerationOptions enumerationOption1 = enumerationOption;
					enumerationOption1.EnsureLocatable = false;
					enumerationOption1.PrototypeOnly = false;
					enumerationOption1.ReturnImmediately = false;
					if (watcher.HaveListenersForProgress)
					{
						enumerationOption1.SendStatus = true;
					}
					WmiEventSink newSink = watcher.GetNewSink(base.Scope, enumerationOption1.Context);
					SecurityHandler securityHandler = base.Scope.GetSecurityHandler();
					int num = this.scope.GetSecuredIWbemServicesHandler(base.Scope.GetIWbemServices()).CreateClassEnumAsync_(base.ClassName, enumerationOption1.Flags, enumerationOption1.GetContext(), newSink.Stub);
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
					throw new InvalidOperationException();
				}
			}
			else
			{
				throw new ArgumentNullException("watcher");
			}
		}
	}
}