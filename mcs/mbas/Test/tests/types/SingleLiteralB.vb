Imports System
Module SingleLiteral
	Sub Main()
			Dim a As Single=True
			If a<>-1 Then
                        throw new System.Exception("SingleLiteralB:Failed")
                  End If
	End Sub
End Module
