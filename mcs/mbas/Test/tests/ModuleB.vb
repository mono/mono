NameSpace NS
	Public Module M1
		Public a As Integer
		public Const b as integer = 10
		Class C1
		End Class
	End Module

	Friend Module MainModule
		Sub Main()
			M1.a = 20
			dim x as integer = M1.b
			if (x <> 10) then
				Throw new System.Exception("#A1, Unexpected result")
			end if

			x = NS.M1.b
			if x <> 10 then
				Throw new System.Exception("#A2, Unexpected result")
			end if
		End Sub
	End Module
End NameSpace
