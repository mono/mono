REM LineNo: 12
REM ExpectedError: BC30091
REM ErrorMessage: 'Loop' must be preceded by a matching 'Do'

Imports System

Module LoopStatementsC5

    Sub main()

        Console.WriteLine("Hello World")
        Loop While 1

    End Sub

End Module