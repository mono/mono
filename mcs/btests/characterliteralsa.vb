Imports System
Module CharacterLiteral
	Sub Main()
		Try
			Dim a As Char=""""c
			If a<>""""c
				Throw New Exception("CharacterLiteralA: Failed")
			End If
			
		Catch e As Exception
		End Try
	End Sub
End Module
