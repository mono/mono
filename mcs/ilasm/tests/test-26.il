

.assembly extern mscorlib {

}


.class public Test {

       .method public static void main ()
       {
                .entrypoint
                
                try_begin:
                        ldstr "BEGIN TRY"
                        call void [mscorlib]System.Console::WriteLine (string)
                        
                	leave try_end
                exc_handler:
                        ldstr "IN HANDLER"
                        call void [mscorlib]System.Console::WriteLine (string)

                        leave try_end
                try_end:
                        ldstr "END TRY"
                        call void [mscorlib]System.Console::WriteLine (string)
                        
                .try try_begin to try_end catch [mscorlib]System.Exception handler exc_handler to try_end

       }

}

