using System;
using System.Threading;
using System.Globalization;

namespace Mono.Tools {

        public class DumpCultureInfo {

		internal static string ToUnicode (string input)
		{
			string output = "";
			for (int i = 0; i < input.Length; i++) {
				int chr = input [i];
				output += "\\x" + chr.ToString ("x4");
			}

			return output;
		}

		public static void DumpNumberFormatInfo (CultureInfo culture)
		{
			NumberFormatInfo nfi = culture.NumberFormat;

			string currency_group_sizes = "";
			{
				int[] group_sizes = nfi.CurrencyGroupSizes;

				currency_group_sizes = "new int[" + group_sizes.Length + "] { ";
				for (int i = 0; i < group_sizes.Length; i++) {
					if (i > 0) currency_group_sizes += ", ";
					currency_group_sizes += group_sizes[i] + " ";
				}
				currency_group_sizes += "}";
			}

			string number_group_sizes = "";
			{
				int[] group_sizes = nfi.NumberGroupSizes;

				number_group_sizes = "new int[" + group_sizes.Length + "] { ";
				for (int i = 0; i < group_sizes.Length; i++) {
					if (i > 0) number_group_sizes += ", ";
					number_group_sizes += group_sizes[i] + " ";
				}
				number_group_sizes += "}";
			}

			string percent_group_sizes = "";
			{
				int[] group_sizes = nfi.PercentGroupSizes;

				percent_group_sizes = "new int[" + group_sizes.Length + "] { ";
				for (int i = 0; i < group_sizes.Length; i++) {
					if (i > 0) percent_group_sizes += ", ";
					percent_group_sizes += group_sizes[i] + " ";
				}
				percent_group_sizes += "}";
			}


			Object[] data = { "\t\t\t\t",
					  nfi.CurrencyDecimalDigits,
					  ToUnicode (nfi.CurrencyDecimalSeparator),
					  ToUnicode (nfi.CurrencyGroupSeparator),
					  currency_group_sizes,
					  nfi.CurrencyNegativePattern,
					  nfi.CurrencyPositivePattern,
					  ToUnicode (nfi.CurrencySymbol),
					  ToUnicode (nfi.NaNSymbol),
					  ToUnicode (nfi.NegativeInfinitySymbol),
					  ToUnicode (nfi.NegativeSign),
					  nfi.NumberDecimalDigits,
					  ToUnicode (nfi.NumberDecimalSeparator),
					  ToUnicode (nfi.NumberGroupSeparator),
					  number_group_sizes,
					  nfi.NumberNegativePattern,
					  nfi.PercentDecimalDigits,
					  ToUnicode (nfi.PercentDecimalSeparator),
					  ToUnicode (nfi.PercentGroupSeparator),
					  percent_group_sizes,
					  nfi.PercentNegativePattern,
					  nfi.PercentPositivePattern,
					  ToUnicode (nfi.PercentSymbol),
					  ToUnicode (nfi.PerMilleSymbol),
					  ToUnicode (nfi.PositiveInfinitySymbol),
					  ToUnicode (nfi.PositiveSign)
			};

			string format = "{0}currencyDecimalDigits\t\t= {1};\n"
				+ "{0}currencyDecimalSeparator\t= \"{2}\";\n"
				+ "{0}currencyGroupSeparator\t\t= \"{3}\";\n"
				+ "{0}currencyGroupSizes\t\t= {4};\n"
				+ "{0}currencyNegativePattern\t\t= {5};\n"
				+ "{0}currencyPositivePattern\t\t= {6};\n"
				+ "{0}currencySymbol\t\t\t= \"{7}\";\n\n"
				+ "{0}naNSymbol\t\t\t= \"{8}\";\n"
				+ "{0}negativeInfinitySymbol\t\t= \"{9}\";\n"
				+ "{0}negativeSign\t\t\t= \"{10}\";\n\n"
				+ "{0}numberDecimalDigits\t\t= {11};\n"
				+ "{0}numberDecimalSeparator\t\t= \"{12}\";\n"
				+ "{0}numberGroupSeparator\t\t= \"{13}\";\n"
				+ "{0}numberGroupSizes\t\t= {14};\n"
				+ "{0}numberNegativePattern\t\t= {15};\n"
				+ "{0}percentDecimalDigits\t\t= {16};\n"
				+ "{0}percentDecimalSeparator\t\t= \"{17}\";\n"
				+ "{0}percentGroupSeparator\t\t= \"{18}\";\n"
				+ "{0}percentGroupSizes\t\t= {19};\n"
				+ "{0}percentNegativePattern\t\t= {20};\n"
				+ "{0}percentPositivePattern\t\t= {21};\n"
				+ "{0}percentSymbol\t\t\t= \"{22}\";\n\n"
				+ "{0}perMilleSymbol\t\t\t= \"{23}\";\n"
				+ "{0}positiveInfinitySymbol\t\t= \"{24}\";\n"
				+ "{0}positiveSign\t\t\t= \"{25}\";\n";

			string output = String.Format (format, data);

			Object[] data2 = { "\t\t\t\t", "\t\t\t", culture.EnglishName,
					   culture.LCID.ToString ("x4"), output };

			string format2 = "{0}// {2}\n{1}case 0x{3}:\n{0}readOnly = false;\n\n"
				+ "{4}\n{0}break;\n\n";

			Console.WriteLine (String.Format (format2, data2));
		}

		public static int[] AllCultures = {
			0x007F, 0x0001, 0x0401, 0x0801, 0x0C01, 0x1001, 0x1401, 0x1801,
			0x1C01, 0x2001, 0x2401, 0x2801, 0x2C01, 0x3001, 0x3401, 0x3801,
			0x3C01, 0x4001, 0x0002, 0x0402, 0x0003, 0x0403, 0x0004, 0x0404,
			0x0804, 0x0C04, 0x1004, 0x1404, 0x7C04, 0x0005, 0x0405, 0x0006,
			0x0406, 0x0007, 0x0407, 0x0807, 0x0C07, 0x1007, 0x1407, 0x0008,
			0x0408, 0x0009, 0x0409, 0x0809, 0x0C09, 0x1009, 0x1409, 0x1809,
			0x1C09, 0x2009, 0x2409, 0x2809, 0x2C09, 0x3009, 0x3409, 0x000A,
			0x080A, 0x0C0A, 0x100A, 0x140A, 0x180A, 0x1C0A, 0x200A, 0x240A,
			0x280A, 0x2C0A, 0x300A, 0x340A, 0x380A, 0x3C0A, 0x400A, 0x440A,
			0x480A, 0x4C0A, 0x500A, 0x000B, 0x040B, 0x000C, 0x040C, 0x080C,
			0x0C0C, 0x100C, 0x140C, 0x180C, 0x000D, 0x040D, 0x000E, 0x040E,
			0x000F, 0x040F, 0x0010, 0x0410, 0x0810, 0x0011, 0x0411, 0x0012,
			0x0412, 0x0013, 0x0413, 0x0813, 0x0014, 0x0414, 0x0814, 0x0015,
			0x0415, 0x0016, 0x0416, 0x0816, 0x0018, 0x0418, 0x0019, 0x0419,
			0x001A, 0x041A, 0x081A, 0x0C1A, 0x001B, 0x041B, 0x001C, 0x041C,
			0x001D, 0x041D, 0x081D, 0x001E, 0x041E, 0x001F, 0x041F, 0x0020,
			0x0420, 0x0021, 0x0421, 0x0022, 0x0422, 0x0023, 0x0423, 0x0024,
			0x0424, 0x0025, 0x0425, 0x0026, 0x0426, 0x0027, 0x0427, 0x0029,
			0x0429, 0x002A, 0x042A, 0x002B, 0x042B, 0x002C, 0x042C, 0x082C,
			0x002D, 0x042D, 0x002F, 0x042F, 0x0036, 0x0436, 0x0037, 0x0437,
			0x0038, 0x0438, 0x0039, 0x0439, 0x003E, 0x043E, 0x083E, 0x003F,
			0x043F, 0x0040, 0x0440, 0x0041, 0x0441, 0x0043, 0x0443, 0x0843,
			0x0044, 0x0444, 0x0046, 0x0446, 0x0047, 0x0447, 0x0049, 0x0449,
			0x004A, 0x044A, 0x004B, 0x044B, 0x004E, 0x044E, 0x004F, 0x044F,
			0x0050, 0x0450, 0x0056, 0x0456, 0x0057, 0x0457, 0x005A, 0x045A,
			0x0065, 0x0465
		};

		public static int[] Cultures = {
			// Invariant culture
			0x007f,
			// English
			0x0409, 0x0809, 0x0c09, 0x1009,
			// German
			0x0407, 0x0807, 0x0c07, 0x1007, 0x1407
		};

		public static void Main ()
		{
			for (int i = 0; i < AllCultures.Length; i++) {
				CultureInfo culture = new CultureInfo (AllCultures [i], false);

				if (culture.IsNeutralCulture)
					continue;

				DumpNumberFormatInfo (culture);
			}
		}
        }
}
