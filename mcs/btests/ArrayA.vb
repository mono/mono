Imports System

Module VariableC
    Dim a() As Integer = {1, 2, 3, 4, 5}
    Dim b(3) As Integer
    Dim c(2), d(4) As Long
    Dim e as Integer() = {1, 2, 3}
    dim f(2,3) as Integer 
    Dim g as Integer(,) = { {1,1}, {2,2}, {3,3} }
    Dim h() as Integer
    Dim i as integer(,)
    Dim j as integer()

    Sub Main()

	If a(2) <> 3 Then
            Throw New Exception("#A1, value mismatch")
        End If
	
	b(0) = 0
	b(1) = 5
	b(2) = 10
	If b(1) <> 5 Then
            Throw New Exception("#A2, value mismatch")
        End If

	If e(1) <> 2 Then
            Throw New Exception("#A3, value mismatch")
        End If

	If g(2,1) <> 3 Then
            Throw New Exception("#A4, value mismatch")
        End If


	'Console.WriteLine(e(1))
	'Console.WriteLine(g(2,1))

	h = new Integer(){0, 1, 2}
	i = new Integer(2,1){ {1,1}, {2,2}, {3,3} }

	j = new Integer(2) {}
    End Sub
End Module
