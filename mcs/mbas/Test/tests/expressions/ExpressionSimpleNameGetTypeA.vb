'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System

Module SomeGetType
	Sub Main() 
		Dim x As Integer
		Dim y As System.Int32
		If x.GetType() Is y.GetType( ) Then
		Else
			Throw New Exception ("Unexpected Behavior of GetType.expected was Int32 ")
		End If
	End Sub 
End Module