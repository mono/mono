Imports System

Module BoolLiteralTest1
    Sub Main()
        Try
            Dim b As Boolean
            b = Not True
        Catch e As Exception
            Console.WriteLine(e.Message)
        End Try
    End Sub
End Module
