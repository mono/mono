'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module ConversionNotOperator

Sub Main()
	Dim A As Integer = 333
	Dim B As Long = 555.555   
	Dim R As Boolean
	R = Not (B > A)
	if R = True Then
		throw new Exception("#Error With Not Operator")
	End if
End Sub
End Module
