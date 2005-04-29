Imports System
Module Module1
	Class C
		Sub F(ParamArray ByVal args() As Short)
			Console.WriteLine ("Integer")
			Dim a as Integer
			a = args.Length
		End Sub
		Sub F(ParamArray ByVal args() As Long)
			Throw New System.Exception("#A1, Unexcepted behavoiur of PARAM ARRAY")
		End Sub
	End Class

	Sub Main()
		Dim obj As Object = new C()
		Dim a As Byte = 1
		obj.F(a,a,a,a)
	End Sub
End Module
