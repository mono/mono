Imports System
Module IntegerLiteralTestC4
    Sub Main()
        Try
	    Dim A as Short
            Dim i As Short
            i = A
        Catch e As Exception
            Console.WriteLine(e.Message)
        End Try
    End Sub
End Module
