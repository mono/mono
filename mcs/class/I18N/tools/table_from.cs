/**
 * Create a table from CHARSET to Unicode.
 *
 * @author Bruno Haible
 */

using System; /* String, Console */
using System.Text; /* Encoding */

public class table_from {
  static String toHexString1 (int i) {
    return new String(new char[] { "0123456789ABCDEF" [i] });
  }
  static String toHexString2 (int i) {
    return  toHexString1((i>>4)&0x0f)
           +toHexString1(i&0x0f);
  }
  static String toHexString4 (int i) {
    return  toHexString1((i>>12)&0x0f)
           +toHexString1((i>>8)&0x0f)
           +toHexString1((i>>4)&0x0f)
           +toHexString1(i&0x0f);
  }
  static void printOutput(char[] outp) {
    Console.Out.Write("0x");
    for (int j = 0; j < outp.Length; j++) {
      if (j > 0)
        Console.Out.Write(" 0x");
      if (j+1 < outp.Length
          && outp[j] >= 0xd800 && outp[j] < 0xdc00
          && outp[j+1] >= 0xdc00 && outp[j+1] < 0xe000) {
        int c = 0x10000 + ((outp[j] - 0xd800) << 10) + (outp[j+1] - 0xdc00);
        Console.Out.Write(((Int32)c).ToString("X"));
        j++;
      } else
        Console.Out.Write(toHexString4(outp[j]));
    }
  }
  public static int Main (String[] args) {
    try {
      if (args.Length != 1) {
        Console.Error.WriteLine("Usage: mono table_from charset");
        return 1;
      }
      String charset = args[0];
      Encoding encoding;
      try {
        encoding = Encoding.GetEncoding(charset);
      } catch (NotSupportedException e) {
        Console.Error.WriteLine("no converter for "+charset);
        return 1;
      }
      byte[] qmark = encoding.GetBytes(new char[] { (char)0x003f });
      for (int i0 = 0; i0 < 0x100; i0++) {
        char[] outp = encoding.GetChars(new byte[] { (byte)i0 });
        if (outp.Length > 0
            && !(outp.Length >= 1 && outp[0] == 0x003f
                 && !(qmark.Length == 1 && i0 == qmark[0]))) {
          Console.Out.Write("0x"+toHexString2(i0)+"\t");
          printOutput(outp);
          Console.Out.WriteLine();
        } else if (outp.Length <= 1) {
          for (int i1 = 0; i1 < 0x100; i1++) {
            outp = encoding.GetChars(new byte[] { (byte)i0, (byte)i1 });
            if (outp.Length > 0
                && !(outp.Length >= 1 && outp[0] == 0x003f
                     && !(qmark.Length == 2 && i0 == qmark[0] && i1 == qmark[1]))) {
              Console.Out.Write("0x"+toHexString2(i0)+toHexString2(i1)+"\t");
              printOutput(outp);
              Console.Out.WriteLine();
            } else if (outp.Length <= 1) {
              for (int i2 = 0; i2 < 0x100; i2++) {
                outp = encoding.GetChars(new byte[] { (byte)i0, (byte)i1, (byte)i2 });
                if (outp.Length > 0
                    && !(outp.Length >= 1 && outp[0] == 0x003f
                         && !(qmark.Length == 3
                              && i0 == qmark[0] && i1 == qmark[1] && i2 == qmark[2]))) {
                  Console.Out.Write("0x"+toHexString2(i0)+toHexString2(i1)+toHexString2(i2)+"\t");
                  printOutput(outp);
                  Console.Out.WriteLine();
                } else if (outp.Length <= 1) {
                  for (int i3 = 0; i3 < 0x100; i3++) {
                    outp = encoding.GetChars(new byte[] { (byte)i0, (byte)i1, (byte)i2, (byte)i3 });
                    if (outp.Length > 0
                        && !(outp.Length >= 1 && outp[0] == 0x003f
                             && !(qmark.Length == 4
                                  && i0 == qmark[0] && i1 == qmark[1] && i2 == qmark[2] && i3 == qmark[3]))) {
                      Console.Out.Write("0x"+toHexString2(i0)+toHexString2(i1)+toHexString2(i2)+toHexString2(i3)+"\t");
                      printOutput(outp);
                      Console.Out.WriteLine();
                    }
                  }
                }
              }
            }
          }
        }
      }
    } catch (Exception e) {
      Console.Error.WriteLine(e);
      Console.Error.WriteLine(e.StackTrace);
      return 1;
    }
    return 0;
  }
}
