Module DateLiterals
    Sub Main()
        Dim d As Date
	dim d1 as Date

        d = # 12/1/2001 3:24:59 PM #
        d1 = # 12-01-2001 3:24:59 PM #

	if d1 <> d then
		Throw new System.Exception("#A1 : values d and d1 are not same")
	end if

	d = # 12/01/2001 #
	d1 = # 12/01/2001 12:00:00 PM#

	System.Console.WriteLine(d)
	System.Console.WriteLine(d1)	
	
	'if d1 <> d then
	'	Throw new System.Exception("#A2 : values d and d1 are not same")
	'end if

	d = # 3:24:59 #
	d1 = # 01/01/0001 3:24:59 AM#

	if d1 <> d then
		Throw new System.Exception("#A3 : values d and d1 are not same")
	end if

	d = # 15:24:59 #
	d1 = # 01/01/0001 3:24:59 PM#

	System.Console.WriteLine(d)
	System.Console.WriteLine(d1)	

	if d1 <> d then
		Throw new System.Exception("#A4 : values d and d1 are not same")
	end if

	d = # 3 PM #
	d1 = # 01/01/0001 15:00:00 #

	if d1 <> d then
		Throw new System.Exception("#A5 : values d and d1 are not same")
	end if

	d = # 3:13 PM #
	d1 = # 01/01/0001 3:13:00 PM#

	if d1 <> d then
		Throw new System.Exception("#A6 : values d and d1 are not same")
	end if
    End Sub
End Module
