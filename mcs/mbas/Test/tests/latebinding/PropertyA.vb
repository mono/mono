Imports System
Module M
	Class C
		private i as integer = 20
	
		public Property p() as Integer
			GET
				return i
			END GET
	
			SET (ByVal val as Integer)
				i = val
			End SET
	
		End Property
	End Class

	Sub Main()
		Dim o As Object = new C()
		if (o.p <> 20) Then
			throw new Exception ("Property Access not working in LateBinding!!")
		End If
	End Sub

End Module
