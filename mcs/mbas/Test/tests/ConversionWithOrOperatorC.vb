'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System
Module ConversionOrOperator

Sub Main()
	Dim A As Integer = 333
	Dim B As Double = 555.555   
	Dim R As Boolean
	R = (B > A) Or (A < B) 
	if R = 0 Then
		throw new Exception("#Error With Or Operator")
	End if
End Sub
End Module
