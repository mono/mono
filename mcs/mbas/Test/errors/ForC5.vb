REM LineNo: 13
REM ExpectedError: BC30092
REM ErrorMessage: 'Next' must be preceded by a matching 'For'

Imports System

Module ForC5

    Sub main()
        Dim i As Integer

        Console.WriteLine("Hello World")
        Next
    End Sub

End Module