namespace System.ComponentModel.DataAnnotations {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Resources;
    using System.Linq;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class CreditCardAttribute : DataTypeAttribute {
        public CreditCardAttribute()
            : base(DataType.CreditCard) {
            ErrorMessage = DataAnnotationsResources.CreditCardAttribute_Invalid;
        }

        public override bool IsValid(object value) {
            if (value == null) {
                return true;
            }

            string ccValue = value as string;
            if (ccValue == null) {
                return false;
            }
            ccValue = ccValue.Replace("-", "");
            ccValue = ccValue.Replace(" ", "");

            int checksum = 0;
            bool evenDigit = false;

            // http://www.beachnet.com/~hstiles/cardtype.html
            foreach (char digit in ccValue.Reverse()) {
                if (digit < '0' || digit > '9') {
                    return false;
                }

                int digitValue = (digit - '0') * (evenDigit ? 2 : 1);
                evenDigit = !evenDigit;

                while (digitValue > 0) {
                    checksum += digitValue % 10;
                    digitValue /= 10;
                }
            }

            return (checksum % 10) == 0;
        }
    }
}
