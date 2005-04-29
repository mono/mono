Imports System 
Module Test 

	Class C
		Sub F(i as integer) 
			if i <> 10 Then
				Throw New Exception ("got in to the one with 1 argu ")
			End if
		End Sub 
		
		Sub F()
			Dim Funct = "With no argument"
		End Sub 
		
		Sub F(i as integer, j as integer) 
			if i <> 10 And j <> 20 Then
				Throw New Exception ("got in to the one with 1 argu ")
			End if
		End Sub 
		
		Sub F(i as integer, j as integer, k as integer) 
			if i <> 10 And j <> 20 And k <> 30 Then
				Throw New Exception ("got in to the one with 1 argu ")
			End if
		End Sub 
	End Class
	
	Sub Main() 
		Dim a As Object = new C()
		a.F() 
		a.F(10) 
		a.F(10, 20) 
		a.F(10, 20, 30) 
	End Sub 
End Module
