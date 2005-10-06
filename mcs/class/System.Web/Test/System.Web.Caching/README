The CacheStress.cs test is a standalone test that should be compiled and run
as a console application.
In normal mode the test prints every 10 seconds the number of transactions it
committed.
In case of an exception the test prints the exception and continues. In case
of a deadlock the transaction count will remain constant.
Note that the test does not run in .Net on Windows since the System.Web.Caching
of .Net cannot be used from a console application. Mono's implementation does
not currently have this dependency. When (and if) it does this test should
be made into a Web application.
