Imports System
Module IntegerLiteral
	Sub Main()
		Try
			Dim a As Integer
			a="Hello"
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try
	End Sub
End Module
