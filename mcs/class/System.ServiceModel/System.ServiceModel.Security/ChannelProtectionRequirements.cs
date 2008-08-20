//
// ChannelProtectionRequirements.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005-2006 Novell, Inc.  http://www.novell.com
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
using System.Net.Security;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Xml;

namespace System.ServiceModel.Security
{
	// Represents sp:SignedParts and sp:EncryptedParts in
	// sp:SupportingTokens/ws:Policy/.
	public class ChannelProtectionRequirements
	{
		bool is_readonly;
		ScopedMessagePartSpecification in_enc, in_sign, out_enc, out_sign;

		public ChannelProtectionRequirements ()
		{
			in_enc = new ScopedMessagePartSpecification ();
			out_enc = new ScopedMessagePartSpecification ();
			in_sign = new ScopedMessagePartSpecification ();
			out_sign = new ScopedMessagePartSpecification ();
		}

		public ChannelProtectionRequirements (
			ChannelProtectionRequirements other)
		{
			if (other == null)
				throw new ArgumentNullException ("other");
			in_enc = new ScopedMessagePartSpecification (other.in_enc);
			out_enc = new ScopedMessagePartSpecification (other.out_enc);
			in_sign = new ScopedMessagePartSpecification (other.in_sign);
			out_sign = new ScopedMessagePartSpecification (other.out_sign);
		}

		public bool IsReadOnly {
			get { return is_readonly; }
		}

		public ScopedMessagePartSpecification IncomingEncryptionParts {
			get { return in_enc; }
		}

		public ScopedMessagePartSpecification IncomingSignatureParts {
			get { return in_sign; }
		}

		public ScopedMessagePartSpecification OutgoingEncryptionParts {
			get { return out_enc; }
		}

		public ScopedMessagePartSpecification OutgoingSignatureParts {
			get { return out_sign; }
		}

		public void Add (
			ChannelProtectionRequirements protectionRequirements)
		{
			Add (protectionRequirements, false);
		}

		public void Add (
			ChannelProtectionRequirements protectionRequirements,
			bool channelScopeOnly)
		{
			if (is_readonly)
				throw new InvalidOperationException ("This ChannelProtectionRequirements is read-only.");

			AddScopedParts (
				protectionRequirements.IncomingEncryptionParts, 
				IncomingEncryptionParts,
				channelScopeOnly);
			AddScopedParts (
				protectionRequirements.IncomingSignatureParts, 
				IncomingSignatureParts,
				channelScopeOnly);
			AddScopedParts (
				protectionRequirements.OutgoingEncryptionParts, 
				OutgoingEncryptionParts,
				channelScopeOnly);
			AddScopedParts (
				protectionRequirements.OutgoingSignatureParts, 
				OutgoingSignatureParts,
				channelScopeOnly);
		}

		void AddScopedParts (ScopedMessagePartSpecification src, ScopedMessagePartSpecification dst, bool channelOnly)
		{
			dst.AddParts (src.ChannelParts);
			if (channelOnly)
				return;

			foreach (string a in src.Actions) {
				MessagePartSpecification m;
				src.TryGetParts (a, out m);
				src.AddParts (m);
			}
		}

		public ChannelProtectionRequirements CreateInverse ()
		{
			ChannelProtectionRequirements r =
				new ChannelProtectionRequirements ();
			AddScopedParts (in_enc, r.out_enc, false);
			AddScopedParts (in_sign, r.out_sign, false);
			AddScopedParts (out_enc, r.in_enc, false);
			AddScopedParts (out_sign, r.in_sign, false);
			return r;
		}

		public void MakeReadOnly ()
		{
			is_readonly = true;
			in_enc.MakeReadOnly ();
			in_sign.MakeReadOnly ();
			out_enc.MakeReadOnly ();
			out_sign.MakeReadOnly ();
		}

		internal static ChannelProtectionRequirements CreateFromContract (ContractDescription cd)
		{
			ChannelProtectionRequirements cp =
				new ChannelProtectionRequirements ();
			List<XmlQualifiedName> enc = new List<XmlQualifiedName> ();
			List<XmlQualifiedName> sig = new List<XmlQualifiedName> ();
			if (cd.HasProtectionLevel) {
				switch (cd.ProtectionLevel) {
				case ProtectionLevel.EncryptAndSign:
					cp.IncomingEncryptionParts.ChannelParts.IsBodyIncluded = true;
					cp.OutgoingEncryptionParts.ChannelParts.IsBodyIncluded = true;
					goto case ProtectionLevel.Sign;
				case ProtectionLevel.Sign:
					cp.IncomingSignatureParts.ChannelParts.IsBodyIncluded = true;
					cp.OutgoingSignatureParts.ChannelParts.IsBodyIncluded = true;
					break;
				}
			}
			foreach (OperationDescription od in cd.Operations) {
				foreach (MessageDescription md in od.Messages) {
					enc.Clear ();
					sig.Clear ();
					ProtectionLevel mplv =
						md.HasProtectionLevel ? md.ProtectionLevel :
						od.HasProtectionLevel ? od.ProtectionLevel :
						ProtectionLevel.EncryptAndSign; // default
					foreach (MessageHeaderDescription hd in md.Headers)
						AddPartProtectionRequirements (enc, sig, hd, cp);

					ScopedMessagePartSpecification spec;
					bool includeBodyEnc = mplv == ProtectionLevel.EncryptAndSign;
					bool includeBodySig = mplv != ProtectionLevel.None;

					// enc
					spec = md.Direction == MessageDirection.Input ?
						cp.IncomingEncryptionParts :
						cp.OutgoingEncryptionParts;
					spec.AddParts (new MessagePartSpecification (includeBodyEnc, enc.ToArray ()), md.Action);
					// sig
					spec = md.Direction == MessageDirection.Input ?
						cp.IncomingSignatureParts :
						cp.OutgoingSignatureParts;
					spec.AddParts (new MessagePartSpecification (includeBodySig, sig.ToArray ()), md.Action);
				}
			}
			return cp;
		}

		static void AddPartProtectionRequirements (List<XmlQualifiedName> enc,
			List<XmlQualifiedName> sig,
			MessageHeaderDescription pd,
			ChannelProtectionRequirements cp)
		{
			if (!pd.HasProtectionLevel)
				return; // no specific part indication
			switch (pd.ProtectionLevel) {
			case ProtectionLevel.EncryptAndSign:
				enc.Add (new XmlQualifiedName (pd.Name, pd.Namespace));
				goto case ProtectionLevel.Sign;
			case ProtectionLevel.Sign:
				sig.Add (new XmlQualifiedName (pd.Name, pd.Namespace));
				break;
			}
		}
	}
}
