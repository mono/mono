'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)
'Nothing keyword represents the default value of any data type

Imports System
Imports Microsoft.Visualbasic
Module ExpressionLiteralsChar 
	Sub Main ( ) 
		Dim L As Long,S As String,B As Boolean,O As Object,D As Date
		B = Nothing 
		If B <> False Then
			Throw New Exception ("Unexpected Behavior of Nothing. As B should be assigned Flase")
		End If
		S = Nothing 
		If S <> Nothing Then
			Throw New Exception ("Unexpected Behavior of Nothing. As S should be assigned Nothing")
		End If
		D = Nothing 
		If D <> #1/1/001 12:00:00 PM # Then
			Throw New Exception("Unexpected Behavior of Nothing. D not set to default value")
		End If
		L = Nothing 
		If L <> 0 Then
			Throw New Exception ("Unexpected Behavior of Nothing. As L should be assigned 0 ")
		End If
	End Sub 
End Module

