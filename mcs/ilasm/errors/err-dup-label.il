// Test for duplicate labels

.assembly extern mscorlib { }

.class public T {

       .method public static void main ()
       {
                .entrypoint

		IL_0001:   ldstr      "Hello"
		IL_0001:   call       void [mscorlib]System.Console::WriteLine(string)
		ret
       }

}

