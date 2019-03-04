.assembly extern mscorlib
{
  .ver 2:0:0:0
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 ) // .z\V.4..
}
.assembly 'dtest-lib' { }

.class public auto ansi Tests
       extends [mscorlib]System.Object
{
	.method public static void Main (string[] args) cil managed
	{
		.entrypoint
		call void class Tests::Test ()
		ret
	}

	.method public static int32 Filter ([mscorlib]System.Exception exc) cil managed noinlining
	{
		ldc.i4.1
		ret
	}

	.method public static void Handler ([mscorlib]System.Exception exc) cil managed noinlining
	{
		ret
	}

	.method public static void Test () cil managed noinlining
	{
		.try {
			newobj instance void class [mscorlib]System.InvalidOperationException::.ctor ()
			throw

			leave end
		} filter {
		    castclass [mscorlib]System.Exception
			call int32 class Tests::Filter([mscorlib]System.Exception)
			endfilter
		} {
		    castclass [mscorlib]System.Exception
			call void class Tests::Handler([mscorlib]System.Exception)
			leave end
		}

	end:
		ret
	}
}
