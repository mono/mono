Imports System
Module APV1_0
	Class C
		Function  F(ByVal p As Integer) As Integer
			p += 1
			return p
		End Function 
	End Class 
	Sub Main()
		Dim obj As Object = new C()
		Dim a As Integer = 1
		Dim b As Integer = 0 
		b = obj.F(a)
		if b=a
			Throw new System.Exception("#A1, Unexcepted behaviour")
		end if
	End Sub 
End Module
