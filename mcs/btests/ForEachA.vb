Imports System

Module M
	Sub Main
		Dim a As Integer() = { 1, 2, 3}
		Dim b As Integer
		Dim c As Integer = 0
		For Each b In a
			c = c + b
			c = c + 1
		Next
		If c <> 9 Then
			Throw New Exception("#A1: count is wrong")
		End If
		c = 0
		For Each b In a
			c = c + b
			c = c + 1
			Dim d As Integer
			For Each d In a
				c = c + d
			Next
		Next
		If c <> 27 Then
			Throw New Exception("#A2: count is wrong")
		End If
	End Sub
End Module

