//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------


namespace System.ServiceModel.Security.Tokens
{
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.ServiceModel.Diagnostics;
    using System.Text;

    public class SupportingTokenParameters
    {
        Collection<SecurityTokenParameters> signed = new Collection<SecurityTokenParameters>();
        Collection<SecurityTokenParameters> signedEncrypted = new Collection<SecurityTokenParameters>();
        Collection<SecurityTokenParameters> endorsing = new Collection<SecurityTokenParameters>();
        Collection<SecurityTokenParameters> signedEndorsing = new Collection<SecurityTokenParameters>();

        SupportingTokenParameters(SupportingTokenParameters other)
        {
            if (other == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("other");

            foreach (SecurityTokenParameters p in other.signed)
                this.signed.Add((SecurityTokenParameters)p.Clone());
            foreach (SecurityTokenParameters p in other.signedEncrypted)
                this.signedEncrypted.Add((SecurityTokenParameters)p.Clone());
            foreach (SecurityTokenParameters p in other.endorsing)
                this.endorsing.Add((SecurityTokenParameters)p.Clone());
            foreach (SecurityTokenParameters p in other.signedEndorsing)
                this.signedEndorsing.Add((SecurityTokenParameters)p.Clone());
        }

        public SupportingTokenParameters()
        {
            // empty
        }

        public Collection<SecurityTokenParameters> Endorsing
        {
            get
            {
                return this.endorsing;
            }
        }

        public Collection<SecurityTokenParameters> SignedEndorsing
        {
            get
            {
                return this.signedEndorsing;
            }
        }

        public Collection<SecurityTokenParameters> Signed
        {
            get
            {
                return this.signed;
            }
        }

        public Collection<SecurityTokenParameters> SignedEncrypted
        {
            get
            {
                return this.signedEncrypted;
            }
        }

        public void SetKeyDerivation(bool requireDerivedKeys)
        {
            foreach (SecurityTokenParameters t in this.endorsing)
            {
                if (t.HasAsymmetricKey)
                {
                    t.RequireDerivedKeys = false;
                }
                else
                {
                    t.RequireDerivedKeys = requireDerivedKeys;
                }
            }
            foreach (SecurityTokenParameters t in this.signedEndorsing)
            {
                if (t.HasAsymmetricKey)
                {
                    t.RequireDerivedKeys = false;
                }
                else
                {
                    t.RequireDerivedKeys = requireDerivedKeys;
                }
            }
        }

        internal bool IsSetKeyDerivation(bool requireDerivedKeys)
        {
            foreach (SecurityTokenParameters t in this.endorsing)
                if (t.RequireDerivedKeys != requireDerivedKeys)
                    return false;

            foreach (SecurityTokenParameters t in this.signedEndorsing)
                if (t.RequireDerivedKeys != requireDerivedKeys)
                    return false;
            return true;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            int k;

            if (this.endorsing.Count == 0)
                sb.AppendLine("No endorsing tokens.");
            else
                for (k = 0; k < this.endorsing.Count; k++)
                {
                    sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "Endorsing[{0}]", k.ToString(CultureInfo.InvariantCulture)));
                    sb.AppendLine("  " + this.endorsing[k].ToString().Trim().Replace("\n", "\n  "));
                }

            if (this.signed.Count == 0)
                sb.AppendLine("No signed tokens.");
            else
                for (k = 0; k < this.signed.Count; k++)
                {
                    sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "Signed[{0}]", k.ToString(CultureInfo.InvariantCulture)));
                    sb.AppendLine("  " + this.signed[k].ToString().Trim().Replace("\n", "\n  "));
                }

            if (this.signedEncrypted.Count == 0)
                sb.AppendLine("No signed encrypted tokens.");
            else
                for (k = 0; k < this.signedEncrypted.Count; k++)
                {
                    sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "SignedEncrypted[{0}]", k.ToString(CultureInfo.InvariantCulture)));
                    sb.AppendLine("  " + this.signedEncrypted[k].ToString().Trim().Replace("\n", "\n  "));
                }

            if (this.signedEndorsing.Count == 0)
                sb.AppendLine("No signed endorsing tokens.");
            else
                for (k = 0; k < this.signedEndorsing.Count; k++)
                {
                    sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "SignedEndorsing[{0}]", k.ToString(CultureInfo.InvariantCulture)));
                    sb.AppendLine("  " + this.signedEndorsing[k].ToString().Trim().Replace("\n", "\n  "));
                }

            return sb.ToString().Trim();
        }

        public SupportingTokenParameters Clone()
        {
            SupportingTokenParameters parameters = this.CloneCore();
            if (parameters == null || parameters.GetType() != this.GetType())
            {
                TraceUtility.TraceEvent(
                    TraceEventType.Error, 
                    TraceCode.Security, 
                    SR.GetString(SR.CloneNotImplementedCorrectly, new object[] { this.GetType(), (parameters != null) ? parameters.ToString() : "null" }));
            }

            return parameters;
        }

        protected virtual SupportingTokenParameters CloneCore()
        {
            return new SupportingTokenParameters(this);
        }

        internal bool IsEmpty()
        {
            return signed.Count == 0 && signedEncrypted.Count == 0 && endorsing.Count == 0 && signedEndorsing.Count == 0;
        }
    }
}
