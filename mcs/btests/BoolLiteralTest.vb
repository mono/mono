Imports System
Module BoolLiteralTest
    Sub Main()
        Try
            Dim b As Boolean
            b = True
            b = False
        Catch e As Exception
            Console.WriteLine(e.Message)
        End Try
    End Sub
End Module
