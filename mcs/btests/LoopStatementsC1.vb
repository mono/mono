REM LineNo: 11
REM ExpectedError: BC30201
REM ErrorMessage: Expression expected.

Imports System

Module LoopStatementsC1

    Sub main()

        Do While
            Console.WriteLine("Hello World")
        Loop

    End Sub

End Module