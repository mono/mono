REM LineNo: 11
REM ExpectedError: BC30311
REM ErrorMessage: Value of type 'Char' cannot be converted to 'Boolean'

Imports System

Module LoopStatementsC4

    Sub main()

        Do While "H"c
            Console.WriteLine("Hello World")
        Loop

    End Sub

End Module