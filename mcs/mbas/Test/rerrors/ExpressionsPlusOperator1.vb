Imports System
Module ExpressionOperator1

   Sub main()
		Dim a As Integer = 5
		Dim b As Integer = 6
		a += 1
		if a <> b Then
		Throw New Exception ("# <> operators: Failed")
		End if
    End Sub
End Module
