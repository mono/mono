//
// SupportingTokenParameters.cs
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Security;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Text;

using ParamList = System.Collections.ObjectModel.Collection<System.ServiceModel.Security.Tokens.SecurityTokenParameters>;

namespace System.ServiceModel.Security.Tokens
{
	public class SupportingTokenParameters
	{
		ParamList endorsing = new ParamList (),
			  signed = new ParamList (),
			  signed_encrypted = new ParamList (),
			  signed_endorsing = new ParamList ();

		public SupportingTokenParameters ()
		{
		}

		private SupportingTokenParameters (SupportingTokenParameters source)
		{
			endorsing = new ParamList (source.endorsing);
			signed = new ParamList (source.signed);
			signed_encrypted= new ParamList (source.signed_encrypted);
			signed_endorsing = new ParamList (source.signed_endorsing);
		}

		public Collection<SecurityTokenParameters> Endorsing {
			get { return endorsing; }
		}

		public Collection<SecurityTokenParameters> Signed {
			get { return signed; }
		}

		public Collection<SecurityTokenParameters> SignedEncrypted {
			get { return signed_encrypted; }
		}

		public Collection<SecurityTokenParameters> SignedEndorsing {
			get { return signed_endorsing; }
		}

		public SupportingTokenParameters Clone ()
		{
			return new SupportingTokenParameters (this);
		}

		public void SetKeyDerivation (bool requireDerivedKeys)
		{
			foreach (SecurityTokenParameters p in endorsing)
				p.RequireDerivedKeys = requireDerivedKeys;
			foreach (SecurityTokenParameters p in signed)
				p.RequireDerivedKeys = requireDerivedKeys;
			foreach (SecurityTokenParameters p in signed_encrypted)
				p.RequireDerivedKeys = requireDerivedKeys;
			foreach (SecurityTokenParameters p in signed_endorsing)
				p.RequireDerivedKeys = requireDerivedKeys;
		}

		public override string ToString ()
		{
			var sb = new StringBuilder ();
			AppendCollection (sb, Endorsing, "endorsing", "Endorsing");
			AppendCollection (sb, Signed, "signed", "Signed");
			AppendCollection (sb, SignedEncrypted, "signed encrypted", "SignedEncrypted");
			AppendCollection (sb, SignedEndorsing, "signed endorsing", "SignedEndorsing");
			sb.Length--; // chop trailing EOL.
			return sb.ToString ();
		}

		void AppendCollection (StringBuilder sb, Collection<SecurityTokenParameters> col, string emptyLabel, string label)
		{
			if (col.Count == 0)
				sb.AppendFormat ("No {0} tokens.\n", emptyLabel);
			for (int i = 0; i < col.Count; i++)
				sb.AppendFormat ("{0}[{1}]\n  {2}\n", label, i, String.Join ("\n  ", col [i].ToString ().Split ('\n')));
		}
	}
}
