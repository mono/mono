using System;
class Test {

	static void receive_int (int a)
	{
		Console.Write ("        ");
		Console.WriteLine (a);
	}

	static void receive_uint (uint a)
	{
		Console.Write ("        ");
		Console.WriteLine (a);
	}

	static void receive_short (short a)
	{
		Console.Write ("        ");
		Console.WriteLine (a);
	}

	static void receive_ushort (ushort a)
	{
		Console.Write ("        ");
		Console.WriteLine (a);
	}

	static void receive_long (long a)
	{
		Console.Write ("        ");
		Console.WriteLine (a);
	}

	static void receive_ulong (ulong a)
	{
		Console.Write ("        ");
		Console.WriteLine (a);
	}

	static void receive_sbyte (sbyte a)
	{
		Console.Write ("        ");
		Console.WriteLine (a);
	}

	static void receive_byte (byte a)
	{
		Console.Write ("        ");
		Console.WriteLine (a);
	}

	static void receive_char (char a)
	{
		Console.Write ("        ");
		Console.WriteLine (a);
	}

	static void probe_int()
	{
		int zero = (int) 0;
		int min = (int) int.MinValue;
		int max = (int) int.MaxValue;

		Console.WriteLine ("Testing: int");

		Console.WriteLine ("   arg: int (int)");

		receive_int (unchecked((int) zero));
		receive_int (unchecked((int) min));
		receive_int (unchecked((int) max));
		Console.WriteLine ("   arg: uint (int)");

		receive_uint (unchecked((uint) zero));
		receive_uint (unchecked((uint) min));
		receive_uint (unchecked((uint) max));
		Console.WriteLine ("   arg: short (int)");

		receive_short (unchecked((short) zero));
		receive_short (unchecked((short) min));
		receive_short (unchecked((short) max));
		Console.WriteLine ("   arg: ushort (int)");

		receive_ushort (unchecked((ushort) zero));
		receive_ushort (unchecked((ushort) min));
		receive_ushort (unchecked((ushort) max));
		Console.WriteLine ("   arg: long (int)");

		receive_long (unchecked((long) zero));
		receive_long (unchecked((long) min));
		receive_long (unchecked((long) max));
		Console.WriteLine ("   arg: ulong (int)");

		receive_ulong (unchecked((ulong) zero));
		receive_ulong (unchecked((ulong) min));
		receive_ulong (unchecked((ulong) max));
		Console.WriteLine ("   arg: sbyte (int)");

		receive_sbyte (unchecked((sbyte) zero));
		receive_sbyte (unchecked((sbyte) min));
		receive_sbyte (unchecked((sbyte) max));
		Console.WriteLine ("   arg: byte (int)");

		receive_byte (unchecked((byte) zero));
		receive_byte (unchecked((byte) min));
		receive_byte (unchecked((byte) max));
		Console.WriteLine ("   arg: char (int)");

		receive_char (unchecked((char) zero));
		receive_char (unchecked((char) min));
		receive_char (unchecked((char) max));
	}

	static void probe_uint()
	{
		uint zero = (uint) 0;
		uint min = (uint) uint.MinValue;
		uint max = (uint) uint.MaxValue;

		Console.WriteLine ("Testing: uint");

		Console.WriteLine ("   arg: int (uint)");

		receive_int (unchecked((int) zero));
		receive_int (unchecked((int) min));
		receive_int (unchecked((int) max));
		Console.WriteLine ("   arg: uint (uint)");

		receive_uint (unchecked((uint) zero));
		receive_uint (unchecked((uint) min));
		receive_uint (unchecked((uint) max));
		Console.WriteLine ("   arg: short (uint)");

		receive_short (unchecked((short) zero));
		receive_short (unchecked((short) min));
		receive_short (unchecked((short) max));
		Console.WriteLine ("   arg: ushort (uint)");

		receive_ushort (unchecked((ushort) zero));
		receive_ushort (unchecked((ushort) min));
		receive_ushort (unchecked((ushort) max));
		Console.WriteLine ("   arg: long (uint)");

		receive_long (unchecked((long) zero));
		receive_long (unchecked((long) min));
		receive_long (unchecked((long) max));
		Console.WriteLine ("   arg: ulong (uint)");

		receive_ulong (unchecked((ulong) zero));
		receive_ulong (unchecked((ulong) min));
		receive_ulong (unchecked((ulong) max));
		Console.WriteLine ("   arg: sbyte (uint)");

		receive_sbyte (unchecked((sbyte) zero));
		receive_sbyte (unchecked((sbyte) min));
		receive_sbyte (unchecked((sbyte) max));
		Console.WriteLine ("   arg: byte (uint)");

		receive_byte (unchecked((byte) zero));
		receive_byte (unchecked((byte) min));
		receive_byte (unchecked((byte) max));
		Console.WriteLine ("   arg: char (uint)");

		receive_char (unchecked((char) zero));
		receive_char (unchecked((char) min));
		receive_char (unchecked((char) max));
	}

	static void probe_short()
	{
		short zero = (short) 0;
		short min = (short) short.MinValue;
		short max = (short) short.MaxValue;

		Console.WriteLine ("Testing: short");

		Console.WriteLine ("   arg: int (short)");

		receive_int (unchecked((int) zero));
		receive_int (unchecked((int) min));
		receive_int (unchecked((int) max));
		Console.WriteLine ("   arg: uint (short)");

		receive_uint (unchecked((uint) zero));
		receive_uint (unchecked((uint) min));
		receive_uint (unchecked((uint) max));
		Console.WriteLine ("   arg: short (short)");

		receive_short (unchecked((short) zero));
		receive_short (unchecked((short) min));
		receive_short (unchecked((short) max));
		Console.WriteLine ("   arg: ushort (short)");

		receive_ushort (unchecked((ushort) zero));
		receive_ushort (unchecked((ushort) min));
		receive_ushort (unchecked((ushort) max));
		Console.WriteLine ("   arg: long (short)");

		receive_long (unchecked((long) zero));
		receive_long (unchecked((long) min));
		receive_long (unchecked((long) max));
		Console.WriteLine ("   arg: ulong (short)");

		receive_ulong (unchecked((ulong) zero));
		receive_ulong (unchecked((ulong) min));
		receive_ulong (unchecked((ulong) max));
		Console.WriteLine ("   arg: sbyte (short)");

		receive_sbyte (unchecked((sbyte) zero));
		receive_sbyte (unchecked((sbyte) min));
		receive_sbyte (unchecked((sbyte) max));
		Console.WriteLine ("   arg: byte (short)");

		receive_byte (unchecked((byte) zero));
		receive_byte (unchecked((byte) min));
		receive_byte (unchecked((byte) max));
		Console.WriteLine ("   arg: char (short)");

		receive_char (unchecked((char) zero));
		receive_char (unchecked((char) min));
		receive_char (unchecked((char) max));
	}

	static void probe_ushort()
	{
		ushort zero = (ushort) 0;
		ushort min = (ushort) ushort.MinValue;
		ushort max = (ushort) ushort.MaxValue;

		Console.WriteLine ("Testing: ushort");

		Console.WriteLine ("   arg: int (ushort)");

		receive_int (unchecked((int) zero));
		receive_int (unchecked((int) min));
		receive_int (unchecked((int) max));
		Console.WriteLine ("   arg: uint (ushort)");

		receive_uint (unchecked((uint) zero));
		receive_uint (unchecked((uint) min));
		receive_uint (unchecked((uint) max));
		Console.WriteLine ("   arg: short (ushort)");

		receive_short (unchecked((short) zero));
		receive_short (unchecked((short) min));
		receive_short (unchecked((short) max));
		Console.WriteLine ("   arg: ushort (ushort)");

		receive_ushort (unchecked((ushort) zero));
		receive_ushort (unchecked((ushort) min));
		receive_ushort (unchecked((ushort) max));
		Console.WriteLine ("   arg: long (ushort)");

		receive_long (unchecked((long) zero));
		receive_long (unchecked((long) min));
		receive_long (unchecked((long) max));
		Console.WriteLine ("   arg: ulong (ushort)");

		receive_ulong (unchecked((ulong) zero));
		receive_ulong (unchecked((ulong) min));
		receive_ulong (unchecked((ulong) max));
		Console.WriteLine ("   arg: sbyte (ushort)");

		receive_sbyte (unchecked((sbyte) zero));
		receive_sbyte (unchecked((sbyte) min));
		receive_sbyte (unchecked((sbyte) max));
		Console.WriteLine ("   arg: byte (ushort)");

		receive_byte (unchecked((byte) zero));
		receive_byte (unchecked((byte) min));
		receive_byte (unchecked((byte) max));
		Console.WriteLine ("   arg: char (ushort)");

		receive_char (unchecked((char) zero));
		receive_char (unchecked((char) min));
		receive_char (unchecked((char) max));
	}

	static void probe_long()
	{
		long zero = (long) 0;
		long min = (long) long.MinValue;
		long max = (long) long.MaxValue;

		Console.WriteLine ("Testing: long");

		Console.WriteLine ("   arg: int (long)");

		receive_int (unchecked((int) zero));
		receive_int (unchecked((int) min));
		receive_int (unchecked((int) max));
		Console.WriteLine ("   arg: uint (long)");

		receive_uint (unchecked((uint) zero));
		receive_uint (unchecked((uint) min));
		receive_uint (unchecked((uint) max));
		Console.WriteLine ("   arg: short (long)");

		receive_short (unchecked((short) zero));
		receive_short (unchecked((short) min));
		receive_short (unchecked((short) max));
		Console.WriteLine ("   arg: ushort (long)");

		receive_ushort (unchecked((ushort) zero));
		receive_ushort (unchecked((ushort) min));
		receive_ushort (unchecked((ushort) max));
		Console.WriteLine ("   arg: long (long)");

		receive_long (unchecked((long) zero));
		receive_long (unchecked((long) min));
		receive_long (unchecked((long) max));
		Console.WriteLine ("   arg: ulong (long)");

		receive_ulong (unchecked((ulong) zero));
		receive_ulong (unchecked((ulong) min));
		receive_ulong (unchecked((ulong) max));
		Console.WriteLine ("   arg: sbyte (long)");

		receive_sbyte (unchecked((sbyte) zero));
		receive_sbyte (unchecked((sbyte) min));
		receive_sbyte (unchecked((sbyte) max));
		Console.WriteLine ("   arg: byte (long)");

		receive_byte (unchecked((byte) zero));
		receive_byte (unchecked((byte) min));
		receive_byte (unchecked((byte) max));
		Console.WriteLine ("   arg: char (long)");

		receive_char (unchecked((char) zero));
		receive_char (unchecked((char) min));
		receive_char (unchecked((char) max));
	}

	static void probe_ulong()
	{
		ulong zero = (ulong) 0;
		ulong min = (ulong) ulong.MinValue;
		ulong max = (ulong) ulong.MaxValue;

		Console.WriteLine ("Testing: ulong");

		Console.WriteLine ("   arg: int (ulong)");

		receive_int (unchecked((int) zero));
		receive_int (unchecked((int) min));
		receive_int (unchecked((int) max));
		Console.WriteLine ("   arg: uint (ulong)");

		receive_uint (unchecked((uint) zero));
		receive_uint (unchecked((uint) min));
		receive_uint (unchecked((uint) max));
		Console.WriteLine ("   arg: short (ulong)");

		receive_short (unchecked((short) zero));
		receive_short (unchecked((short) min));
		receive_short (unchecked((short) max));
		Console.WriteLine ("   arg: ushort (ulong)");

		receive_ushort (unchecked((ushort) zero));
		receive_ushort (unchecked((ushort) min));
		receive_ushort (unchecked((ushort) max));
		Console.WriteLine ("   arg: long (ulong)");

		receive_long (unchecked((long) zero));
		receive_long (unchecked((long) min));
		receive_long (unchecked((long) max));
		Console.WriteLine ("   arg: ulong (ulong)");

		receive_ulong (unchecked((ulong) zero));
		receive_ulong (unchecked((ulong) min));
		receive_ulong (unchecked((ulong) max));
		Console.WriteLine ("   arg: sbyte (ulong)");

		receive_sbyte (unchecked((sbyte) zero));
		receive_sbyte (unchecked((sbyte) min));
		receive_sbyte (unchecked((sbyte) max));
		Console.WriteLine ("   arg: byte (ulong)");

		receive_byte (unchecked((byte) zero));
		receive_byte (unchecked((byte) min));
		receive_byte (unchecked((byte) max));
		Console.WriteLine ("   arg: char (ulong)");

		receive_char (unchecked((char) zero));
		receive_char (unchecked((char) min));
		receive_char (unchecked((char) max));
	}

	static void probe_sbyte()
	{
		sbyte zero = (sbyte) 0;
		sbyte min = (sbyte) sbyte.MinValue;
		sbyte max = (sbyte) sbyte.MaxValue;

		Console.WriteLine ("Testing: sbyte");

		Console.WriteLine ("   arg: int (sbyte)");

		receive_int (unchecked((int) zero));
		receive_int (unchecked((int) min));
		receive_int (unchecked((int) max));
		Console.WriteLine ("   arg: uint (sbyte)");

		receive_uint (unchecked((uint) zero));
		receive_uint (unchecked((uint) min));
		receive_uint (unchecked((uint) max));
		Console.WriteLine ("   arg: short (sbyte)");

		receive_short (unchecked((short) zero));
		receive_short (unchecked((short) min));
		receive_short (unchecked((short) max));
		Console.WriteLine ("   arg: ushort (sbyte)");

		receive_ushort (unchecked((ushort) zero));
		receive_ushort (unchecked((ushort) min));
		receive_ushort (unchecked((ushort) max));
		Console.WriteLine ("   arg: long (sbyte)");

		receive_long (unchecked((long) zero));
		receive_long (unchecked((long) min));
		receive_long (unchecked((long) max));
		Console.WriteLine ("   arg: ulong (sbyte)");

		receive_ulong (unchecked((ulong) zero));
		receive_ulong (unchecked((ulong) min));
		receive_ulong (unchecked((ulong) max));
		Console.WriteLine ("   arg: sbyte (sbyte)");

		receive_sbyte (unchecked((sbyte) zero));
		receive_sbyte (unchecked((sbyte) min));
		receive_sbyte (unchecked((sbyte) max));
		Console.WriteLine ("   arg: byte (sbyte)");

		receive_byte (unchecked((byte) zero));
		receive_byte (unchecked((byte) min));
		receive_byte (unchecked((byte) max));
		Console.WriteLine ("   arg: char (sbyte)");

		receive_char (unchecked((char) zero));
		receive_char (unchecked((char) min));
		receive_char (unchecked((char) max));
	}

	static void probe_byte()
	{
		byte zero = (byte) 0;
		byte min = (byte) byte.MinValue;
		byte max = (byte) byte.MaxValue;

		Console.WriteLine ("Testing: byte");

		Console.WriteLine ("   arg: int (byte)");

		receive_int (unchecked((int) zero));
		receive_int (unchecked((int) min));
		receive_int (unchecked((int) max));
		Console.WriteLine ("   arg: uint (byte)");

		receive_uint (unchecked((uint) zero));
		receive_uint (unchecked((uint) min));
		receive_uint (unchecked((uint) max));
		Console.WriteLine ("   arg: short (byte)");

		receive_short (unchecked((short) zero));
		receive_short (unchecked((short) min));
		receive_short (unchecked((short) max));
		Console.WriteLine ("   arg: ushort (byte)");

		receive_ushort (unchecked((ushort) zero));
		receive_ushort (unchecked((ushort) min));
		receive_ushort (unchecked((ushort) max));
		Console.WriteLine ("   arg: long (byte)");

		receive_long (unchecked((long) zero));
		receive_long (unchecked((long) min));
		receive_long (unchecked((long) max));
		Console.WriteLine ("   arg: ulong (byte)");

		receive_ulong (unchecked((ulong) zero));
		receive_ulong (unchecked((ulong) min));
		receive_ulong (unchecked((ulong) max));
		Console.WriteLine ("   arg: sbyte (byte)");

		receive_sbyte (unchecked((sbyte) zero));
		receive_sbyte (unchecked((sbyte) min));
		receive_sbyte (unchecked((sbyte) max));
		Console.WriteLine ("   arg: byte (byte)");

		receive_byte (unchecked((byte) zero));
		receive_byte (unchecked((byte) min));
		receive_byte (unchecked((byte) max));
		Console.WriteLine ("   arg: char (byte)");

		receive_char (unchecked((char) zero));
		receive_char (unchecked((char) min));
		receive_char (unchecked((char) max));
	}

	static void probe_char()
	{
		char zero = (char) 0;
		char min = (char) char.MinValue;
		char max = (char) char.MaxValue;

		Console.WriteLine ("Testing: char");

		Console.WriteLine ("   arg: int (char)");

		receive_int (unchecked((int) zero));
		receive_int (unchecked((int) min));
		receive_int (unchecked((int) max));
		Console.WriteLine ("   arg: uint (char)");

		receive_uint (unchecked((uint) zero));
		receive_uint (unchecked((uint) min));
		receive_uint (unchecked((uint) max));
		Console.WriteLine ("   arg: short (char)");

		receive_short (unchecked((short) zero));
		receive_short (unchecked((short) min));
		receive_short (unchecked((short) max));
		Console.WriteLine ("   arg: ushort (char)");

		receive_ushort (unchecked((ushort) zero));
		receive_ushort (unchecked((ushort) min));
		receive_ushort (unchecked((ushort) max));
		Console.WriteLine ("   arg: long (char)");

		receive_long (unchecked((long) zero));
		receive_long (unchecked((long) min));
		receive_long (unchecked((long) max));
		Console.WriteLine ("   arg: ulong (char)");

		receive_ulong (unchecked((ulong) zero));
		receive_ulong (unchecked((ulong) min));
		receive_ulong (unchecked((ulong) max));
		Console.WriteLine ("   arg: sbyte (char)");

		receive_sbyte (unchecked((sbyte) zero));
		receive_sbyte (unchecked((sbyte) min));
		receive_sbyte (unchecked((sbyte) max));
		Console.WriteLine ("   arg: byte (char)");

		receive_byte (unchecked((byte) zero));
		receive_byte (unchecked((byte) min));
		receive_byte (unchecked((byte) max));
		Console.WriteLine ("   arg: char (char)");

		receive_char (unchecked((char) zero));
		receive_char (unchecked((char) min));
		receive_char (unchecked((char) max));
	}

	static void Main ()
	{
		probe_int ();
		probe_uint ();
		probe_short ();
		probe_ushort ();
		probe_long ();
		probe_ulong ();
		probe_sbyte ();
		probe_byte ();
		probe_char ();
	}
}

