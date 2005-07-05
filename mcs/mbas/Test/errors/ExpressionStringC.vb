REM LineNo: 11
REM ExpectedError: BC0019

Imports System
	
Module ExpressionDBNull

   Sub main()
	Dim A as System.DBNull	
	Dim B as System.DBNull
	Dim C=A+B	
	
	End Sub
End Module
