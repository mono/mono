REM LineNo: 11
REM ExpectedError: BC30083
REM ErrorMessage: 'Do' must end with a matching 'Loop'.

Imports System

Module LoopStatementsC6

    Sub main()

        Do Until 1
            Console.WriteLine("Hello World")

    End Sub

End Module