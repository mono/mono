'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)
'this is when first term is False

Imports System
Imports Microsoft.VisualBasic
Module Test

Function f1(i As integer,j As integer,k As integer) As Boolean
	Dim E As Boolean = (k - i) > (j - i)
	Return E
End Function

Function f2 (l as integer,m as integer)As Boolean
	Dim F As Boolean = l > m
	Return F
End Function

Sub Main()
	Dim MyCheck As Boolean
	MyCheck = f1(10,40,30)OrElse f2(10,20)
	If Mycheck = True Then
		Throw New Exception (" Unexpected Result for the Expression  ")
	End if
End Sub
End Module
