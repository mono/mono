Imports System
Module IntegerLiteral
	Sub Main()
		'Assigning boolean to integer
		Dim a As Integer
		a=True
		If a<>-1 Then
			Throw new System.Exception("IntegerLiteralA:Failed")
		End If
		' Assigning float to integer
		' if option strict is off this 
		' Test case should pass
                a=1.23
                If a<>1 Then
                        Throw new System.Exception("IntegerLiteralA:Failed")
                End If
	End Sub
End Module
