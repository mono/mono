Imports System
Module LongLiteral
	Sub Main()
	Try
		Dim a As Long
		If a<>0 Then
			Console.WriteLine("LongLiteralC:Failed-Default value assigned to long variable should be 0")
		End If
	Catch e As Exception
		Console.WriteLine(e.Message)
	End Try
	End Sub
End Module
