'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)
'Nothing keyword represents the default value of any data type

Imports System
Imports Microsoft.Visualbasic

Module ExpressionLiteralsNothing 
Public Structure MyStruct
Public Name As String
Public Number As Short
End Structure
	Sub Main()
		Dim A As MyStruct
		A = Nothing
		If A.Name <> Nothing Then
			Throw New Exception ("Unexpected behavior. A.Name Should be Nothing ")	
		End If
		If A.Number <> 0
			Throw New Exception ("Unexpected behavior. A.Number Should be 0 ")	
		End If
	End Sub
End Module
