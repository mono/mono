Imports System

Structure S
	Dim a as String
	Const b as integer = 25

	Class c
	End class

	Function f(l as long) as long
		f = l
	End Function

	Structure S1
		dim g as string
	End Structure

End Structure


Module M
	Sub Main()
		dim x as S

		x.a = 10
		If x.a <> 10 then
			Throw new Exception ("#A1, Unexpected result")
		End If

		dim y as S = x

		x.a = 20
		If y.a <> 10 then
			Throw new Exception ("#A2, Unexpected result")
		End If
		If x.a <> 20 then
			Throw new Exception ("#A3, Unexpected result")
		End If

		If x.b <> 25 then
			Throw new Exception ("#A4, Unexpected result")
		End If
		'Console.WriteLine(x.b)

		If x.f(99) <> 99 then
			Throw new Exception ("#A5, Unexpected result")
		End If

	End Sub
End Module
