Module ImpConversionofShorttoBool
	Sub Main()
		Dim a as Short = 0
		Dim b as Boolean = a
		if b <> False then 
			Throw New System.Exception("Implicit Conversion of Short to Bool(False) has Failed. Expected False, but got " & b)
		End if		
	End Sub
End Module
