Imports System

Module Test
	Sub Main
		Dim s As String = GetType(String).ToString()
		If s <> "System.String" Then
			Throw New Exception("#A1: wrong type returned")
		End If
		Dim t As Type = GetType(String)
		If Not t Is s.GetType() Then
			Throw New Exception("#A2: wrong type returned")
		End If
	End Sub
End Module


