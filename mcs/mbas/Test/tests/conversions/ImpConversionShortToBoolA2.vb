Module ImpConversionofShorttoBool
	Sub Main()
		Dim a as Short = 123
		Dim b as Boolean = a
		if b <> True then 
			Throw New System.Exception("Implicit Conversion of Short to Bool(True) has Failed. Expected True got "+b)
		End if		
	End Sub
End Module
