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

		Console.WriteLine ("   arg: int");

		receive_int (unchecked ((int) zero));
		receive_int (unchecked ((int) min));
		receive_int (unchecked ((int) max));
		Console.WriteLine ("   arg: uint");

		receive_uint (unchecked ((uint) zero));
		receive_uint (unchecked ((uint) min));
		receive_uint (unchecked ((uint) max));
		Console.WriteLine ("   arg: short");

		receive_short (unchecked ((short) zero));
		receive_short (unchecked ((short) min));
		receive_short (unchecked ((short) max));
		Console.WriteLine ("   arg: ushort");

		receive_ushort (unchecked ((ushort) zero));
		receive_ushort (unchecked ((ushort) min));
		receive_ushort (unchecked ((ushort) max));
		Console.WriteLine ("   arg: long");

		receive_long (unchecked ((long) zero));
		receive_long (unchecked ((long) min));
		receive_long (unchecked ((long) max));
		Console.WriteLine ("   arg: ulong");

		receive_ulong (unchecked ((ulong) zero));
		receive_ulong (unchecked ((ulong) min));
		receive_ulong (unchecked ((ulong) max));
		Console.WriteLine ("   arg: sbyte");

		receive_sbyte (unchecked ((sbyte) zero));
		receive_sbyte (unchecked ((sbyte) min));
		receive_sbyte (unchecked ((sbyte) max));
		Console.WriteLine ("   arg: byte");

		receive_byte (unchecked ((byte) zero));
		receive_byte (unchecked ((byte) min));
		receive_byte (unchecked ((byte) max));
		Console.WriteLine ("   arg: char");

		receive_char (unchecked ((char) zero));
		receive_char (unchecked ((char) min));
		receive_char (unchecked ((char) max));
	}

	static void probe_uint()
	{
		uint zero = (uint) 0;
		uint min = (uint) uint.MinValue;
		uint max = (uint) uint.MaxValue;

		Console.WriteLine ("Testing: uint");

		Console.WriteLine ("   arg: int");

		receive_int (unchecked ((int) zero));
		receive_int (unchecked ((int) min));
		receive_int (unchecked ((int) max));
		Console.WriteLine ("   arg: uint");

		receive_uint (unchecked ((uint) zero));
		receive_uint (unchecked ((uint) min));
		receive_uint (unchecked ((uint) max));
		Console.WriteLine ("   arg: short");

		receive_short (unchecked ((short) zero));
		receive_short (unchecked ((short) min));
		receive_short (unchecked ((short) max));
		Console.WriteLine ("   arg: ushort");

		receive_ushort (unchecked ((ushort) zero));
		receive_ushort (unchecked ((ushort) min));
		receive_ushort (unchecked ((ushort) max));
		Console.WriteLine ("   arg: long");

		receive_long (unchecked ((long) zero));
		receive_long (unchecked ((long) min));
		receive_long (unchecked ((long) max));
		Console.WriteLine ("   arg: ulong");

		receive_ulong (unchecked ((ulong) zero));
		receive_ulong (unchecked ((ulong) min));
		receive_ulong (unchecked ((ulong) max));
		Console.WriteLine ("   arg: sbyte");

		receive_sbyte (unchecked ((sbyte) zero));
		receive_sbyte (unchecked ((sbyte) min));
		receive_sbyte (unchecked ((sbyte) max));
		Console.WriteLine ("   arg: byte");

		receive_byte (unchecked ((byte) zero));
		receive_byte (unchecked ((byte) min));
		receive_byte (unchecked ((byte) max));
		Console.WriteLine ("   arg: char");

		receive_char (unchecked ((char) zero));
		receive_char (unchecked ((char) min));
		receive_char (unchecked ((char) max));
	}

	static void probe_short()
	{
		short zero = (short) 0;
		short min = (short) short.MinValue;
		short max = (short) short.MaxValue;

		Console.WriteLine ("Testing: short");

		Console.WriteLine ("   arg: int");

		receive_int (unchecked ((int) zero));
		receive_int (unchecked ((int) min));
		receive_int (unchecked ((int) max));
		Console.WriteLine ("   arg: uint");

		receive_uint (unchecked ((uint) zero));
		receive_uint (unchecked ((uint) min));
		receive_uint (unchecked ((uint) max));
		Console.WriteLine ("   arg: short");

		receive_short (unchecked ((short) zero));
		receive_short (unchecked ((short) min));
		receive_short (unchecked ((short) max));
		Console.WriteLine ("   arg: ushort");

		receive_ushort (unchecked ((ushort) zero));
		receive_ushort (unchecked ((ushort) min));
		receive_ushort (unchecked ((ushort) max));
		Console.WriteLine ("   arg: long");

		receive_long (unchecked ((long) zero));
		receive_long (unchecked ((long) min));
		receive_long (unchecked ((long) max));
		Console.WriteLine ("   arg: ulong");

		receive_ulong (unchecked ((ulong) zero));
		receive_ulong (unchecked ((ulong) min));
		receive_ulong (unchecked ((ulong) max));
		Console.WriteLine ("   arg: sbyte");

		receive_sbyte (unchecked ((sbyte) zero));
		receive_sbyte (unchecked ((sbyte) min));
		receive_sbyte (unchecked ((sbyte) max));
		Console.WriteLine ("   arg: byte");

		receive_byte (unchecked ((byte) zero));
		receive_byte (unchecked ((byte) min));
		receive_byte (unchecked ((byte) max));
		Console.WriteLine ("   arg: char");

		receive_char (unchecked ((char) zero));
		receive_char (unchecked ((char) min));
		receive_char (unchecked ((char) max));
	}

	static void probe_ushort()
	{
		ushort zero = (ushort) 0;
		ushort min = (ushort) ushort.MinValue;
		ushort max = (ushort) ushort.MaxValue;

		Console.WriteLine ("Testing: ushort");

		Console.WriteLine ("   arg: int");

		receive_int (unchecked ((int) zero));
		receive_int (unchecked ((int) min));
		receive_int (unchecked ((int) max));
		Console.WriteLine ("   arg: uint");

		receive_uint (unchecked ((uint) zero));
		receive_uint (unchecked ((uint) min));
		receive_uint (unchecked ((uint) max));
		Console.WriteLine ("   arg: short");

		receive_short (unchecked ((short) zero));
		receive_short (unchecked ((short) min));
		receive_short (unchecked ((short) max));
		Console.WriteLine ("   arg: ushort");

		receive_ushort (unchecked ((ushort) zero));
		receive_ushort (unchecked ((ushort) min));
		receive_ushort (unchecked ((ushort) max));
		Console.WriteLine ("   arg: long");

		receive_long (unchecked ((long) zero));
		receive_long (unchecked ((long) min));
		receive_long (unchecked ((long) max));
		Console.WriteLine ("   arg: ulong");

		receive_ulong (unchecked ((ulong) zero));
		receive_ulong (unchecked ((ulong) min));
		receive_ulong (unchecked ((ulong) max));
		Console.WriteLine ("   arg: sbyte");

		receive_sbyte (unchecked ((sbyte) zero));
		receive_sbyte (unchecked ((sbyte) min));
		receive_sbyte (unchecked ((sbyte) max));
		Console.WriteLine ("   arg: byte");

		receive_byte (unchecked ((byte) zero));
		receive_byte (unchecked ((byte) min));
		receive_byte (unchecked ((byte) max));
		Console.WriteLine ("   arg: char");

		receive_char (unchecked ((char) zero));
		receive_char (unchecked ((char) min));
		receive_char (unchecked ((char) max));
	}

	static void probe_long()
	{
		long zero = (long) 0;
		long min = (long) long.MinValue;
		long max = (long) long.MaxValue;

		Console.WriteLine ("Testing: long");

		Console.WriteLine ("   arg: int");

		receive_int (unchecked ((int) zero));
		receive_int (unchecked ((int) min));
		receive_int (unchecked ((int) max));
		Console.WriteLine ("   arg: uint");

		receive_uint (unchecked ((uint) zero));
		receive_uint (unchecked ((uint) min));
		receive_uint (unchecked ((uint) max));
		Console.WriteLine ("   arg: short");

		receive_short (unchecked ((short) zero));
		receive_short (unchecked ((short) min));
		receive_short (unchecked ((short) max));
		Console.WriteLine ("   arg: ushort");

		receive_ushort (unchecked ((ushort) zero));
		receive_ushort (unchecked ((ushort) min));
		receive_ushort (unchecked ((ushort) max));
		Console.WriteLine ("   arg: long");

		receive_long (unchecked ((long) zero));
		receive_long (unchecked ((long) min));
		receive_long (unchecked ((long) max));
		Console.WriteLine ("   arg: ulong");

		receive_ulong (unchecked ((ulong) zero));
		receive_ulong (unchecked ((ulong) min));
		receive_ulong (unchecked ((ulong) max));
		Console.WriteLine ("   arg: sbyte");

		receive_sbyte (unchecked ((sbyte) zero));
		receive_sbyte (unchecked ((sbyte) min));
		receive_sbyte (unchecked ((sbyte) max));
		Console.WriteLine ("   arg: byte");

		receive_byte (unchecked ((byte) zero));
		receive_byte (unchecked ((byte) min));
		receive_byte (unchecked ((byte) max));
		Console.WriteLine ("   arg: char");

		receive_char (unchecked ((char) zero));
		receive_char (unchecked ((char) min));
		receive_char (unchecked ((char) max));
	}

	static void probe_ulong()
	{
		ulong zero = (ulong) 0;
		ulong min = (ulong) ulong.MinValue;
		ulong max = (ulong) ulong.MaxValue;

		Console.WriteLine ("Testing: ulong");

		Console.WriteLine ("   arg: int");

		receive_int (unchecked ((int) zero));
		receive_int (unchecked ((int) min));
		receive_int (unchecked ((int) max));
		Console.WriteLine ("   arg: uint");

		receive_uint (unchecked ((uint) zero));
		receive_uint (unchecked ((uint) min));
		receive_uint (unchecked ((uint) max));
		Console.WriteLine ("   arg: short");

		receive_short (unchecked ((short) zero));
		receive_short (unchecked ((short) min));
		receive_short (unchecked ((short) max));
		Console.WriteLine ("   arg: ushort");

		receive_ushort (unchecked ((ushort) zero));
		receive_ushort (unchecked ((ushort) min));
		receive_ushort (unchecked ((ushort) max));
		Console.WriteLine ("   arg: long");

		receive_long (unchecked ((long) zero));
		receive_long (unchecked ((long) min));
		receive_long (unchecked ((long) max));
		Console.WriteLine ("   arg: ulong");

		receive_ulong (unchecked ((ulong) zero));
		receive_ulong (unchecked ((ulong) min));
		receive_ulong (unchecked ((ulong) max));
		Console.WriteLine ("   arg: sbyte");

		receive_sbyte (unchecked ((sbyte) zero));
		receive_sbyte (unchecked ((sbyte) min));
		receive_sbyte (unchecked ((sbyte) max));
		Console.WriteLine ("   arg: byte");

		receive_byte (unchecked ((byte) zero));
		receive_byte (unchecked ((byte) min));
		receive_byte (unchecked ((byte) max));
		Console.WriteLine ("   arg: char");

		receive_char (unchecked ((char) zero));
		receive_char (unchecked ((char) min));
		receive_char (unchecked ((char) max));
	}

	static void probe_sbyte()
	{
		sbyte zero = (sbyte) 0;
		sbyte min = (sbyte) sbyte.MinValue;
		sbyte max = (sbyte) sbyte.MaxValue;

		Console.WriteLine ("Testing: sbyte");

		Console.WriteLine ("   arg: int");

		receive_int (unchecked ((int) zero));
		receive_int (unchecked ((int) min));
		receive_int (unchecked ((int) max));
		Console.WriteLine ("   arg: uint");

		receive_uint (unchecked ((uint) zero));
		receive_uint (unchecked ((uint) min));
		receive_uint (unchecked ((uint) max));
		Console.WriteLine ("   arg: short");

		receive_short (unchecked ((short) zero));
		receive_short (unchecked ((short) min));
		receive_short (unchecked ((short) max));
		Console.WriteLine ("   arg: ushort");

		receive_ushort (unchecked ((ushort) zero));
		receive_ushort (unchecked ((ushort) min));
		receive_ushort (unchecked ((ushort) max));
		Console.WriteLine ("   arg: long");

		receive_long (unchecked ((long) zero));
		receive_long (unchecked ((long) min));
		receive_long (unchecked ((long) max));
		Console.WriteLine ("   arg: ulong");

		receive_ulong (unchecked ((ulong) zero));
		receive_ulong (unchecked ((ulong) min));
		receive_ulong (unchecked ((ulong) max));
		Console.WriteLine ("   arg: sbyte");

		receive_sbyte (unchecked ((sbyte) zero));
		receive_sbyte (unchecked ((sbyte) min));
		receive_sbyte (unchecked ((sbyte) max));
		Console.WriteLine ("   arg: byte");

		receive_byte (unchecked ((byte) zero));
		receive_byte (unchecked ((byte) min));
		receive_byte (unchecked ((byte) max));
		Console.WriteLine ("   arg: char");

		receive_char (unchecked ((char) zero));
		receive_char (unchecked ((char) min));
		receive_char (unchecked ((char) max));
	}

	static void probe_byte()
	{
		byte zero = (byte) 0;
		byte min = (byte) byte.MinValue;
		byte max = (byte) byte.MaxValue;

		Console.WriteLine ("Testing: byte");

		Console.WriteLine ("   arg: int");

		receive_int (unchecked ((int) zero));
		receive_int (unchecked ((int) min));
		receive_int (unchecked ((int) max));
		Console.WriteLine ("   arg: uint");

		receive_uint (unchecked ((uint) zero));
		receive_uint (unchecked ((uint) min));
		receive_uint (unchecked ((uint) max));
		Console.WriteLine ("   arg: short");

		receive_short (unchecked ((short) zero));
		receive_short (unchecked ((short) min));
		receive_short (unchecked ((short) max));
		Console.WriteLine ("   arg: ushort");

		receive_ushort (unchecked ((ushort) zero));
		receive_ushort (unchecked ((ushort) min));
		receive_ushort (unchecked ((ushort) max));
		Console.WriteLine ("   arg: long");

		receive_long (unchecked ((long) zero));
		receive_long (unchecked ((long) min));
		receive_long (unchecked ((long) max));
		Console.WriteLine ("   arg: ulong");

		receive_ulong (unchecked ((ulong) zero));
		receive_ulong (unchecked ((ulong) min));
		receive_ulong (unchecked ((ulong) max));
		Console.WriteLine ("   arg: sbyte");

		receive_sbyte (unchecked ((sbyte) zero));
		receive_sbyte (unchecked ((sbyte) min));
		receive_sbyte (unchecked ((sbyte) max));
		Console.WriteLine ("   arg: byte");

		receive_byte (unchecked ((byte) zero));
		receive_byte (unchecked ((byte) min));
		receive_byte (unchecked ((byte) max));
		Console.WriteLine ("   arg: char");

		receive_char (unchecked ((char) zero));
		receive_char (unchecked ((char) min));
		receive_char (unchecked ((char) max));
	}

	static void probe_char()
	{
		char zero = (char) 0;
		char min = (char) char.MinValue;
		char max = (char) char.MaxValue;

		Console.WriteLine ("Testing: char");

		Console.WriteLine ("   arg: int");

		receive_int (unchecked ((int) zero));
		receive_int (unchecked ((int) min));
		receive_int (unchecked ((int) max));
		Console.WriteLine ("   arg: uint");

		receive_uint (unchecked ((uint) zero));
		receive_uint (unchecked ((uint) min));
		receive_uint (unchecked ((uint) max));
		Console.WriteLine ("   arg: short");

		receive_short (unchecked ((short) zero));
		receive_short (unchecked ((short) min));
		receive_short (unchecked ((short) max));
		Console.WriteLine ("   arg: ushort");

		receive_ushort (unchecked ((ushort) zero));
		receive_ushort (unchecked ((ushort) min));
		receive_ushort (unchecked ((ushort) max));
		Console.WriteLine ("   arg: long");

		receive_long (unchecked ((long) zero));
		receive_long (unchecked ((long) min));
		receive_long (unchecked ((long) max));
		Console.WriteLine ("   arg: ulong");

		receive_ulong (unchecked ((ulong) zero));
		receive_ulong (unchecked ((ulong) min));
		receive_ulong (unchecked ((ulong) max));
		Console.WriteLine ("   arg: sbyte");

		receive_sbyte (unchecked ((sbyte) zero));
		receive_sbyte (unchecked ((sbyte) min));
		receive_sbyte (unchecked ((sbyte) max));
		Console.WriteLine ("   arg: byte");

		receive_byte (unchecked ((byte) zero));
		receive_byte (unchecked ((byte) min));
		receive_byte (unchecked ((byte) max));
		Console.WriteLine ("   arg: char");

		receive_char (unchecked ((char) zero));
		receive_char (unchecked ((char) min));
		receive_char (unchecked ((char) max));
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

