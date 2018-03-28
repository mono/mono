//
// CardSpaceSelector.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005-2007 Novell, Inc.  http://www.novell.com
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens;
using System.Reflection;
using System.Xml;

namespace System.IdentityModel.Selectors
{
	public static class CardSpaceSelector
	{
		static readonly Type impl_type;
		static readonly object impl;
		static readonly MethodInfo get_token, import, manage;

		static CardSpaceSelector ()
		{
			string implName;
			switch (Environment.GetEnvironmentVariable ("MONO_IDENTITY_SELECTOR_TYPE")) {
			default:
				implName = "Mono.ServiceModel.IdentitySelectors.Win32.CardSelectorClientWin32, Mono.ServiceModel.IdentitySelectors,  Version=3.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756";
				break;
			}
			impl_type = Type.GetType (implName);
			impl = Activator.CreateInstance (impl_type, new object [0]);
			get_token = impl_type.GetMethod ("GetToken", new Type [] {
				typeof (CardSpacePolicyElement []),
				typeof (SecurityTokenSerializer)});
			import = impl_type.GetMethod ("Import", new Type [] {
				typeof (string)});
			manage = impl_type.GetMethod ("Manage", new Type [0]);
		}

		[MonoTODO]
		public static GenericXmlSecurityToken GetToken (
			CardSpacePolicyElement [] policyChain,
			SecurityTokenSerializer tokenSerializer)
		{
			return (GenericXmlSecurityToken) get_token.Invoke (impl, new object [] {policyChain, tokenSerializer});
		}

		public static GenericXmlSecurityToken GetToken (
			XmlElement endpoint,
			IEnumerable<XmlElement> policy,
			XmlElement requiredRemoteTokenIssuer,
			SecurityTokenSerializer tokenSerializer)
		{
			CardSpacePolicyElement pe = new CardSpacePolicyElement (endpoint, requiredRemoteTokenIssuer, new Collection<XmlElement> (new List<XmlElement> (policy)), null, 0, requiredRemoteTokenIssuer != null);
			return GetToken (new CardSpacePolicyElement [] {pe}, tokenSerializer);
		}

		[MonoTODO]
		public static void Import (string fileName)
		{
			import.Invoke (impl, new object [] {fileName});
		}

		[MonoTODO]
		public static void Manage ()
		{
			manage.Invoke (impl, new object [0]);
		}
	}
}
