Imports System.Array
Module APV1_0

	Class C
		Public Sub Increase(ByVal A() As Long)
    			Dim J As Integer
    			For J = 0 To 3
       			A(J) = A(J) + 1
    			Next J
		End Sub
   	' ...
		Public Sub Replace(ByVal A() As Long)
   			Dim J As Integer
   			Dim K() As Long = {100, 200, 300, 400}
   			A = K
   			For J = 0 To 3
      			A(J) = A(J) + 1
   			Next J
		End Sub
 	' ...
	End Class
 	
	Sub Main()
		Dim obj As Object = new C()
		Dim N() As Long = {10, 20, 30, 40}
		Dim N1() As Long = {11, 21, 31, 41}
		Dim N2() As Long = {100, 200, 300, 400}
		Dim i As Integer
		obj.Increase(N)
		For i=0 To 3
			if(N(i)<>N1(i))
				Throw new System.Exception ("#A1, Unexpected behavior in Increase function")
			end if
		Next i
		i=0	
		obj.Replace(N)
		For i=0 To 3
			if(N(i)=N2(i))
				Throw new System.Exception ("#A2, Unexpected behavior in Replace function")
			end if
		Next i
	End Sub 
End Module
