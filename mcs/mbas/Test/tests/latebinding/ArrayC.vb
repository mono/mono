Imports System
Imports Microsoft.VisualBasic

Class C
	Public a() As Integer = {1, 2, 3, 4, 5}
End Class

Module VariableC
	Sub Main()
		dim c as Integer
		dim o as Object = new C()
		c = UBound(o.a, 1)
		if c<>4
			Throw New System.Exception("#A1 Error")
		End If
		c = LBound(o.a, 1)
		if c<>0 
			Throw New System.Exception("#A2 Error")
		End if
	End Sub
End Module
