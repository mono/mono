Imports System
Module BoolLiteralTest
    Sub main()
        Try
            Dim b As Boolean
            b = True
            b = False
        Catch e As Exception
            Console.WriteLine(e.Message)
        End Try
    End Sub
End Module
