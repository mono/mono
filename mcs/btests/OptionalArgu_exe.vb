Module M
	Sub Main()
		Dim x As New c()
		
		Dim res as String

		res = x.s (10)
		If (res <> "s : 10 - 10 - aaa")
			Throw New System.Exception ("#A1, Unexpected result returned by OptionalArgu_dll.dll")
		End If
		
		res = x.s (5, 5)
		If (res <> "s : 5 - 5 - aaa")
			Throw New System.Exception ("#A2, Unexpected result returned by OptionalArgu_dll.dll")
		End If
	End Sub
End Module
