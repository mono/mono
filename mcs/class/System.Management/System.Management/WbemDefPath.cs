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
using System.Runtime.InteropServices;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace System.Management
{
	[ClassInterface((short)0)]
	[Guid("CF4CC405-E2C5-4DDD-B3CE-5E7582D8C9FA")]
	[TypeLibType(0x202)]
	internal class WbemDefPath : IWbemPath
	{

		private string _serverName;
		private string _className;
		private string[] _nameSpace = new string[128];
		private string[] _scopes = new string[128];
		private Dictionary<int, string> _texts = new Dictionary<int, string>();

		public WbemDefPath ()
		{

		}

		#region IWbemPath implementation

		public int CreateClassPart_ (int lFlags, string Name)
		{
			return 0;
		}

		public int DeleteClassPart_ (int lFlags)
		{
			return 0;
		}

		public int GetClassName_ (out int puBuffLength, string pszName)
		{
			if (pszName == null) {
				puBuffLength = string.IsNullOrEmpty (_className) ? 0 : _className.Length;
			}
			else {
				ParseString(pszName, _className);
				puBuffLength = string.IsNullOrEmpty (_className) ? 0 : _className.Length;
			}
			return 0;
		}

		private static void ParseString(string target, string source)
		{
			MethodInfo method = typeof(string).GetMethods (BindingFlags.NonPublic | BindingFlags.Static).FirstOrDefault(x => x.Name == "CharCopy" && x.GetParameters ().Length == 5 && x.GetParameters ().ElementAt (2).ParameterType == typeof(char[]));
			method.Invoke (null, new object[] { target, 0, source.ToCharArray (), 0, source.Length });
		}

		public int GetInfo_ (uint uRequestedInfo, out ulong puResponse)
		{
			puResponse = 0;
			if (!string.IsNullOrEmpty (this._className))
			{
				puResponse = 4;
			}
			else if (_texts.ContainsKey (2)) {
				string str = _texts[2];
				if (!string.IsNullOrEmpty (str))
				{
					if (!str.Contains ("="))
					{
						puResponse = 4;
					}
				}
			}
			return 0;
		}

		public int GetKeyList_ (out IWbemPathKeyList pOut)
		{
			pOut = null;
			return 0;
		}

		public int GetNamespaceAt_ (uint uIndex, out int puNameBufLength, string pName)
		{
			string val = this._nameSpace [(int)uIndex];
			if (string.IsNullOrEmpty (val) && _texts.ContainsKey(16)) {
				val = _texts[16];
			}
			else if (string.IsNullOrEmpty (val) && _texts.ContainsKey(8)) {
				val = _texts[8];
			}
			if (pName == null)
				puNameBufLength = val == null ? 0 : val.Length;
			else {
				ParseString(pName, val);
				puNameBufLength = pName == null ? 0 : pName.Length;
			}
			return 0;
		}

		public int GetNamespaceCount_ (out uint puCount)
		{
			uint count = 0;
			for (var i = 0; i < this._nameSpace.Length; i++) {
				if (this._nameSpace [i] != null)
					count++;
			}
			puCount = count;
			if (puCount == 0) {
				if (_texts.ContainsKey(16) || _texts.ContainsKey(8)) {
					puCount = 1;
				}
			}
			return 0;
		}

		public int GetScope_ (uint uIndex, out uint puClassNameBufSize, string pszClass, out IWbemPathKeyList pKeyList)
		{
			pKeyList = null;
			puClassNameBufSize = Convert.ToUInt32(pszClass.Length);
			return 0;
		}

		public int GetScopeAsText_ (uint uIndex, out uint puTextBufSize, string pszText)
		{
			puTextBufSize = Convert.ToUInt32(pszText.Length);
			return 0;
		}

		public int GetScopeCount_ (out uint puCount)
		{
			uint count = 0;
			for (var i = 0; i < this._scopes.Length; i++) {
				if (this._scopes[i] != null) count++;
			}
			puCount = count;
			return 0;
		}

		public int GetServer_ (out int puNameBufLength, string pName)
		{
			if (pName == null)
				puNameBufLength = (_serverName == null ? 0 : _serverName.Length);
			else {
				ParseString(pName, _serverName);
				puNameBufLength = (pName == null ? 0 : pName.Length);
			}
			uint namespaceCount_ = 0;
			GetNamespaceCount_ (out namespaceCount_);
			return (int)namespaceCount_;
		}

		public int GetText_ (int lFlags, out int puBuffLength, string pszText)
		{
			string val = null;
			if (_texts.ContainsKey ((int)lFlags)) {
				val = _texts[(int)lFlags];
			} else {
				val = null;
			}
			if (pszText != null && val != null) ParseString (pszText, val);
			puBuffLength = (val == null ? 0 : val.Length);
			return 0;
		}

		public int IsLocal_ (string wszMachine)
		{
			return 0;
		}

		public int IsRelative_ (string wszMachine, string wszNamespace)
		{
			return 0;
		}

		public int IsRelativeOrChild_ (string wszMachine, string wszNamespace, int lFlags)
		{
			return 0;
		}

		public int IsSameClassName_ (string wszClass)
		{
			bool isSame = _className.Equals (wszClass, StringComparison.OrdinalIgnoreCase);
			return isSame ? 1 : 0;
		}

		public int RemoveAllNamespaces_ ()
		{
			for(var i = 0; i < this._nameSpace.Length; i++)
			{
				this._nameSpace[i] = null;
			}
			return 0;
		}

		public int RemoveAllScopes_ ()
		{
			for(var i = 0; i < this._scopes.Length; i++)
			{
				this._scopes[i] = null;
			}
			return 0;
		}

		public int RemoveNamespaceAt_ (uint uIndex)
		{
			this._nameSpace[(int)uIndex] = null;
			return 0;
		}

		public int RemoveScope_ (uint uIndex)
		{
			this._scopes[(int)uIndex] = null;
			return 0;
		}

		public int SetClassName_ (string Name)
		{
			_className = Name + char.MinValue;
			return 0;
		}

		public int SetNamespaceAt_ (uint uIndex, string pszName)
		{
			_nameSpace[(int)uIndex] = pszName + char.MinValue;
			SetText_ (8, pszName);
			SetText_ (16, pszName);
			return 0;
		}

		public int SetScope_ (uint uIndex, string pszClass)
		{
			return 0;
		}

		public int SetScopeFromText_ (uint uIndex, string pszText)
		{
			return 0;
		}

		public int SetServer_ (string Name)
		{
			_serverName = Name + char.MinValue;
			return 0;
		}

		public int SetText_ (uint uMode, string pszPath)
		{
			if (string.IsNullOrEmpty (pszPath)) return 0;
			pszPath = pszPath.Replace ("\\", "/");
			if (pszPath.StartsWith ("///"))
			{
				pszPath = pszPath.Substring (1);
			}
			if (_texts.ContainsKey ((int)uMode)) {
				_texts[(int)uMode] = pszPath  + char.MinValue;
			} else {
				_texts.Add ((int)uMode, pszPath + char.MinValue);
			}
			if (pszPath != null && uMode == 4) {
				if (pszPath.Length == 0)
				{
					//SetText_ (2, string.Empty);
					//SetText_ (8, string.Empty);
					//SetText_ (16, string.Empty);
				}
				else {
					string workingPath = pszPath;
					string value = "";
					int valueIndex = workingPath.IndexOf ("="); 
					if (valueIndex > 0)
					{
						value = workingPath.Substring (valueIndex + 1);
						workingPath = workingPath.Substring (0, valueIndex);
					}
					bool withServer = false;
					if (workingPath.StartsWith ("//"))
					{
						withServer = true;
						workingPath = workingPath.Substring (2);
					}
					string[] data = workingPath.Split(new char[] { '/' });
					if (data.Length == 0)
					{
						//SetText_ (2, string.Empty);
						//SetText_ (8, string.Empty);
						//SetText_ (16, string.Empty);
					}
					else {
						string serverName = data[0];
						if (withServer) _serverName = serverName;
						string className = null;
						string path = data[data.Length - 1];
						if (valueIndex <= 0)
						{
							className = path;
							path = "";
						}
						else {
							className = data[data.Length - 2];
						}
						int nsCount = string.IsNullOrEmpty (path) ? data.Length - 1 : data.Length - 2;
						string nameSpace = withServer ? "" : serverName + "/";
						for(int i = 1; i < nsCount; i++)
						{
							if (i > 1) nameSpace += "/";
							nameSpace += data[i];
						}
						string relPath = nameSpace + (nameSpace.EndsWith("/") ? "" : "/") + className;
						if (!string.IsNullOrEmpty (path))
						{
							relPath += "/" + path;
						}
						if (!nameSpace.Contains ("/") || nameSpace.EndsWith ("/"))
						{
							nameSpace = relPath;
							className = string.Empty;
						}
						if (!string.IsNullOrEmpty (value))
						{
							relPath += "=" + value;
						}
						//Set Mode 2 Relative
						//Set Mode 8 Namespace //TODO: Rewview (Root Namespace)
						//Set Mode 16 Namespace 
						SetText_ (2, relPath);
						SetText_ (8, nameSpace);
						SetText_ (16, nameSpace);
					}
				}
			}


			return 0;
		}

		#endregion
	}
}