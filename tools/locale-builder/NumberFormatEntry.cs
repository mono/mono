//
// Mono.Tools.LocaleBuilder.NumberFormatEntry
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
//


using System;
using System.Text;

namespace Mono.Tools.LocaleBuilder {

        public class NumberFormatEntry : Entry {

                public static readonly int MaxGroupSize = 5;

                public int CurrencyDecimalDigits;
                public string CurrencyDecimalSeparator;
                public string CurrencyGroupSeparator;
                public int [] CurrencyGroupSizes;
                public int CurrencyNegativePattern;
                public int CurrencyPositivePattern;
                public string CurrencySymbol;
                public string NaNSymbol;
                public string NegativeSign;
                public int NumberDecimalDigits;
                public string NumberDecimalSeparator;
                public string NumberGroupSeparator;
                public int [] NumberGroupSizes;
                public int NumberNegativePattern;
                public int PercentDecimalDigits;
                public string PercentDecimalSeparator;
                public string PercentGroupSeparator;
                public int [] PercentGroupSizes;
                public int PercentNegativePattern;
                public int PercentPositivePattern;
                public string PercentSymbol;
                public string PerMilleSymbol;
                public string PositiveInfinitySymbol;
                public string PositiveSign;

                public int Row;

                public string NegativeInfinitySymbol {
                        get {
                                return NegativeSign + PositiveInfinitySymbol;
                        }
                }
                
                public void AppendTableRow (StringBuilder builder)
                {
                        builder.Append ("\t{");

                        builder.Append (EncodeStringIdx (CurrencyDecimalSeparator) + ", ");
                        builder.Append (EncodeStringIdx (CurrencyGroupSeparator) + ", ");
                        builder.Append (EncodeStringIdx (PercentDecimalSeparator) + ", ");
                        builder.Append (EncodeStringIdx (PercentGroupSeparator) + ", ");
                        builder.Append (EncodeStringIdx (NumberDecimalSeparator) + ", ");
                        builder.Append (EncodeStringIdx (NumberGroupSeparator) + ", ");

                        builder.Append (EncodeStringIdx (CurrencySymbol) + ", ");
                        builder.Append (EncodeStringIdx (PercentSymbol) + ", ");
                        builder.Append (EncodeStringIdx (NaNSymbol) + ", ");
                        builder.Append (EncodeStringIdx (PerMilleSymbol) + ", ");
                        builder.Append (EncodeStringIdx (NegativeInfinitySymbol) + ", ");
                        builder.Append (EncodeStringIdx (PositiveInfinitySymbol) + ", ");

                        builder.Append (EncodeStringIdx (NegativeSign) + ", ");
                        builder.Append (EncodeStringIdx (PositiveSign) + ", ");

                        builder.Append (CurrencyNegativePattern + ", ");
                        builder.Append (CurrencyPositivePattern + ", ");
                        builder.Append (PercentNegativePattern + ", ");
                        builder.Append (PercentPositivePattern + ", ");
                        builder.Append (NumberNegativePattern + ", ");

                        builder.Append (CurrencyDecimalDigits + ", ");
                        builder.Append (PercentDecimalDigits + ", ");
                        builder.Append (NumberDecimalDigits + ", ");

                        AppendGroupSizes (builder, CurrencyGroupSizes);
                        builder.Append (", ");
                        AppendGroupSizes (builder, PercentGroupSizes);
                        builder.Append (", ");
                        AppendGroupSizes (builder, NumberGroupSizes);
                        
                        builder.Append ('}');
                }

                private void AppendGroupSizes (StringBuilder builder, int [] gs)
                {
                        int len = (gs == null ? 0 : gs.Length);

                        builder.Append ('{');
                        for (int i = 0; i < MaxGroupSize; i++) {
                                if (i < len)
                                        builder.Append (gs [0]);
                                else
                                        builder.Append (-1);
                                if (i+1 < MaxGroupSize)
                                        builder.Append (", ");
                        }
                        builder.Append ('}');
                }

                public override string ToString ()
                {
                        StringBuilder builder = new StringBuilder ();
                        AppendTableRow (builder);
                        return builder.ToString ();
                }
        }
}

