Imports System
Imports TestUtils

Module Test

Public Sub Main()
	Dim a() As String
	Dim b(,) As String
	Dim c(,,) As String
	Dim o As Object
	Dim s As String = ""
	
	a = New String(1) { "0", "1" }
	b = New String(1,1) { {"0,0", "0,1"}, {"1,0", "1,1"} }
	c = New String(1,1,1) { {{"0,0,0", "0,0,1"}, {"0,1,0", "0,1,1"}}, {{"1,0,0", "1,0,1"}, {"1,1,0", "1,1,1"}}   }
	
	' Testing with monodimensional array
	o = a
	s = String.Format ("/{0}/{1}", o(0), o(1))
	
	' Testing with bidimensional array
	o = b
	s = s & String.Format ("/{0}/{1}/{2}/{3}", o(0,0), o(0,1), o(1,0), o(1,1))
	
	' Testing with tridimensional array
	o = c
	s = s & String.Format  ("/{0}/{1}/{2}/{3}/{4}/{5}/{6}/{7}/", o(0,0,0), o(0,0,1), o(0,1,0), o(0,1,1), o(1,0,0), o(1,0,1), o(1,1,0), o(1,1,1))	
	Console.WriteLine (TestUtils.GenerateHash(s))
End Sub

End Module
