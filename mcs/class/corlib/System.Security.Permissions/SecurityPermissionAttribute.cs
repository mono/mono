// 
// System.Security.Permissions.SecurityPermissionAttribute.cs 
//
// Author:         Nick Drochak, ndrochak@gol.com
// Created:        2002-01-06 
//
// Copyright (C) 2001 Nick Drochak, All Rights Reserved
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

namespace System.Security.Permissions {

#if MOBILE && !MONOTOUCH
	[Obsolete ("CAS support is not available with Silverlight applications.")]
#endif
	[ComVisible (true)]
	[AttributeUsage (
		System.AttributeTargets.Assembly
		| System.AttributeTargets.Class 
		| System.AttributeTargets.Struct 
		| System.AttributeTargets.Constructor 
		| System.AttributeTargets.Method, 
		AllowMultiple=true, 
		Inherited=false)
	]
	[Serializable]
	public sealed class SecurityPermissionAttribute : CodeAccessSecurityAttribute {
		private SecurityPermissionFlag m_Flags;

		public SecurityPermissionAttribute (SecurityAction action) : base(action) 
		{
			m_Flags = SecurityPermissionFlag.NoFlags;
		}

		public bool Assertion {
			get {
				return ((m_Flags & SecurityPermissionFlag.Assertion) != 0);
			}
			set {
				if (value) {
					m_Flags |= SecurityPermissionFlag.Assertion;
				}
				else{
					m_Flags &= ~SecurityPermissionFlag.Assertion;
				}
			}
		}
		public bool BindingRedirects {
			get {
				return ((m_Flags & SecurityPermissionFlag.BindingRedirects) != 0);
			}
			set {
				if (value) {
					m_Flags |= SecurityPermissionFlag.BindingRedirects;
				}
				else{
					m_Flags &= ~SecurityPermissionFlag.BindingRedirects;
				}
			}
		}

		public bool ControlAppDomain {
			get {
				return ((m_Flags & SecurityPermissionFlag.ControlAppDomain) != 0);
			}
			set {
				if (value) {
					m_Flags |= SecurityPermissionFlag.ControlAppDomain;
				}
				else {
					m_Flags &= ~SecurityPermissionFlag.ControlAppDomain;
				}
			}
		}

		public bool ControlDomainPolicy {
			get {
				return ((m_Flags & SecurityPermissionFlag.ControlDomainPolicy) != 0);
			}
			set {
				if (value) {
					m_Flags |= SecurityPermissionFlag.ControlDomainPolicy;
				}
				else {
					m_Flags &= ~SecurityPermissionFlag.ControlDomainPolicy;
				}
			}
		}

		public bool ControlEvidence {
			get {
				return ((m_Flags & SecurityPermissionFlag.ControlEvidence) != 0);
			}
			set {
				if (value) {
					m_Flags |= SecurityPermissionFlag.ControlEvidence;
				}
				else {
					m_Flags &= ~SecurityPermissionFlag.ControlEvidence;
				}
			}
		}
		
		public bool ControlPolicy {
			get {
				return ((m_Flags & SecurityPermissionFlag.ControlPolicy) != 0);
			}
			set {
				if (value) {
					m_Flags |= SecurityPermissionFlag.ControlPolicy;
				}
				else {
					m_Flags &= ~SecurityPermissionFlag.ControlPolicy;
				}
			}
		}
		
		public bool ControlPrincipal {
			get {
				return ((m_Flags & SecurityPermissionFlag.ControlPrincipal) != 0);
			}
			set {
				if (value) {
					m_Flags |= SecurityPermissionFlag.ControlPrincipal;
				}
				else {
					m_Flags &= ~SecurityPermissionFlag.ControlPrincipal;
				}
			}
		}

		public bool ControlThread {
			get {
				return ((m_Flags & SecurityPermissionFlag.ControlThread) != 0);
			}
			set {
				if (value) {
					m_Flags |= SecurityPermissionFlag.ControlThread;
				}
				else {
					m_Flags &= ~SecurityPermissionFlag.ControlThread;
				}
			}
		}

		public bool Execution {
			get {
				return ((m_Flags & SecurityPermissionFlag.Execution) != 0);
			}
			set {
				if (value) {
					m_Flags |= SecurityPermissionFlag.Execution;
				}
				else {
					m_Flags &= ~SecurityPermissionFlag.Execution;
				}
			}
		}

		[ComVisible (true)]
		public bool Infrastructure {
			get {
				return ((m_Flags & SecurityPermissionFlag.Infrastructure) != 0);
			}
			set {
				if (value) {
					m_Flags |= SecurityPermissionFlag.Infrastructure;
				}
				else {
					m_Flags &= ~SecurityPermissionFlag.Infrastructure;
				}
			}
		}

		public bool RemotingConfiguration {
			get {
				return ((m_Flags & SecurityPermissionFlag.RemotingConfiguration) != 0);
			}
			set {
				if (value) {
					m_Flags |= SecurityPermissionFlag.RemotingConfiguration;
				}
				else {
					m_Flags &= ~SecurityPermissionFlag.RemotingConfiguration;
				}
			}
		}
		
		public bool SerializationFormatter {
			get {
				return ((m_Flags & SecurityPermissionFlag.SerializationFormatter) != 0);
			}
			set {
				if (value) {
					m_Flags |= SecurityPermissionFlag.SerializationFormatter;
				}
				else {
					m_Flags &= ~SecurityPermissionFlag.SerializationFormatter;
				}
			}
		}
		
		public bool SkipVerification {
			get {
				return ((m_Flags & SecurityPermissionFlag.SkipVerification) != 0);
			}
			set {
				if (value) {
					m_Flags |= SecurityPermissionFlag.SkipVerification;
				}
				else {
					m_Flags &= ~SecurityPermissionFlag.SkipVerification;
				}
			}
		}

		public bool UnmanagedCode {
			get {
				return ((m_Flags & SecurityPermissionFlag.UnmanagedCode) != 0);
			}
			set {
				if (value) {
					m_Flags |= SecurityPermissionFlag.UnmanagedCode;
				}
				else {
					m_Flags &= ~SecurityPermissionFlag.UnmanagedCode;
				}
			}
		}

		public override IPermission CreatePermission ()
		{
#if MOBILE
			return null;
#else
			SecurityPermission perm = null;
			if (this.Unrestricted)
				perm = new SecurityPermission (PermissionState.Unrestricted);
			else
				perm = new SecurityPermission (m_Flags);
			return perm;
#endif
		}

		public SecurityPermissionFlag Flags {
			get {
				return m_Flags;
			}
			set {
				m_Flags = value;
			}
		}
	}  // public sealed class SecurityPermissionAttribute 
}  // namespace System.Security.Permissions
