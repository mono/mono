'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionLongtoDoubleC
	Sub Main()
			Dim a as Long= 111
			Dim b as Double = 111.9 + a
			if b <> 222.9
				Throw new System.Exception("Addition of Long & Double not working. Expected 222.9 but got " &b) 
			End if		
	End Sub
End Module

