//
// Test a generic type type param field
//

.assembly extern mscorlib { }


.namespace Mono.ILASM.Tests {

.class public Covariant<+T> extends [mscorlib]System.Object {

	.field public !0 val

	.method public instance void .ctor () {
		ret
	}
}

.class public Contravariant<-T> extends [mscorlib]System.Object {

	.field public !0 val

	.method public instance void .ctor () {
		ret
	}
}

.class public Driver extends [mscorlib]System.Object {

	.method public static void Main () {
		.entrypoint
		newobj instance void class Mono.ILASM.Tests.Covariant<string>::.ctor ()
		castclass class Mono.ILASM.Tests.Covariant<object>
		call void [mscorlib]System.Console::WriteLine (object)

		newobj instance void class Mono.ILASM.Tests.Covariant<object>::.ctor ()
		castclass class Mono.ILASM.Tests.Covariant<string>
		call void [mscorlib]System.Console::WriteLine (object)
		
       	ret
	}
}
}