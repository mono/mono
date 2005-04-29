Imports System

Module Module1
	Class C
		Function F(ByVal ParamArray a As Object()) As Integer
			return 0
		End Function
	
		Function F()
			return 1
		End Function

		Function F(ByVal a As Object, ByVal b As Object)
			return 2
		End Function
		
		Function F(ByVal a As Object, ByVal b As Object, _
			ByVal ParamArray c As Object())
			return 3
		End Function
	End Class

	Sub Main()
		Dim obj As Object = new C()
		if obj.F() <> 1 Then
			Throw new exception ("#A1 - Overload resolution not working in Late binding")
		End If
		if obj.F(1) <> 0 Then
			Throw new exception ("#A2 - Overload resolution not working in Late binding")
		End If
		if obj.F(1, 2) <> 2
			Throw new exception ("#A3 - Overload resolution not working in Late binding")
		End If
		if obj.F(1, 2, 3) <> 3 Then
			Throw new exception ("#A4 - Overload resolution not working in Late binding")
		End If

	End Sub
End Module
