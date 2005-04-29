Imports System
Module Module1
	Class C
		Sub F(ParamArray ByVal args() As Integer)
			Dim a as Integer
			a = args.Length
			if a=0
				Throw New System.Exception("#A1, Unexcepted behavoiur of PARAM ARRAY")
			end if
		End Sub
	End Class

	Sub Main()
		Dim obj As Object = new C()
		Dim a As Integer() = { 1, 2, 3 }
		obj.F(a)
		Dim x As Integer = 10
		obj.F(x, x, x, x)
	End Sub
End Module
