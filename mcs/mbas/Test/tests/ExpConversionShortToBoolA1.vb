Module ExpConversionShorttoBool
	Sub Main()
		Dim a As Short = 123
		Dim b as Boolean
		b = CBool(a)
		if b <> True
			Throw new System.Exception("Short to Boolean Conversion is not working properly. Expected True but got " &b)
		End if	
	End Sub
End Module
