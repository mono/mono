REM LineNo: 13
REM ExpectedError: BC30205
REM ErrorMessage: End of statement expected.

Imports System

Module LoopStatementsC3

    Sub main()

        Dim i As Integer = 0
        Do Until 1 _
            i = 0
            Console.WriteLine("Hello World")
        Loop

    End Sub

End Module