class Test
{
    public enum Ebyte : byte
    {
        Mask = 1
    }

    public enum Esbyte : sbyte
    {
        Mask = -127
    }

    public enum Eshort : short
    {
        Mask = 1
    }

    public enum Eushort : ushort
    {
        Mask = 1
    }

    static void Main ()
    {
        byte v1 = (byte)(~Ebyte.Mask);
        sbyte v2 = (sbyte)(~Esbyte.Mask);

        short v3 = (short)(~Eshort.Mask);
        ushort v4 = (ushort)(~Eushort.Mask);
    }
}