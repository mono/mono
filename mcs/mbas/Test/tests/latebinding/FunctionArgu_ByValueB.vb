Imports System
Module APV1_4_0
	Class C
		Function F(p As Integer) as Integer
			p += 1
			return p
		End Function
	End Class 

	Sub Main()
		Dim obj As Object = new C()
		Dim a As Integer = 1
		Dim b as Integer = 0
		b = obj.F(a)
		if b=a
			Throw new System.Exception("#A1, uncexcepted behaviour of Default VB pass arguments")
		end if
	End Sub 
End Module
