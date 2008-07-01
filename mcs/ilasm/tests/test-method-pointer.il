//From bug: #405383

.assembly 'test-method-pointer'
{
}
.module 'test-method-pointer.dll'

.class public auto ansi Test extends [mscorlib]System.Object {

	// field referencing a method pointer with no params
	.field private static method void  * () * 'p?fs1515'

	.field private static method void  * (int32, uint8) * mptr
}
