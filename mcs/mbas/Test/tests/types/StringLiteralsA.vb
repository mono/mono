Imports System
Module StringLiteral
	Sub Main()
			Dim a As String="Hello"
			Dim b As String=" World "
			Dim c As String=47
			Dim d As String=a+b+c
			If d<>"Hello World 47" Then
				Throw New Exception("StringLiteralA:Failed-String concatenation does not work right")
			End If
	End Sub
End Module
