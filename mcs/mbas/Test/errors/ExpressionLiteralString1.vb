'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)
REM LineNo: 11
REM ExpectedError: BC30205
REM ErrorMessage: End of statement expected. Dim A as String = "a"b" 

imports Microsoft.Visualbasic
Imports System
Module ExpressionLiteralString
	Sub Main() 
		Dim A as String = "a"b"
		Console.WriteLine(A)
	End Sub 
End Module
