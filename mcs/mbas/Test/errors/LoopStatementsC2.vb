REM LineNo: 13
REM ExpectedError: BC30238
REM ErrorMessage: 'Loop' cannot have condition if matching 'Do' has one.

Imports System

Module LoopStatementsC2

    Sub main()

        Do While 1
            Console.WriteLine("Hello World")
        Loop While 1

    End Sub

End Module