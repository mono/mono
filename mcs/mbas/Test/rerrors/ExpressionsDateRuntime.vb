Imports System
Module ExpressionOperator1

   Sub main()
		Dim SomeDate1 As Date = #2/10/2005 11:11 PM#
		Dim SomeDate2 As Date = #2/11/2005 12:12 PM#
		if SomeDate1 > SomeDate2  Then
		Throw New Exception ("# Error D1 can't be equal to D2 ")
		End if
    End Sub
End Module

