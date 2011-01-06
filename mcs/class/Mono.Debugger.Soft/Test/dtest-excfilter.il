.assembly extern mscorlib
{
  .ver 2:0:0:0
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 ) // .z\V.4..
}
.assembly 'dtest-lib' { }

.class public auto ansi ExceptionFilterTest
       extends [mscorlib]System.Object
{
	.method public static void Main () cil managed
	{
		.entrypoint
		call void class ExceptionFilterTest::Test ()
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
			call int32 class ExceptionFilterTest::Filter([mscorlib]System.Exception)
			endfilter
		} {
			call void class ExceptionFilterTest::Handler([mscorlib]System.Exception)
			leave end
		}

	end:
		ret
	}
}
