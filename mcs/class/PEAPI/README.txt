PEAPI
-----

This is a preliminary version of our PE writer component.  It is a managed 
component which presents on the client side an API and constructs program 
executable files.  

We have tested this component as an alternative backend for Gardens Point 
Component Pascal .NET.  Our previous versions of gpcp produced textual CIL and
invoked ilasm.  The new backend can create a program executable file in almost
exactly the same length of time as it takes to write the equivalent CIL text 
file.  

PEAPI is written in C# and is released as open source under a FreeBSD-like 
licence.  Included in this release is pdf documentation and both html and 
chm documentation.  The main documentation is written as if it was a new 
Appendix to John Gough's book Compiling for the .NET Common Language Runtime, 
Prentice-Hall 2002.

The current release implements most of the facilities of the API however, 
some final features not required for component pascal have yet to be added.  
We expect to update the component incrementally as additional features are 
added.  Currently the component does not produce debugger information (pdb 
files).  We are considering possible ways of doing this or alternatively 
producing rotor-format debugging information.

The team has a committment to maintain and update the component into the 
foreseeable future, as several other projects here depend on it.  Users are 
encouraged to send feedback on missing features, bug reports etc. to assist 
in this quest.

Update (18th Apr 2005)

Some portions of the code have been taken from PERWAPI (http://www.plas.fit.qut.edu.au/perwapi/Default.aspx), which extends PEAPI. It is developed by Diane Corney.
