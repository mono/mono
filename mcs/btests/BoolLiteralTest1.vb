Module BoolLiteralTest1
    Sub Main()
        Try
            Dim b As Boolean
            b = NotTrue
        Catch e As Exception
            Console.WriteLine(e.Message)
        End Try
    End Sub
End Module
