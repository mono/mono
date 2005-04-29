Imports Microsoft.VisualBasic
Imports System

Module InvocationStatementA
	Dim i As Integer = 0
	Class C
	
		Sub f1()
			i += 1
			f2()
		End Sub
		
		Function f2() As Integer
			i += 2
		End Function
	
		Function f2(ByVal i As Integer) As Integer
			i += 5
			Return i
		End Function
	
		Function f2(ByVal o As Object) As Boolean
			Return True
		End Function
	
		Function f3(ByVal j As Integer)
			i += j
		End Function
	
		Function f4(ByVal j As Integer)
			i += j * 10
		End Function
	End Class
	
	Sub main()
		Dim obj As Object = new C()
		Call obj.f1()
		If i <> 3 Then
			Throw New Exception("#ISB1 - Invocation Statement failed")
		End If
		
		If obj.f2(i) <> 8 Then
			Throw New Exception("#ISB2 - Invocation Statement failed")
		End If
		
		If Not obj.f2("Hello") Then
			Throw New Exception("#ISB3 - Invocation Statement failed")
		End If
		
		If Not obj.f2(2.3D) Then
			Throw New Exception("#ISB4 - Invocation Statement failed")
		End If
	
	End Sub
	
End Module
