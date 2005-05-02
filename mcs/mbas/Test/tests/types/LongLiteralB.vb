Imports System
Module LongLiteral
	Sub Main()
		Dim a As Long
		If a<>0 Then
			throw new System.Exception("LongLiteralC:Failed-Default value assigned to long variable should be 0")
		End If
	End Sub
End Module
