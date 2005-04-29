Imports System
Imports System.Array
Module APV1_0
	Class C
		 Function Increase(ByVal A() As Long) As Long()
			Dim J As Integer
			For J = 0 To 3
			A(J) = A(J) + 1
			Next J
			return A
		End Function
		' ...
		 Function Replace(ByVal A() As Long) As Long()
			Dim J As Integer
			Dim K() As Long = {100, 200, 300, 400}
			A = K
			For J = 0 To 3
			A(J) = A(J) + 1
			Next J
			return A
		End Function
		' ...
	End Class
		
	Sub Main()
		Dim obj As Object = new c()
		Dim N() As Long = {10, 20, 30, 40}
		Dim N1() As Long = {0,0,0,0}
		Dim N2(3) As Long 
		Dim i As Integer
		N1=obj.Increase(N)
		For i=0 To 3
			if(N(i)<>N1(i))
				Throw new System.Exception ("#A1, Unexpected behavior in Increase function")
			end if
		Next i	
		N2=obj.Replace(N)
		For i=0 To 3
			if(N(i)=N2(i))
				Throw new System.Exception ("#A2, Unexpected behavior in Replace function")
			end if
		Next i
	End Sub 
End Module
