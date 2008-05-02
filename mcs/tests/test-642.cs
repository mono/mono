enum E : byte { }

class C
{
  static E e;
  static byte b;
  static ushort u;

  public static int Main ()
  {
    b |= (byte) e;
    u |= (ushort) e;
    return b;
  }
}
