//
// SecurityTokenParameters.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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

#if !MOBILE && !XAMMAC_4_5
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
#endif
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.Text;

namespace System.ServiceModel.Security.Tokens
{
	public abstract class SecurityTokenParameters
	{
		protected SecurityTokenParameters ()
		{
		}

		protected SecurityTokenParameters (SecurityTokenParameters other)
		{
			inclusion_mode = other.inclusion_mode;
			reference_style = other.reference_style;
			require_derived_keys = other.require_derived_keys;
			issuer_binding_context = other.issuer_binding_context != null ? other.issuer_binding_context.Clone () : null;
		}

		SecurityTokenInclusionMode inclusion_mode;
		SecurityTokenReferenceStyle reference_style;
		bool require_derived_keys = true;
		BindingContext issuer_binding_context;

		public SecurityTokenInclusionMode InclusionMode {
			get { return inclusion_mode; }
			set { inclusion_mode = value; }
		}

		public SecurityTokenReferenceStyle ReferenceStyle {
			get { return reference_style; }
			set { reference_style = value; }
		}

		public bool RequireDerivedKeys {
			get { return require_derived_keys; }
			set { require_derived_keys = value; }
		}

		public SecurityTokenParameters Clone ()
		{
			return CloneCore ();
		}

		public override string ToString ()
		{
			var sb = new StringBuilder ();
			sb.Append (GetType ().FullName).Append (":\n");
			foreach (var pi in GetType ().GetProperties ()) {
				var simple = Type.GetTypeCode (pi.PropertyType) != TypeCode.Object;
				var val = pi.GetValue (this, null);
				sb.Append (pi.Name).Append (':');
				if (val != null)
					sb.AppendFormat ("{0}{1}{2}", simple ? " " : "\n", simple ? "" : "  ", String.Join ("\n  ", val.ToString ().Split ('\n')));
				sb.Append ('\n');
			}
			sb.Length--; // chop trailing EOL.
			return sb.ToString ();
		}

		protected abstract bool HasAsymmetricKey { get; }

		protected abstract bool SupportsClientAuthentication { get; }

		protected abstract bool SupportsClientWindowsIdentity { get; }

		protected abstract bool SupportsServerAuthentication { get; }

		internal bool InternalHasAsymmetricKey {
			get { return HasAsymmetricKey; }
		}

		internal bool InternalSupportsClientAuthentication {
			get { return SupportsClientAuthentication; }
		}

		internal bool InternalSupportsClientWindowsIdentity {
			get { return SupportsClientWindowsIdentity; }
		}

		internal bool InternalSupportsServerAuthentication {
			get { return SupportsServerAuthentication; }
		}

		protected abstract SecurityTokenParameters CloneCore ();

#if !MOBILE && !XAMMAC_4_5
		protected abstract SecurityKeyIdentifierClause CreateKeyIdentifierClause (
			SecurityToken token, SecurityTokenReferenceStyle referenceStyle);

		// internalized call to CreateKeyIdentifierClause()
		internal SecurityKeyIdentifierClause CallCreateKeyIdentifierClause (
			SecurityToken token, SecurityTokenReferenceStyle referenceStyle)
		{
			return CreateKeyIdentifierClause (token, referenceStyle);
		}

		protected internal abstract void InitializeSecurityTokenRequirement (SecurityTokenRequirement requirement);
#endif

		internal BindingContext IssuerBindingContext {
			set { issuer_binding_context = value; }
		}

#if !MOBILE && !XAMMAC_4_5
		internal void CallInitializeSecurityTokenRequirement (SecurityTokenRequirement requirement)
		{
			if (issuer_binding_context != null)
				requirement.Properties [ServiceModelSecurityTokenRequirement.IssuerBindingContextProperty] = issuer_binding_context;
			InitializeSecurityTokenRequirement (requirement);
		}

		[MonoTODO]
		protected virtual bool MatchesKeyIdentifierClause (
			SecurityToken token,
			SecurityKeyIdentifierClause keyIdentifierClause,
			SecurityTokenReferenceStyle referenceStyle)
		{
			throw new NotImplementedException ();
		}
#endif
	}
}
