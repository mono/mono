Module PA_1_0_0
	Class C
		Function F(ByVal b as Integer ,ParamArray args() As Integer)as Boolean
			Dim a as integer
			a = args.Length
			if a=b
				return true
			else 
				return false
			end if
		End Function
	End Class
	Sub Main()
		Dim obj As Object = new C()
		Dim a As Integer() = { 1, 2, 3 }
		Dim c as Integer
		c = a.Length
		Dim b as Boolean
		b= obj.F(c, a)
		if b<>true
			Throw New System.Exception("#A1, Unexcepted Behaviour in F(a)")
		end if
	End Sub
End Module
