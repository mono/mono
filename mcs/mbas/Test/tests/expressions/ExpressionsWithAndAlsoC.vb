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
	Dim F As Boolean = l < m
	If l = 10 then
		Throw New Exception ("Second Term is not supposed to be evaluated as the First Term is False")	
	End If
	Return F
End Function

Sub Main()
        Dim MyCheck As Boolean
        MyCheck = f1(10,40,30)AndAlso f2(10,20)
        If Mycheck = True Then
	  	Throw New Exception (" Unexpected Result for the Expression  ")
        End if
End Sub
End Module

