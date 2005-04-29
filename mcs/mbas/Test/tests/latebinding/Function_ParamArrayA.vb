Imports System
Module PA_1_2_1
	Class C
		Function F(ParamArray ByVal args() As Integer)As Integer
			Dim a as integer
			a = args.Length
			return a
		End Function
	End Class

	Sub Main()
		Dim obj As Object = new C()
		Dim a As Integer() = { 1, 2, 3 }
		Dim b as Integer
		b= obj.F(a)
		if b<>3
			Throw New System.Exception("#A1, Unexcepted Behaviour in F(a)")
		end if
	
		b = obj.F(10, 20, 30, 40)
		if b<>4
			Throw New System.Exception("#A2, Unexcepted Behaviour in F(10,20,30,40)")
		end if
		b = obj.F()
		if b<>0
			Throw New System.Exception("#A3, Unexcepted Behaviour in F()")
		end if
	End Sub
End Module
