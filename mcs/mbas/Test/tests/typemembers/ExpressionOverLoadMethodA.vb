'Author:
'   Satya Sudha K (ksathyasudha@novell.com)
'
' (C) 2005 Novell, Inc.
'
' Since the argument is of type object, the first method should be invoked irrespective of the 
' runtime type of the object

Imports System

Module M
	Function F (a As Object) As Integer
		return 1
	End Function
	Function F (a As String) As Integer
		return 2
	End Function
	Sub Main ()
		Dim obj As Object = "ABC"
		if F (obj) <> 1 Then
			throw new Exception ("Overload Resolution failed in latebinding")
		End If
	End Sub
End Module
