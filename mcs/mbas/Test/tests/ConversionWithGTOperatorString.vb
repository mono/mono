'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module ExpressionOperatorStrings

   Sub main()
	Dim Str1 As String = "Test1"
	Dim Str2 As String = "Test2"
	if Str1 > Str2 Then
		Throw New Exception ("Exception occured Str1 can't be greater that Str2")
	End if
    End Sub
End Module

