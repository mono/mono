REM LineNo: 11
REM ExpectedError: BC30082
REM ErrorMessage: 'While' must end with a matching 'End While'

Imports System

Module LoopStatementsC7

    Sub main()

        While 1
            Console.WriteLine("Hello World")

    End Sub

End Module