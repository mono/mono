Imports System
Imports TestUtils

Module Test

Dim s As String = ""

Public Function MyFunction1 (a As String) As String
	Return a & a
End Function

Public Function MyFunction2 (a As String) As String
	MyFunction2 = a  & a
End Function

Public Sub MySub(Optional Byval a As Integer = -7)
	s = s & a
End Sub

Public Sub MySub2(a As String, Optional Byval b As Integer = -7)
	s = s & a
	s = s & b
End Sub

Public Sub Main()
	MySub ()
	MySub (2)
	
	MySub2 ("a")
	MySub2 ("a", 9)
	s = s & MyFunction1 ("abc")
	s = s & MyFunction2 ("def")
	Console.WriteLine (TestUtils.GenerateHash(s))
End Sub

End Module
