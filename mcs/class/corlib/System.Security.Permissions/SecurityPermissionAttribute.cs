//------------------------------------------------------------------------------
// 
// System.Security.Permissions.SecurityPermissionAttribute.cs 
//
// Copyright (C) 2001 Nick Drochak, All Rights Reserved
// 
// Author:         Nick Drochak, ndrochak@gol.com
// Created:        2002-01-06 
//
//------------------------------------------------------------------------------

using System;
using System.Security;

namespace System.Security.Permissions {
	[System.AttributeUsage(
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
					m_Flags &= SecurityPermissionFlag.Assertion;
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
					m_Flags &= SecurityPermissionFlag.ControlAppDomain;
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
					m_Flags &= SecurityPermissionFlag.ControlDomainPolicy;
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
					m_Flags &= SecurityPermissionFlag.ControlEvidence;
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
					m_Flags &= SecurityPermissionFlag.ControlPolicy;
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
					m_Flags &= SecurityPermissionFlag.ControlPrincipal;
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
					m_Flags &= SecurityPermissionFlag.ControlThread;
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
					m_Flags &= SecurityPermissionFlag.Execution;
				}
			}
		}

		public bool Infrastructure {
			get {
				return ((m_Flags & SecurityPermissionFlag.Infrastructure) != 0);
			}
			set {
				if (value) {
					m_Flags |= SecurityPermissionFlag.Infrastructure;
				}
				else {
					m_Flags &= SecurityPermissionFlag.Infrastructure;
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
					m_Flags &= SecurityPermissionFlag.RemotingConfiguration;
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
					m_Flags &= SecurityPermissionFlag.SerializationFormatter;
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
					m_Flags &= SecurityPermissionFlag.SkipVerification;
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
					m_Flags &= SecurityPermissionFlag.UnmanagedCode;
				}
			}
		}

		public override IPermission CreatePermission ()
		{
			SecurityPermission perm = null;
			if (this.Unrestricted)
				perm = new SecurityPermission (PermissionState.Unrestricted);
			else
				perm = new SecurityPermission (m_Flags);
			return perm;
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
