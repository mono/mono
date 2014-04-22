.assembly extern mscorlib
{
}

.assembly 'test-883'
{
  .publickey = (00 24 00 00 04 80 00 00 94 00 00 00 06 02 00 00   // .$..............
                00 24 00 00 52 53 41 31 00 04 00 00 01 00 01 00   // .$..RSA1........
                79 15 99 77 D2 D0 3A 8E 6B EA 7A 2E 74 E8 D1 AF   // y..w..:.k.z.t...
                CC 93 E8 85 19 74 95 2B B4 80 A1 2C 91 34 47 4D   // .....t.+...,.4GM
                04 06 24 47 C3 7E 0E 68 C0 80 53 6F CF 3C 3F BE   // ..$G.~.h..So.<?.
                2F F9 C9 79 CE 99 84 75 E5 06 E8 CE 82 DD 5B 0F   // /..y...u......[.
                35 0D C1 0E 93 BF 2E EE CF 87 4B 24 77 0C 50 81   // 5.........K$w.P.
                DB EA 74 47 FD DA FA 27 7B 22 DE 47 D6 FF EA 44   // ..tG...'{".G...D
                96 74 A4 F9 FC CF 84 D1 50 69 08 93 80 28 4D BD   // .t......Pi...(M.
                D3 5F 46 CD FF 12 A1 BD 78 E4 EF 00 65 D0 16 DF ) // ._F.....x...e...
  .hash algorithm 0x00008004
  .ver 2:0:0:0
}
.module 'test-883.dll'

.class public auto ansi sealed E
       extends [mscorlib]System.Enum
{
  .field public specialname rtspecialname int32 value__
  .field public static literal valuetype E TestField = int32(0x00000003)
}
