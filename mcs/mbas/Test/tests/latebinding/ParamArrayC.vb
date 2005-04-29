Module PA_1_0_0
	Class C
		Sub F(ParamArray args() As Long)
			Dim a as Integer
			a=args.Length
			if a <> 3
				Throw New System.Exception("#A1, Unexcepted behavoiur of PARAM ARRAY")
			end if
		End Sub
		Sub F(ParamArray args() As Integer)
			Dim a as Integer
			a=args.Length
			if a <> 4
				Throw New System.Exception("#A1, Unexcepted behavoiur of PARAM ARRAY")
			end if
		End Sub
	End Class

	Sub Main()
		Dim obj As Object = new C()
		Dim a As long() = { 1, 2, 3 }
		obj.F(a)
		obj.F(10, 20, 30, 40)
	End Sub
End Module
