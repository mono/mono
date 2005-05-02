Imports System
Module IntegerLiteral
	Sub Main()
		Dim a As Integer
		If a<>0 Then
			throw new System.Exception("IntegerLiteralC:Failed-Default value assigned to integer variable should be 0")
		End If
	End Sub
End Module
