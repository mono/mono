//Test for keywords : uint8, uint16, uint32 & uint64
//Should get parsed as unsigned int8 .. etc
//
//uint8 in marshal (..) should get parsed as native types

.assembly Test
{
}

.namespace MonoTest
{
	.class private auto autochar beforefieldinit ConsoleApp
		extends [mscorlib]System.Object
		{
    			.field  private  marshal (uint8) int32 a0
    			.field  private  marshal (uint16) int32 a1
    			.field  private  marshal (uint32) int32 a2
    			.field  private  marshal (uint64) int64 a3

			.method assembly hidebysig static void Test() cil managed
			{
				// Code size       74 (0x4a)
				.maxstack  2

				.locals init (uint8 V_0,
						uint16 V_1,
						uint32 V_2,
						uint64 V_3)
				ret
			}
		}
}
