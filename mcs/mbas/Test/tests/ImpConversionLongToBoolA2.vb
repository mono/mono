Module ImpConversionofLongtoBool
	Sub Main()
		Dim a as Long = 123
		Dim b as Boolean = a
		if b <> True then 
			Throw New System.Exception("Implicit Conversion of Long to Bool(True) has Failed. Expected True got "+b)
		End if
	End Sub
End Module
