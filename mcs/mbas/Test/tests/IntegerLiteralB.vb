Imports System
Module IntegerLiteral
	Sub Main()
	Try
		Dim a As Integer
		If a<>0 Then
			Console.WriteLine("IntegerLiteralC:Failed-Default value assigned to integer variable should be 0")
		End If
	Catch e As Exception
		Console.WriteLine(e.Message)
	End Try
	End Sub
End Module
