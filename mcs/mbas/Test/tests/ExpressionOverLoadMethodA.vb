'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System 
Module Test 

Sub F(i as integer) 
	if i <> 10 Then
		Throw New Exception ("got in to the one with 1 argu ")
	End if
End Sub 

Sub F()
	Dim Funct = "With no argument"
End Sub 

Sub F(i as integer, j as integer) 
	if i <> 10 And j <> 20 Then
		Throw New Exception ("got in to the one with 1 argu ")
	End if
End Sub 

Sub F(i as integer, j as integer, k as integer) 
	if i <> 10 And j <> 20 And k <> 30 Then
		Throw New Exception ("got in to the one with 1 argu ")
	End if
End Sub 

Sub Main() 
	F() 
	F(10) 
	F(10, 20) 
	F(10, 20, 30) 
End Sub 
End Module
