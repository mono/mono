Module ImpConversionofBooltoLong
	Sub Main()
		Dim b as Boolean = False
		Dim a as Long = b 
		if a <> 0 then 
			Throw New System.Exception("Implicit Conversion of Bool(False) to Long has Failed. Expected 0, but got " & a)
		End if		
	End Sub
End Module
