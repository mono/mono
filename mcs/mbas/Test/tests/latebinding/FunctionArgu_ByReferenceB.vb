Imports System
Imports System.Array
Module APR_1_3_0
	Class C
		Public Function Increase(ByRef A() As Long) As Long()
			Dim J As Integer
			For J = 0 To 3
			A(J) = A(J) + 1
			Next J
			return A
		End Function
	' ...
		Public Function Replace(ByRef A() As Long) As Long()
			Dim J As Integer
			Dim K() As Long = {100, 200, 300,400}
			A = K
			For J = 0 To 3
			A(J) = A(J) + 1
			Next J
			return A
		End Function
	' ...
	End Class 
	Sub Main()
		Dim obj As Object = new C()
		Dim N() As Long = {10, 20, 30, 40}
		Dim N1(3) As Long
		Dim N2(3) As Long 
		Dim i As Integer
		N1=obj.Increase(N)
		For i = 0 to 3
			if (N(i) <> N1(i))
				Throw New System.Exception("#A1, Unexception Behaviour in Increase Function")
			end if
		Next i
		N2=obj.Replace(N)
		For i= 0 to 3
			if ( N(i) <> N2(i))
				Throw New System.Exception("#A2, Unexception Behaviour in Increase Function")
			end if
		Next i
	
	End Sub 
End Module
