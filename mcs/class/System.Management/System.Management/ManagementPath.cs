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
using System.Runtime;
using System.Runtime.InteropServices;

namespace System.Management
{
	[TypeConverter(typeof(ManagementPathConverter))]
	public class ManagementPath : ICloneable
	{
		private static ManagementPath defaultPath;

		private bool isWbemPathShared;

		private IWbemPath wmiPath;

		[RefreshProperties(RefreshProperties.All)]
		public string ClassName
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.internalClassName;
			}
			set
			{
				string className = this.ClassName;
				if (string.Compare(className, value, StringComparison.OrdinalIgnoreCase) != 0)
				{
					this.internalClassName = value;
					this.FireIdentifierChanged();
				}
			}
		}

		public static ManagementPath DefaultPath
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return ManagementPath.defaultPath;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				ManagementPath.defaultPath = value;
			}
		}

		internal string internalClassName
		{
			get
			{
				string empty = string.Empty;
				if (this.wmiPath != null)
				{
					int num = 0;
					int className_ = this.wmiPath.GetClassName_(out num, null);
					if (className_ >= 0 && 0 < num)
					{
						empty = new string('0', num - 1);
						className_ = this.wmiPath.GetClassName_(out num, empty);
						if (className_ < 0)
						{
							empty = string.Empty;
						}
					}
				}
				return empty;
			}
			set
			{
				int num = 0;
				if (this.wmiPath != null)
				{
					if (this.isWbemPathShared)
					{
						this.wmiPath = this.CreateWbemPath(this.GetWbemPath());
						this.isWbemPathShared = false;
					}
				}
				else
				{
					this.wmiPath = (IWbemPath)MTAHelper.CreateInMTA(typeof(WbemDefPath));
				}
				try
				{
					num = this.wmiPath.SetClassName_(value);
				}
				catch (COMException cOMException)
				{
					throw new ArgumentOutOfRangeException("value");
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
						return;
					}
				}
			}
		}

		public bool IsClass
		{
			get
			{
				if (this.wmiPath != null)
				{
					ulong num = (long)0;
					int info_ = this.wmiPath.GetInfo_(0, out num);
					if (info_ < 0)
					{
						if (((long)info_ & (long)-4096) != (long)-2147217408)
						{
							Marshal.ThrowExceptionForHR(info_);
						}
						else
						{
							ManagementException.ThrowWithExtendedInfo((ManagementStatus)info_);
						}
					}
					return (long)0 != (num & (long)4);
				}
				else
				{
					return false;
				}
			}
		}

		internal bool IsEmpty
		{
			get
			{
				return this.Path.Length == 0;
			}
		}

		public bool IsInstance
		{
			get
			{
				if (this.wmiPath != null)
				{
					ulong num = (long)0;
					int info_ = this.wmiPath.GetInfo_(0, out num);
					if (info_ < 0)
					{
						if (((long)info_ & (long)-4096) != (long)-2147217408)
						{
							Marshal.ThrowExceptionForHR(info_);
						}
						else
						{
							ManagementException.ThrowWithExtendedInfo((ManagementStatus)info_);
						}
					}
					return (long)0 != (num & (long)8);
				}
				else
				{
					return false;
				}
			}
		}

		public bool IsSingleton
		{
			get
			{
				if (this.wmiPath != null)
				{
					ulong num = (long)0;
					int info_ = this.wmiPath.GetInfo_(0, out num);
					if (info_ < 0)
					{
						if (((long)info_ & (long)-4096) != (long)-2147217408)
						{
							Marshal.ThrowExceptionForHR(info_);
						}
						else
						{
							ManagementException.ThrowWithExtendedInfo((ManagementStatus)info_);
						}
					}
					return (long)0 != (num & (long)0x1000);
				}
				else
				{
					return false;
				}
			}
		}

		[RefreshProperties(RefreshProperties.All)]
		public string NamespacePath
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.GetNamespacePath(16);
			}
			set
			{
				bool flag = false;
				try
				{
					this.SetNamespacePath(value, out flag);
				}
				catch (COMException cOMException)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				if (flag)
				{
					this.FireIdentifierChanged();
				}
			}
		}

		[RefreshProperties(RefreshProperties.All)]
		public string Path
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.GetWbemPath();
			}
			set
			{
				try
				{
					if (this.isWbemPathShared)
					{
						this.wmiPath = this.CreateWbemPath(this.GetWbemPath());
						this.isWbemPathShared = false;
					}
					this.SetWbemPath(value);
				}
				catch
				{
					throw new ArgumentOutOfRangeException("value");
				}
				this.FireIdentifierChanged();
			}
		}

		[RefreshProperties(RefreshProperties.All)]
		public string RelativePath
		{
			get
			{
				string empty = string.Empty;
				if (this.wmiPath != null)
				{
					int num = 0;
					int text_ = this.wmiPath.GetText_(2, out num, null);
					if (text_ >= 0 && 0 < num)
					{
						empty = new string('0', num - 1);
						text_ = this.wmiPath.GetText_(2, out num, empty);
					}
					if (text_ < 0 && text_ != -2147217400)
					{
						if (((long)text_ & (long)-4096) != (long)-2147217408)
						{
							Marshal.ThrowExceptionForHR(text_);
						}
						else
						{
							ManagementException.ThrowWithExtendedInfo((ManagementStatus)text_);
						}
					}
				}
				return empty;
			}
			set
			{
				try
				{
					this.SetRelativePath(value);
				}
				catch (COMException cOMException)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				this.FireIdentifierChanged();
			}
		}

		[RefreshProperties(RefreshProperties.All)]
		public string Server
		{
			get
			{
				string empty = string.Empty;
				if (this.wmiPath != null)
				{
					int num = 0;
					int server_ = this.wmiPath.GetServer_(out num, null);
					if (server_ >= 0 && 0 < num)
					{
						empty = new string('0', num - 1);
						server_ = this.wmiPath.GetServer_(out num, empty);
					}
					if (server_ < 0 && server_ != -2147217399)
					{
						if (((long)server_ & (long)-4096) != (long)-2147217408)
						{
							Marshal.ThrowExceptionForHR(server_);
						}
						else
						{
							ManagementException.ThrowWithExtendedInfo((ManagementStatus)server_);
						}
					}
				}
				return empty;
			}
			set
			{
				string server = this.Server;
				if (string.Compare(server, value, StringComparison.OrdinalIgnoreCase) != 0)
				{
					if (this.wmiPath != null)
					{
						if (this.isWbemPathShared)
						{
							this.wmiPath = this.CreateWbemPath(this.GetWbemPath());
							this.isWbemPathShared = false;
						}
					}
					else
					{
						this.wmiPath = (IWbemPath)MTAHelper.CreateInMTA(typeof(WbemDefPath));
					}
					int num = this.wmiPath.SetServer_(value);
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
					this.FireIdentifierChanged();
				}
			}
		}

		static ManagementPath()
		{
			ManagementPath.defaultPath = new ManagementPath("//./root/cimv2");
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ManagementPath() : this(null)
		{
		}

		public ManagementPath(string path)
		{
			if (path != null && 0 < path.Length)
			{
				this.wmiPath = this.CreateWbemPath(path);
			}
		}

		internal static ManagementPath _Clone(ManagementPath path)
		{
			return ManagementPath._Clone(path, null);
		}

		internal static ManagementPath _Clone(ManagementPath path, IdentifierChangedEventHandler handler)
		{
			ManagementPath managementPath = new ManagementPath();
			if (handler != null)
			{
				managementPath.IdentifierChanged = handler;
			}
			if (path != null && path.wmiPath != null)
			{
				managementPath.wmiPath = path.wmiPath;
				bool flag = true;
				bool flag1 = flag;
				path.isWbemPathShared = flag;
				managementPath.isWbemPathShared = flag1;
			}
			return managementPath;
		}

		private void ClearKeys(bool setAsSingleton)
		{
			sbyte num;
			int keyList_ = 0;
			try
			{
				if (this.wmiPath != null)
				{
					IWbemPathKeyList wbemPathKeyList = null;
					keyList_ = this.wmiPath.GetKeyList_(out wbemPathKeyList);
					if (wbemPathKeyList != null)
					{
						keyList_ = wbemPathKeyList.RemoveAllKeys_(0);
						if (((long)keyList_ & (long)-2147483648) == (long)0)
						{
							if (setAsSingleton)
							{
								num = -1;
							}
							else
							{
								num = 0;
							}
							sbyte num1 = num;
							keyList_ = wbemPathKeyList.MakeSingleton_(num1);
							this.FireIdentifierChanged();
						}
					}
				}
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				ManagementException.ThrowWithExtendedInfo(cOMException);
			}
			if (((long)keyList_ & (long)-4096) != (long)-2147217408)
			{
				if (((long)keyList_ & (long)-2147483648) != (long)0)
				{
					Marshal.ThrowExceptionForHR(keyList_);
				}
				return;
			}
			else
			{
				ManagementException.ThrowWithExtendedInfo((ManagementStatus)keyList_);
				return;
			}
		}

		public ManagementPath Clone()
		{
			return new ManagementPath(this.Path);
		}

		private IWbemPath CreateWbemPath(string path)
		{
			IWbemPath wbemPath = (IWbemPath)MTAHelper.CreateInMTA(typeof(WbemDefPath));
			ManagementPath.SetWbemPath(wbemPath, path);
			return wbemPath;
		}

		private void FireIdentifierChanged()
		{
			if (this.IdentifierChanged != null)
			{
				this.IdentifierChanged(this, null);
			}
		}

		internal static string GetManagementPath(IWbemClassObjectFreeThreaded wbemObject)
		{
			string str = null;
			if (wbemObject != null)
			{
				int num = 0;
				int num1 = 0;
				object obj = null;
				int num2 = wbemObject.Get_("__PATH", 0, ref obj, ref num, ref num1);
				if (num2 < 0 || obj == DBNull.Value)
				{
					num2 = wbemObject.Get_("__RELPATH", 0, ref obj, ref num, ref num1);
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
				}
				if (DBNull.Value != obj)
				{
					str = (string)obj;
				}
				else
				{
					str = null;
				}
			}
			return str;
		}

		internal string GetNamespacePath(int flags)
		{
			return ManagementPath.GetNamespacePath(this.wmiPath, flags);
		}

		internal static string GetNamespacePath(IWbemPath wbemPath, int flags)
		{
			string empty = string.Empty;
			if (wbemPath != null)
			{
				uint num = 0;
				int namespaceCount_ = wbemPath.GetNamespaceCount_(out num);
				if (namespaceCount_ >= 0 && num > 0)
				{
					int num1 = 0;
					namespaceCount_ = wbemPath.GetText_(flags, out num1, null);
					if (namespaceCount_ >= 0 && num1 > 0)
					{
						empty = new string('0', num1 - 1);
						namespaceCount_ = wbemPath.GetText_(flags, out num1, empty);
					}
				}
				if (namespaceCount_ < 0 && namespaceCount_ != -2147217400)
				{
					if (((long)namespaceCount_ & (long)-4096) != (long)-2147217408)
					{
						Marshal.ThrowExceptionForHR(namespaceCount_);
					}
					else
					{
						ManagementException.ThrowWithExtendedInfo((ManagementStatus)namespaceCount_);
					}
				}
			}
			return empty;
		}

		private string GetWbemPath()
		{
			return ManagementPath.GetWbemPath(this.wmiPath);
		}

		private static string GetWbemPath(IWbemPath wbemPath)
		{
			string empty = string.Empty;
			if (wbemPath != null)
			{
				int num = 4;
				uint num1 = 0;
				int namespaceCount_ = wbemPath.GetNamespaceCount_(out num1);
				if (namespaceCount_ >= 0)
				{
					if (num1 == 0)
					{
						num = 2;
					}
					int num2 = 0;
					namespaceCount_ = wbemPath.GetText_(num, out num2, null);
					if (namespaceCount_ >= 0 && 0 < num2)
					{
						empty = new string('0', num2 - 1);
						namespaceCount_ = wbemPath.GetText_(num, out num2, empty);
					}
				}
				if (namespaceCount_ < 0 && namespaceCount_ != -2147217400)
				{
					if (((long)namespaceCount_ & (long)-4096) != (long)-2147217408)
					{
						Marshal.ThrowExceptionForHR(namespaceCount_);
					}
					else
					{
						ManagementException.ThrowWithExtendedInfo((ManagementStatus)namespaceCount_);
					}
				}
			}
			return empty;
		}

		internal static bool IsValidNamespaceSyntax(string nsPath)
		{
			if (nsPath.Length != 0)
			{
				char[] chrArray = new char[2];
				chrArray[0] = '\\';
				chrArray[1] = '/';
				char[] chrArray1 = chrArray;
				if (nsPath.IndexOfAny(chrArray1) == -1 && string.Compare("root", nsPath, StringComparison.OrdinalIgnoreCase) != 0)
				{
					return false;
				}
			}
			return true;
		}

		public void SetAsClass()
		{
			if (this.IsClass || this.IsInstance)
			{
				if (this.isWbemPathShared)
				{
					this.wmiPath = this.CreateWbemPath(this.GetWbemPath());
					this.isWbemPathShared = false;
				}
				this.ClearKeys(false);
				return;
			}
			else
			{
				throw new ManagementException(ManagementStatus.InvalidOperation, null, null);
			}
		}

		public void SetAsSingleton()
		{
			if (this.IsClass || this.IsInstance)
			{
				if (this.isWbemPathShared)
				{
					this.wmiPath = this.CreateWbemPath(this.GetWbemPath());
					this.isWbemPathShared = false;
				}
				this.ClearKeys(true);
				return;
			}
			else
			{
				throw new ManagementException(ManagementStatus.InvalidOperation, null, null);
			}
		}

		internal string SetNamespacePath (string nsPath, out bool bChange)
		{
			int namespaceCount_ = 0;
			bChange = false;
			if (!ManagementPath.IsValidNamespaceSyntax (nsPath)) {
				ManagementException.ThrowWithExtendedInfo (ManagementStatus.InvalidNamespace);
			}
			IWbemPath wbemPath = this.CreateWbemPath (nsPath);
			if (this.wmiPath != null) {
				if (this.isWbemPathShared) {
					this.wmiPath = this.CreateWbemPath (this.GetWbemPath ());
					this.isWbemPathShared = false;
				}
			} else {
				this.wmiPath = this.CreateWbemPath ("");
			}
			string namespacePath = ManagementPath.GetNamespacePath (this.wmiPath, 16);
			string str = ManagementPath.GetNamespacePath (wbemPath, 16);
			if (string.Compare (namespacePath, str, StringComparison.OrdinalIgnoreCase) != 0) {
				this.wmiPath.RemoveAllNamespaces_ ();
				bChange = true;
				uint num = 0;
				namespaceCount_ = wbemPath.GetNamespaceCount_ (out num);
				if (namespaceCount_ >= 0) {
					for (uint i = 0; i < num; i++) {
						int num1 = 0;
						namespaceCount_ = wbemPath.GetNamespaceAt_ (i, out num1, null);
						if (namespaceCount_ < 0) {
							break;
						}
						string str1 = new string ('0', num1 - 1);
						namespaceCount_ = wbemPath.GetNamespaceAt_ (i, out num1, str1);
						if (namespaceCount_ < 0) {
							break;
						}
						namespaceCount_ = this.wmiPath.SetNamespaceAt_ (i, str1);
						if (namespaceCount_ < 0) {
							break;
						}
					}
				}
			}
			if (namespaceCount_ >= 0 && nsPath.Length > 1 && (nsPath [0] == '\\' && nsPath [1] == '\\' || nsPath [0] == '/' && nsPath [1] == '/')) {
				int num2 = 0;
				namespaceCount_ = wbemPath.GetServer_ (out num2, null);
				if (namespaceCount_ < 0 || num2 <= 0) {
					if (namespaceCount_ == -2147217399) {
						namespaceCount_ = 0;
					}
				} else {
					string str2 = new string ('0', num2 - 1);
					namespaceCount_ = wbemPath.GetServer_ (out num2, str2);
					if (namespaceCount_ >= 0) {
						num2 = 0;
						namespaceCount_ = this.wmiPath.GetServer_ (out num2, null);
						if (namespaceCount_ < 0) {
							if (namespaceCount_ == -2147217399) {
								namespaceCount_ = this.wmiPath.SetServer_ (str2);
								if (namespaceCount_ >= 0) {
									bChange = true;
								}
							}
						} else {
							string str3 = new string ('0', num2 - 1);
							namespaceCount_ = this.wmiPath.GetServer_ (out num2, str3);
							if (namespaceCount_ >= 0 && string.Compare (str3, str2, StringComparison.OrdinalIgnoreCase) != 0) {
								namespaceCount_ = this.wmiPath.SetServer_ (str2);
							}
						}
					}
				}
			} else if (namespaceCount_ <= 0 && nsPath.Length > 1) {
				this.wmiPath.SetNamespaceAt_ (0, nsPath);
				uint nsCount = 0;
				namespaceCount_ = (int)nsCount;
				this.wmiPath.GetNamespaceCount_ (out nsCount);
				str = nsPath;
			}

			if (namespaceCount_ < 0)
			{
				if (((long)namespaceCount_ & (long)-4096) != (long)-2147217408)
				{
					Marshal.ThrowExceptionForHR(namespaceCount_);
				}
				else
				{
					ManagementException.ThrowWithExtendedInfo((ManagementStatus)namespaceCount_);
				}
			}
			return str;
		}

		internal void SetRelativePath(string relPath)
		{
			ManagementPath managementPath = new ManagementPath(relPath);
			managementPath.NamespacePath = this.GetNamespacePath(8);
			managementPath.Server = this.Server;
			this.wmiPath = managementPath.wmiPath;
		}

		private void SetWbemPath(string path)
		{
			if (this.wmiPath != null)
			{
				ManagementPath.SetWbemPath(this.wmiPath, path);
				return;
			}
			else
			{
				this.wmiPath = this.CreateWbemPath(path);
				return;
			}
		}

		private static void SetWbemPath(IWbemPath wbemPath, string path)
		{
			if (wbemPath != null)
			{
				uint num = 4;
				if (string.Compare(path, "root", StringComparison.OrdinalIgnoreCase) == 0)
				{
					num = num | 8;
				}
				int num1 = wbemPath.SetText_(num, path);
				if (num1 < 0)
				{
					if (((long)num1 & (long)-4096) != (long)-2147217408)
					{
						Marshal.ThrowExceptionForHR(num1);
					}
					else
					{
						ManagementException.ThrowWithExtendedInfo((ManagementStatus)num1);
						return;
					}
				}
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		object System.ICloneable.Clone()
		{
			return this.Clone();
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public override string ToString()
		{
			return this.Path;
		}

		internal void UpdateRelativePath(string relPath)
		{
			string str;
			if (relPath != null)
			{
				string namespacePath = this.GetNamespacePath(8);
				if (namespacePath.Length <= 0)
				{
					str = relPath;
				}
				else
				{
					str = string.Concat(namespacePath, ":", relPath);
				}
				if (this.isWbemPathShared)
				{
					this.wmiPath = this.CreateWbemPath(this.GetWbemPath());
					this.isWbemPathShared = false;
				}
				this.SetWbemPath(str);
				return;
			}
			else
			{
				return;
			}
		}

		internal event IdentifierChangedEventHandler IdentifierChanged;
	}
}