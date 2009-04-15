/**
 * Create a table from Unicode to CHARSET.
 *
 * @author Bruno Haible
 */

using System; /* String, Console */
using System.Text; /* Encoding */

public class table_to {
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
  public static int Main (String[] args) {
    try {
      if (args.Length != 1) {
        Console.Error.WriteLine("Usage: mono table_to charset");
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
      for (int i = 0; i < 0x110000; i++) {
        char[] inp =
          (i < 0x10000
           ? new char[] { (char)i }
           : new char[] { (char)(0xd800 + ((i - 0x10000) >> 10)),
                          (char)(0xdc00 + ((i - 0x10000) & 0x3ff)) });
        byte[] outp = encoding.GetBytes(inp);
        if (!(((outp.Length >= qmark.Length
                && outp[0] == qmark[0]
                && (qmark.Length < 2 || outp[1] == qmark[1])
                && (qmark.Length < 3 || outp[2] == qmark[2])
                && (qmark.Length < 4 || outp[3] == qmark[3]))
               || (outp.Length >= 1 && outp[0] == 0x3f))
              && !(i == 0x003f))) {
          Console.Out.Write("0x");
          for (int j = 0; j < outp.Length; j++)
            Console.Out.Write(toHexString2(outp[j]));
          Console.Out.WriteLine("\t0x" + (i<0x10000 ? toHexString4(i) : ((Int32)i).ToString("X")));
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
