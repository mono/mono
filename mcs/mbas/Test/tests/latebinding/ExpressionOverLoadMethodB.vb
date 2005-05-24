'Author:
'   Satya Sudha K (ksathyasudha@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System

Module M
	Function F (a As Long) As Integer
		return 1
	End Function
	Function F (a As String) As Integer
		return 2
	End Function
	Sub Main ()
		Dim obj As Object = "ABC"
		if F (obj) <> 2 Then
			throw new Exception ("Overload Resolution failed in latebinding")
		End If
	End Sub
End Module
