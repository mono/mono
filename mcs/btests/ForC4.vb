REM LineNo: 12
REM ExpectedError: BC30084
REM ErrorMessage: 'For' must end with a matching 'Next'

Imports System

Module ForC4

    Sub main()

        Dim i As Integer
        For i = 0 To 10
            Console.WriteLine("Hello World")

    End Sub

End Module