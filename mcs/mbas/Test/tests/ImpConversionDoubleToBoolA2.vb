Module ImpConversionofDoubletoBool
	Sub Main()
		Dim a as Double = 123
		Dim b as Boolean = a
		if b <> True then 
			Throw New System.Exception("Implicit Conversion of Double to Bool(True) has Failed.Expected True got "+b)
		End if
	End Sub
End Module
