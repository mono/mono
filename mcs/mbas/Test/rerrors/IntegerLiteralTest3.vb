Imports System
Module IntegerLiteralTest3
    Sub Main()
        Try
            Dim l As Long
            l = System.Int64.MaxValue
		Console.WriteLine(l)
            l = l + 1
        Catch e As Exception
            Console.WriteLine(e.Message)
        End Try
    End Sub
End Module
