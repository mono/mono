'Author:
'   Satya Sudha K (ksathyasudha@novell.com)
'
' (C) 2005 Novell, Inc.
'
' Since there is only one applicable method, no late binding is involved

Imports System
Module M
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
