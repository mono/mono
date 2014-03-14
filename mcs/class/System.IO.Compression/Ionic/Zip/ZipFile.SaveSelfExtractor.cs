// ZipFile.saveSelfExtractor.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2008-2011 Dino Chiesa.
// All rights reserved.
//
// This code module is part of DotNetZip, a zipfile class library.
//
// ------------------------------------------------------------------
//
// This code is licensed under the Microsoft Public License.
// See the file License.txt for the license details.
// More info on: http://dotnetzip.codeplex.com
//
// ------------------------------------------------------------------
//
// last saved (in emacs):
// Time-stamp: <2011-August-10 19:22:46>
//
// ------------------------------------------------------------------
//
// This is a the source module that implements the stuff for saving to a
// self-extracting Zip archive.
//
// ZipFile is set up as a "partial class" - defined in multiple .cs source modules.
// This is one of the source modules for the ZipFile class.
//
// Here's the design: The self-extracting zip file is just a regular managed EXE
// file, with embedded resources.  The managed code logic instantiates a ZipFile, and
// then extracts each entry.  The embedded resources include the zip archive content,
// as well as the Zip library itself.  The latter is required so that self-extracting
// can work on any machine, whether or not it has the DotNetZip library installed on
// it.
//
// What we need to do is create the animal I just described, within a method on the
// ZipFile class.  This source module provides that capability. The method is
// SaveSelfExtractor().
//
// The way the method works: it uses the programmatic interface to the csc.exe
// compiler, Microsoft.CSharp.CSharpCodeProvider, to compile "boilerplate"
// extraction logic into a new assembly.  As part of that compile, we embed within
// that assembly the zip archive itself, as well as the Zip library.
//
// Therefore we need to first save to a temporary zip file, then produce the exe.
//
// There are a few twists.
//
// The Visual Studio Project structure is a little weird.  There are code files
// that ARE NOT compiled during a normal build of the VS Solution.  They are
// marked as embedded resources.  These are the various "boilerplate" modules that
// are used in the self-extractor. These modules are: WinFormsSelfExtractorStub.cs
// WinFormsSelfExtractorStub.Designer.cs CommandLineSelfExtractorStub.cs
// PasswordDialog.cs PasswordDialog.Designer.cs
//
// At design time, if you want to modify the way the GUI looks, you have to
// mark those modules to have a "compile" build action.  Then tweak em, test,
// etc.  Then again mark them as "Embedded resource".
//
// ------------------------------------------------------------------

using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;


namespace Ionic.Zip
{
#if !NO_SFX
    /// <summary>
    /// An enum that provides the different self-extractor flavors
    /// </summary>
    internal enum SelfExtractorFlavor
    {
        /// <summary>
        /// A self-extracting zip archive that runs from the console or
        /// command line.
        /// </summary>
        ConsoleApplication = 0,

        /// <summary>
        /// A self-extracting zip archive that presents a graphical user
        /// interface when it is executed.
        /// </summary>
        WinFormsApplication,
    }

    /// <summary>
    /// The options for generating a self-extracting archive.
    /// </summary>
    internal class SelfExtractorSaveOptions
    {
        /// <summary>
        ///   The type of SFX to create.
        /// </summary>
        public SelfExtractorFlavor Flavor
        {
            get;
            set;
        }

        /// <summary>
        ///   The command to run after extraction.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   This is optional. Leave it empty (<c>null</c> in C# or <c>Nothing</c> in
        ///   VB) to run no command after extraction.
        /// </para>
        ///
        /// <para>
        ///   If it is non-empty, the SFX will execute the command specified in this
        ///   string on the user's machine, and using the extract directory as the
        ///   working directory for the process, after unpacking the archive. The
        ///   program to execute can include a path, if you like. If you want to execute
        ///   a program that accepts arguments, specify the program name, followed by a
        ///   space, and then the arguments for the program, each separated by a space,
        ///   just as you would on a normal command line. Example: <c>program.exe arg1
        ///   arg2</c>.  The string prior to the first space will be taken as the
        ///   program name, and the string following the first space specifies the
        ///   arguments to the program.
        /// </para>
        ///
        /// <para>
        ///   If you want to execute a program that has a space in the name or path of
        ///   the file, surround the program name in double-quotes. The first character
        ///   of the command line should be a double-quote character, and there must be
        ///   a matching double-quote following the end of the program file name. Any
        ///   optional arguments to the program follow that, separated by
        ///   spaces. Example: <c>"c:\project files\program name.exe" arg1 arg2</c>.
        /// </para>
        ///
        /// <para>
        ///   If the flavor of the SFX is <c>SelfExtractorFlavor.ConsoleApplication</c>,
        ///   then the SFX starts a new process, using this string as the post-extract
        ///   command line.  The SFX waits for the process to exit.  The exit code of
        ///   the post-extract command line is returned as the exit code of the
        ///   command-line self-extractor exe. A non-zero exit code is typically used to
        ///   indicated a failure by the program. In the case of an SFX, a non-zero exit
        ///   code may indicate a failure during extraction, OR, it may indicate a
        ///   failure of the run-after-extract program if specified, OR, it may indicate
        ///   the run-after-extract program could not be fuond. There is no way to
        ///   distinguish these conditions from the calling shell, aside from parsing
        ///   the output of the SFX. If you have Quiet set to <c>true</c>, you may not
        ///   see error messages, if a problem occurs.
        /// </para>
        ///
        /// <para>
        ///   If the flavor of the SFX is
        ///   <c>SelfExtractorFlavor.WinFormsApplication</c>, then the SFX starts a new
        ///   process, using this string as the post-extract command line, and using the
        ///   extract directory as the working directory for the process. The SFX does
        ///   not wait for the command to complete, and does not check the exit code of
        ///   the program. If the run-after-extract program cannot be fuond, a message
        ///   box is displayed indicating that fact.
        /// </para>
        ///
        /// <para>
        ///   You can specify environment variables within this string, with a format like
        ///   <c>%NAME%</c>. The value of these variables will be expanded at the time
        ///   the SFX is run. Example: <c>%WINDIR%\system32\xcopy.exe</c> may expand at
        ///   runtime to <c>c:\Windows\System32\xcopy.exe</c>.
        /// </para>
        ///
        /// <para>
        ///   By combining this with the <c>RemoveUnpackedFilesAfterExecute</c>
        ///   flag, you can create an SFX that extracts itself, runs a file that
        ///   was extracted, then deletes all the files that were extracted. If
        ///   you want it to run "invisibly" then set <c>Flavor</c> to
        ///   <c>SelfExtractorFlavor.ConsoleApplication</c>, and set <c>Quiet</c>
        ///   to true.  The user running such an EXE will see a console window
        ///   appear, then disappear quickly.  You may also want to specify the
        ///   default extract location, with <c>DefaultExtractDirectory</c>.
        /// </para>
        ///
        /// <para>
        ///   If you set <c>Flavor</c> to
        ///   <c>SelfExtractorFlavor.WinFormsApplication</c>, and set <c>Quiet</c> to
        ///   true, then a GUI with progressbars is displayed, but it is
        ///   "non-interactive" - it accepts no input from the user.  Instead the SFX
        ///   just automatically unpacks and exits.
        /// </para>
        ///
        /// </remarks>
        public String PostExtractCommandLine
        {
            get;
            set;
        }

        /// <summary>
        ///   The default extract directory the user will see when
        ///   running the self-extracting archive.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   Passing null (or Nothing in VB) here will cause the Self Extractor to use
        ///   the the user's personal directory (<see
        ///   cref="Environment.SpecialFolder.Personal"/>) for the default extract
        ///   location.
        /// </para>
        ///
        /// <para>
        ///   This is only a default location.  The actual extract location will be
        ///   settable on the command line when the SFX is executed.
        /// </para>
        ///
        /// <para>
        ///   You can specify environment variables within this string,
        ///   with <c>%NAME%</c>. The value of these variables will be
        ///   expanded at the time the SFX is run. Example:
        ///   <c>%USERPROFILE%\Documents\unpack</c> may expand at runtime to
        ///   <c>c:\users\melvin\Documents\unpack</c>.
        /// </para>
        /// </remarks>
        public String DefaultExtractDirectory
        {
            get;
            set;
        }

        /// <summary>
        ///   The name of an .ico file in the filesystem to use for the application icon
        ///   for the generated SFX.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   Normally, DotNetZip will embed an "zipped folder" icon into the generated
        ///   SFX.  If you prefer to use a different icon, you can specify it here. It
        ///   should be a .ico file.  This file is passed as the <c>/win32icon</c>
        ///   option to the csc.exe compiler when constructing the SFX file.
        /// </para>
        /// </remarks>
        ///
        public string IconFile
        {
            get;
            set;
        }

        /// <summary>
        ///   Whether the ConsoleApplication SFX will be quiet during extraction.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   This option affects the way the generated SFX runs. By default it is
        ///   false.  When you set it to true,...
        /// </para>
        ///
        /// <list type="table">
        ///   <listheader>
        ///     <term>Flavor</term>
        ///     <description>Behavior</description>
        ///   </listheader>
        ///
        /// <item>
        ///   <term><c>ConsoleApplication</c></term>
        ///   <description><para>no messages will be emitted during successful
        ///     operation.</para> <para> Double-clicking the SFX in Windows
        ///     Explorer or as an attachment in an email will cause a console
        ///     window to appear briefly, before it disappears. If you run the
        ///     ConsoleApplication SFX from the cmd.exe prompt, it runs as a
        ///     normal console app; by default, because it is quiet, it displays
        ///     no messages to the console.  If you pass the -v+ command line
        ///     argument to the Console SFX when you run it, you will get verbose
        ///     messages to the console. </para>
        ///   </description>
        /// </item>
        ///
        /// <item>
        ///   <term><c>WinFormsApplication</c></term>
        ///   <description>the SFX extracts automatically when the application
        ///        is launched, with no additional user input.
        ///   </description>
        /// </item>
        ///
        /// </list>
        ///
        /// <para>
        ///   When you set it to false,...
        /// </para>
        ///
        /// <list type="table">
        ///   <listheader>
        ///     <term>Flavor</term>
        ///     <description>Behavior</description>
        ///   </listheader>
        ///
        /// <item>
        ///   <term><c>ConsoleApplication</c></term>
        ///   <description><para>the extractor will emit a
        ///     message to the console for each entry extracted.</para>
        ///     <para>
        ///       When double-clicking to launch the SFX, the console window will
        ///       remain, and the SFX will emit a message for each file as it
        ///       extracts. The messages fly by quickly, they won't be easily
        ///       readable, unless the extracted files are fairly large.
        ///     </para>
        ///   </description>
        /// </item>
        ///
        /// <item>
        ///   <term><c>WinFormsApplication</c></term>
        ///   <description>the SFX presents a forms UI and allows the user to select
        ///     options before extracting.
        ///   </description>
        /// </item>
        ///
        /// </list>
        ///
        /// </remarks>
        public bool Quiet
        {
            get;
            set;
        }


        /// <summary>
        ///   Specify what the self-extractor will do when extracting an entry
        ///   would overwrite an existing file.
        /// </summary>
        /// <remarks>
        /// <para>
        ///   The default behavvior is to Throw.
        /// </para>
        /// </remarks>
        public Ionic.Zip.ExtractExistingFileAction ExtractExistingFile
        {
            get;
            set;
        }


        /// <summary>
        ///   Whether to remove the files that have been unpacked, after executing the
        ///   PostExtractCommandLine.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   If true, and if there is a <see
        ///   cref="SelfExtractorSaveOptions.PostExtractCommandLine">
        ///   PostExtractCommandLine</see>, and if the command runs successfully,
        ///   then the files that the SFX unpacked will be removed, afterwards.  If
        ///   the command does not complete successfully (non-zero return code),
        ///   that is interpreted as a failure, and the extracted files will not be
        ///   removed.
        /// </para>
        ///
        /// <para>
        ///   Setting this flag, and setting <c>Flavor</c> to
        ///   <c>SelfExtractorFlavor.ConsoleApplication</c>, and setting <c>Quiet</c> to
        ///   true, results in an SFX that extracts itself, runs a file that was
        ///   extracted, then deletes all the files that were extracted, with no
        ///   intervention by the user.  You may also want to specify the default
        ///   extract location, with <c>DefaultExtractDirectory</c>.
        /// </para>
        ///
        /// </remarks>
        public bool RemoveUnpackedFilesAfterExecute
        {
            get;
            set;
        }


        /// <summary>
        ///   The file version number to embed into the generated EXE. It will show up, for
        ///   example, during a mouseover in Windows Explorer.
        /// </summary>
        ///
        public Version FileVersion
        {
            get;
            set;
        }

        /// <summary>
        ///   The product version to embed into the generated EXE. It will show up, for
        ///   example, during a mouseover in Windows Explorer.
        /// </summary>
        ///
        /// <remarks>
        ///   You can use any arbitrary string, but a human-readable version number is
        ///   recommended. For example "v1.2 alpha" or "v4.2 RC2".  If you specify nothing,
        ///   then there is no product version embedded into the EXE.
        /// </remarks>
        ///
        public String ProductVersion
        {
            get;
            set;
        }

        /// <summary>
        ///   The copyright notice, if any, to embed into the generated EXE.
        /// </summary>
        ///
        /// <remarks>
        ///   It will show up, for example, while viewing properties of the file in
        ///   Windows Explorer.  You can use any arbitrary string, but typically you
        ///   want something like "Copyright © Dino Chiesa 2011".
        /// </remarks>
        ///
        public String Copyright
        {
            get;
            set;
        }


        /// <summary>
        ///   The description to embed into the generated EXE.
        /// </summary>
        ///
        /// <remarks>
        ///   Use any arbitrary string.  This text will be displayed during a
        ///   mouseover in Windows Explorer.  If you specify nothing, then the string
        ///   "DotNetZip SFX Archive" is embedded into the EXE as the description.
        /// </remarks>
        ///
        public String Description
        {
            get;
            set;
        }

        /// <summary>
        ///   The product name to embed into the generated EXE.
        /// </summary>
        ///
        /// <remarks>
        ///   Use any arbitrary string. This text will be displayed
        ///   while viewing properties of the EXE file in
        ///   Windows Explorer.
        /// </remarks>
        ///
        public String ProductName
        {
            get;
            set;
        }

        /// <summary>
        ///   The title to display in the Window of a GUI SFX, while it extracts.
        /// </summary>
        ///
        /// <remarks>
        ///   <para>
        ///     By default the title show in the GUI window of a self-extractor
        ///     is "DotNetZip Self-extractor (http://DotNetZip.codeplex.com/)".
        ///     You can change that by setting this property before saving the SFX.
        ///   </para>
        ///
        ///   <para>
        ///     This property has an effect only when producing a Self-extractor
        ///     of flavor <c>SelfExtractorFlavor.WinFormsApplication</c>.
        ///   </para>
        /// </remarks>
        ///
        public String SfxExeWindowTitle
        {
            // workitem 12608
            get;
            set;
        }

        /// <summary>
        ///   Additional options for the csc.exe compiler, when producing the SFX
        ///   EXE.
        /// </summary>
        /// <exclude/>
        public string AdditionalCompilerSwitches
        {
            get; set;
        }
    }




    partial class ZipFile
    {
        class ExtractorSettings
        {
            public SelfExtractorFlavor Flavor;
            public List<string> ReferencedAssemblies;
            public List<string> CopyThroughResources;
            public List<string> ResourcesToCompile;
        }


        private static ExtractorSettings[] SettingsList = {
            new ExtractorSettings() {
                Flavor = SelfExtractorFlavor.WinFormsApplication,
                ReferencedAssemblies= new List<string>{
                    "System.dll", "System.Windows.Forms.dll", "System.Drawing.dll"},
                CopyThroughResources = new List<string>{
                    "Ionic.Zip.WinFormsSelfExtractorStub.resources",
                    "Ionic.Zip.Forms.PasswordDialog.resources",
                    "Ionic.Zip.Forms.ZipContentsDialog.resources"},
                ResourcesToCompile = new List<string>{
                    "WinFormsSelfExtractorStub.cs",
                    "WinFormsSelfExtractorStub.Designer.cs", // .Designer.cs?
                    "PasswordDialog.cs",
                    "PasswordDialog.Designer.cs",             //.Designer.cs"
                    "ZipContentsDialog.cs",
                    "ZipContentsDialog.Designer.cs",             //.Designer.cs"
                    "FolderBrowserDialogEx.cs",
                }
            },
            new ExtractorSettings() {
                Flavor = SelfExtractorFlavor.ConsoleApplication,
                ReferencedAssemblies= new List<string> { "System.dll", },
                CopyThroughResources = null,
                ResourcesToCompile = new List<string>{"CommandLineSelfExtractorStub.cs"}
            }
        };



        //string _defaultExtractLocation;
        //string _postExtractCmdLine;
        //         string _SetDefaultLocationCode =
        //         "namespace Ionic.Zip { internal partial class WinFormsSelfExtractorStub { partial void _SetDefaultExtractLocation() {" +
        //         " txtExtractDirectory.Text = \"@@VALUE\"; } }}";



        /// <summary>
        /// Saves the ZipFile instance to a self-extracting zip archive.
        /// </summary>
        ///
        /// <remarks>
        ///
        /// <para>
        /// The generated exe image will execute on any machine that has the .NET
        /// Framework 2.0 installed on it.  The generated exe image is also a
        /// valid ZIP file, readable with DotNetZip or another Zip library or tool
        /// such as WinZip.
        /// </para>
        ///
        /// <para>
        /// There are two "flavors" of self-extracting archive.  The
        /// <c>WinFormsApplication</c> version will pop up a GUI and allow the
        /// user to select a target directory into which to extract. There's also
        /// a checkbox allowing the user to specify to overwrite existing files,
        /// and another checkbox to allow the user to request that Explorer be
        /// opened to see the extracted files after extraction.  The other flavor
        /// is <c>ConsoleApplication</c>.  A self-extractor generated with that
        /// flavor setting will run from the command line. It accepts command-line
        /// options to set the overwrite behavior, and to specify the target
        /// extraction directory.
        /// </para>
        ///
        /// <para>
        /// There are a few temporary files created during the saving to a
        /// self-extracting zip.  These files are created in the directory pointed
        /// to by <see cref="ZipFile.TempFileFolder"/>, which defaults to <see
        /// cref="System.IO.Path.GetTempPath"/>.  These temporary files are
        /// removed upon successful completion of this method.
        /// </para>
        ///
        /// <para>
        /// When a user runs the WinForms SFX, the user's personal directory (<see
        /// cref="Environment.SpecialFolder.Personal">Environment.SpecialFolder.Personal</see>)
        /// will be used as the default extract location.  If you want to set the
        /// default extract location, you should use the other overload of
        /// <c>SaveSelfExtractor()</c>/ The user who runs the SFX will have the
        /// opportunity to change the extract directory before extracting. When
        /// the user runs the Command-Line SFX, the user must explicitly specify
        /// the directory to which to extract.  The .NET Framework 2.0 is required
        /// on the computer when the self-extracting archive is run.
        /// </para>
        ///
        /// <para>
        /// NB: This method is not available in the version of DotNetZip build for
        /// the .NET Compact Framework, nor in the "Reduced" DotNetZip library.
        /// </para>
        ///
        /// </remarks>
        ///
        /// <example>
        /// <code>
        /// string DirectoryPath = "c:\\Documents\\Project7";
        /// using (ZipFile zip = new ZipFile())
        /// {
        ///     zip.AddDirectory(DirectoryPath, System.IO.Path.GetFileName(DirectoryPath));
        ///     zip.Comment = "This will be embedded into a self-extracting console-based exe";
        ///     zip.SaveSelfExtractor("archive.exe", SelfExtractorFlavor.ConsoleApplication);
        /// }
        /// </code>
        /// <code lang="VB">
        /// Dim DirectoryPath As String = "c:\Documents\Project7"
        /// Using zip As New ZipFile()
        ///     zip.AddDirectory(DirectoryPath, System.IO.Path.GetFileName(DirectoryPath))
        ///     zip.Comment = "This will be embedded into a self-extracting console-based exe"
        ///     zip.SaveSelfExtractor("archive.exe", SelfExtractorFlavor.ConsoleApplication)
        /// End Using
        /// </code>
        /// </example>
        ///
        /// <param name="exeToGenerate">
        ///   a pathname, possibly fully qualified, to be created. Typically it
        ///   will end in an .exe extension.</param>
        /// <param name="flavor">
        ///   Indicates whether a Winforms or Console self-extractor is
        ///   desired. </param>
        public void SaveSelfExtractor(string exeToGenerate, SelfExtractorFlavor flavor)
        {
            SelfExtractorSaveOptions options = new SelfExtractorSaveOptions();
            options.Flavor = flavor;
            SaveSelfExtractor(exeToGenerate, options);
        }



        /// <summary>
        ///   Saves the ZipFile instance to a self-extracting zip archive, using
        ///   the specified save options.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   This method saves a self extracting archive, using the specified save
        ///   options. These options include the flavor of the SFX, the default extract
        ///   directory, the icon file, and so on.  See the documentation
        ///   for <see cref="SaveSelfExtractor(string , SelfExtractorFlavor)"/> for more
        ///   details.
        /// </para>
        ///
        /// <para>
        ///   The user who runs the SFX will have the opportunity to change the extract
        ///   directory before extracting. If at the time of extraction, the specified
        ///   directory does not exist, the SFX will create the directory before
        ///   extracting the files.
        /// </para>
        ///
        /// </remarks>
        ///
        /// <example>
        ///   This example saves a WinForms-based self-extracting archive EXE that
        ///   will use c:\ExtractHere as the default extract location. The C# code
        ///   shows syntax for .NET 3.0, which uses an object initializer for
        ///   the SelfExtractorOptions object.
        /// <code>
        /// string DirectoryPath = "c:\\Documents\\Project7";
        /// using (ZipFile zip = new ZipFile())
        /// {
        ///     zip.AddDirectory(DirectoryPath, System.IO.Path.GetFileName(DirectoryPath));
        ///     zip.Comment = "This will be embedded into a self-extracting WinForms-based exe";
        ///     var options = new SelfExtractorOptions
        ///     {
        ///       Flavor = SelfExtractorFlavor.WinFormsApplication,
        ///       DefaultExtractDirectory = "%USERPROFILE%\\ExtractHere",
        ///       PostExtractCommandLine = ExeToRunAfterExtract,
        ///       SfxExeWindowTitle = "My Custom Window Title",
        ///       RemoveUnpackedFilesAfterExecute = true
        ///     };
        ///     zip.SaveSelfExtractor("archive.exe", options);
        /// }
        /// </code>
        /// <code lang="VB">
        /// Dim DirectoryPath As String = "c:\Documents\Project7"
        /// Using zip As New ZipFile()
        ///     zip.AddDirectory(DirectoryPath, System.IO.Path.GetFileName(DirectoryPath))
        ///     zip.Comment = "This will be embedded into a self-extracting console-based exe"
        ///     Dim options As New SelfExtractorOptions()
        ///     options.Flavor = SelfExtractorFlavor.WinFormsApplication
        ///     options.DefaultExtractDirectory = "%USERPROFILE%\\ExtractHere"
        ///     options.PostExtractCommandLine = ExeToRunAfterExtract
        ///     options.SfxExeWindowTitle = "My Custom Window Title"
        ///     options.RemoveUnpackedFilesAfterExecute = True
        ///     zip.SaveSelfExtractor("archive.exe", options)
        /// End Using
        /// </code>
        /// </example>
        ///
        /// <param name="exeToGenerate">The name of the EXE to generate.</param>
        /// <param name="options">provides the options for creating the
        /// Self-extracting archive.</param>
        public void SaveSelfExtractor(string exeToGenerate, SelfExtractorSaveOptions options)
        {
            // Save an SFX that is both an EXE and a ZIP.

            // Check for the case where we are re-saving a zip archive
            // that was originally instantiated with a stream.  In that case,
            // the _name will be null. If so, we set _writestream to null,
            // which insures that we'll cons up a new WriteStream (with a filesystem
            // file backing it) in the Save() method.
            if (_name == null)
                _writestream = null;

            _SavingSfx = true;
            _name = exeToGenerate;
            if (Directory.Exists(_name))
                throw new ZipException("Bad Directory", new System.ArgumentException("That name specifies an existing directory. Please specify a filename.", "exeToGenerate"));
            _contentsChanged = true;
            _fileAlreadyExists = File.Exists(_name);

            _SaveSfxStub(exeToGenerate, options);

            Save();
            _SavingSfx = false;
        }




        private static void ExtractResourceToFile(Assembly a, string resourceName, string filename)
        {
            int n = 0;
            byte[] bytes = new byte[1024];
            using (Stream instream = a.GetManifestResourceStream(resourceName))
            {
                if (instream == null)
                    throw new ZipException(String.Format("missing resource '{0}'", resourceName));

                using (FileStream outstream = File.OpenWrite(filename))
                {
                    do
                    {
                        n = instream.Read(bytes, 0, bytes.Length);
                        outstream.Write(bytes, 0, n);
                    } while (n > 0);
                }
            }
        }


        private void _SaveSfxStub(string exeToGenerate, SelfExtractorSaveOptions options)
        {
            string nameOfIconFile = null;
            string stubExe = null;
            string unpackedResourceDir = null;
            string tmpDir = null;
            try
            {
                if (File.Exists(exeToGenerate))
                {
                    if (Verbose) StatusMessageTextWriter.WriteLine("The existing file ({0}) will be overwritten.", exeToGenerate);
                }
                if (!exeToGenerate.EndsWith(".exe"))
                {
                    if (Verbose) StatusMessageTextWriter.WriteLine("Warning: The generated self-extracting file will not have an .exe extension.");
                }

                // workitem 10553
                tmpDir = TempFileFolder ?? Path.GetDirectoryName(exeToGenerate);
                stubExe = GenerateTempPathname(tmpDir, "exe");

                // get the Ionic.Zip assembly
                Assembly a1 = typeof(ZipFile).Assembly;

                using (var csharp = new Microsoft.CSharp.CSharpCodeProvider
                       (new Dictionary<string,string>() { { "CompilerVersion", "v2.0" } })) {

                    // The following is a perfect opportunity for a linq query, but
                    // I cannot use it.  DotNetZip needs to run on .NET 2.0,
                    // and using LINQ would break that. Here's what it would look
                    // like:
                    //
                    //   var settings = (from x in SettingsList
                    //                   where x.Flavor == flavor
                    //                   select x).First();

                    ExtractorSettings settings = null;
                    foreach (var x in SettingsList)
                    {
                        if (x.Flavor == options.Flavor)
                        {
                            settings = x;
                            break;
                        }
                    }

                    // sanity check; should never happen
                    if (settings == null)
                        throw new BadStateException(String.Format("While saving a Self-Extracting Zip, Cannot find that flavor ({0})?", options.Flavor));

                    // This is the list of referenced assemblies.  Ionic.Zip is
                    // needed here.  Also if it is the winforms (gui) extractor, we
                    // need other referenced assemblies, like
                    // System.Windows.Forms.dll, etc.
                    var cp = new System.CodeDom.Compiler.CompilerParameters();
                    cp.ReferencedAssemblies.Add(a1.Location);
                    if (settings.ReferencedAssemblies != null)
                        foreach (string ra in settings.ReferencedAssemblies)
                            cp.ReferencedAssemblies.Add(ra);

                    cp.GenerateInMemory = false;
                    cp.GenerateExecutable = true;
                    cp.IncludeDebugInformation = false;
                    cp.CompilerOptions = "";

                    Assembly a2 = Assembly.GetExecutingAssembly();

                    // Use this to concatenate all the source code resources into a
                    // single module.
                    var sb = new System.Text.StringBuilder();

                    // In case there are compiler errors later, we allocate a source
                    // file name now. If errors are detected, we'll spool the source
                    // code as well as the errors (in comments) into that filename,
                    // and throw an exception with the filename.  Makes it easier to
                    // diagnose.  This should be rare; most errors happen only
                    // during devlpmt of DotNetZip itself, but there are rare
                    // occasions when they occur in other cases.
                    string sourceFile = GenerateTempPathname(tmpDir, "cs");


                    // // debugging: enumerate the resources in this assembly
                    // Console.WriteLine("Resources in this assembly:");
                    // foreach (string rsrc in a2.GetManifestResourceNames())
                    //   {
                    //     Console.WriteLine(rsrc);
                    //   }
                    // Console.WriteLine();


                    // all the source code is embedded in the DLL as a zip file.
                    using (ZipFile zip = ZipFile.Read(a2.GetManifestResourceStream("Ionic.Zip.Resources.ZippedResources.zip")))
                    {
                        // // debugging: enumerate the files in the embedded zip
                        // Console.WriteLine("Entries in the embbedded zip:");
                        // foreach (ZipEntry entry in zip)
                        //   {
                        //     Console.WriteLine(entry.FileName);
                        //   }
                        // Console.WriteLine();

                        unpackedResourceDir = GenerateTempPathname(tmpDir, "tmp");

                        if (String.IsNullOrEmpty(options.IconFile))
                        {
                            // Use the ico file that is embedded into the Ionic.Zip
                            // DLL itself.  To do this we must unpack the icon to
                            // the filesystem, in order to specify it on the cmdline
                            // of csc.exe.  This method will remove the unpacked
                            // file later.
                            System.IO.Directory.CreateDirectory(unpackedResourceDir);
                            ZipEntry e = zip["zippedFile.ico"];
                            // Must not extract a readonly file - it will be impossible to
                            // delete later.
                            if ((e.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                                e.Attributes ^= FileAttributes.ReadOnly;
                            e.Extract(unpackedResourceDir);
                            nameOfIconFile = Path.Combine(unpackedResourceDir, "zippedFile.ico");
                            cp.CompilerOptions += String.Format("/win32icon:\"{0}\"", nameOfIconFile);
                        }
                        else
                            cp.CompilerOptions += String.Format("/win32icon:\"{0}\"", options.IconFile);

                        cp.OutputAssembly = stubExe;

                        if (options.Flavor == SelfExtractorFlavor.WinFormsApplication)
                            cp.CompilerOptions += " /target:winexe";

                        if (!String.IsNullOrEmpty(options.AdditionalCompilerSwitches))
                            cp.CompilerOptions += " " + options.AdditionalCompilerSwitches;

                        if (String.IsNullOrEmpty(cp.CompilerOptions))
                            cp.CompilerOptions = null;

                        if ((settings.CopyThroughResources != null) && (settings.CopyThroughResources.Count != 0))
                        {
                            if (!Directory.Exists(unpackedResourceDir)) System.IO.Directory.CreateDirectory(unpackedResourceDir);
                            foreach (string re in settings.CopyThroughResources)
                            {
                                string filename = Path.Combine(unpackedResourceDir, re);

                                ExtractResourceToFile(a2, re, filename);
                                // add the file into the target assembly as an embedded resource
                                cp.EmbeddedResources.Add(filename);
                            }
                        }

                        // add the Ionic.Utils.Zip DLL as an embedded resource
                        cp.EmbeddedResources.Add(a1.Location);

                        // file header
                        sb.Append("// " + Path.GetFileName(sourceFile) + "\n")
                            .Append("// --------------------------------------------\n//\n")
                            .Append("// This SFX source file was generated by DotNetZip ")
                            .Append(ZipFile.LibraryVersion.ToString())
                            .Append("\n//         at ")
                            .Append(System.DateTime.Now.ToString("yyyy MMMM dd  HH:mm:ss"))
                            .Append("\n//\n// --------------------------------------------\n\n\n");

                        // assembly attributes
                        if (!String.IsNullOrEmpty(options.Description))
                            sb.Append("[assembly: System.Reflection.AssemblyTitle(\""
                                      + options.Description.Replace("\"", "")
                                      + "\")]\n");
                        else
                            sb.Append("[assembly: System.Reflection.AssemblyTitle(\"DotNetZip SFX Archive\")]\n");

                        if (!String.IsNullOrEmpty(options.ProductVersion))
                            sb.Append("[assembly: System.Reflection.AssemblyInformationalVersion(\""
                                      + options.ProductVersion.Replace("\"", "")
                                      + "\")]\n");

                        // workitem
                        string copyright =
                            (String.IsNullOrEmpty(options.Copyright))
                            ? "Extractor: Copyright © Dino Chiesa 2008-2011"
                            : options.Copyright.Replace("\"", "");

                        if (!String.IsNullOrEmpty(options.ProductName))
                            sb.Append("[assembly: System.Reflection.AssemblyProduct(\"")
                                .Append(options.ProductName.Replace("\"", ""))
                                .Append("\")]\n");
                        else
                            sb.Append("[assembly: System.Reflection.AssemblyProduct(\"DotNetZip\")]\n");


                        sb.Append("[assembly: System.Reflection.AssemblyCopyright(\"" + copyright + "\")]\n")
                            .Append(String.Format("[assembly: System.Reflection.AssemblyVersion(\"{0}\")]\n", ZipFile.LibraryVersion.ToString()));
                        if (options.FileVersion != null)
                            sb.Append(String.Format("[assembly: System.Reflection.AssemblyFileVersion(\"{0}\")]\n",
                                                    options.FileVersion.ToString()));

                        sb.Append("\n\n\n");

                        // Set the default extract location if it is available
                        string extractLoc = options.DefaultExtractDirectory;
                        if (extractLoc != null)
                        {
                            // remove double-quotes and replace slash with double-slash.
                            // This, because the value is going to be embedded into a
                            // cs file as a quoted string, and it needs to be escaped.
                            extractLoc = extractLoc.Replace("\"", "").Replace("\\", "\\\\");
                        }

                        string postExCmdLine = options.PostExtractCommandLine;
                        if (postExCmdLine != null)
                        {
                            postExCmdLine = postExCmdLine.Replace("\\", "\\\\");
                            postExCmdLine = postExCmdLine.Replace("\"", "\\\"");
                        }


                        foreach (string rc in settings.ResourcesToCompile)
                        {
                            using (Stream s = zip[rc].OpenReader())
                            {
                                if (s == null)
                                    throw new ZipException(String.Format("missing resource '{0}'", rc));
                                using (StreamReader sr = new StreamReader(s))
                                {
                                    while (sr.Peek() >= 0)
                                    {
                                        string line = sr.ReadLine();
                                        if (extractLoc != null)
                                            line = line.Replace("@@EXTRACTLOCATION", extractLoc);

                                        line = line.Replace("@@REMOVE_AFTER_EXECUTE", options.RemoveUnpackedFilesAfterExecute.ToString());
                                        line = line.Replace("@@QUIET", options.Quiet.ToString());
                                        if (!String.IsNullOrEmpty(options.SfxExeWindowTitle))

                                            line = line.Replace("@@SFX_EXE_WINDOW_TITLE", options.SfxExeWindowTitle);

                                        line = line.Replace("@@EXTRACT_EXISTING_FILE", ((int)options.ExtractExistingFile).ToString());

                                        if (postExCmdLine != null)
                                            line = line.Replace("@@POST_UNPACK_CMD_LINE", postExCmdLine);

                                        sb.Append(line).Append("\n");
                                    }
                                }
                                sb.Append("\n\n");
                            }
                        }
                    }

                    string LiteralSource = sb.ToString();

#if DEBUGSFX
                    // for debugging only
                    string sourceModule = GenerateTempPathname(tmpDir, "cs");
                    using (StreamWriter sw = File.CreateText(sourceModule))
                    {
                        sw.Write(LiteralSource);
                    }
                    Console.WriteLine("source: {0}", sourceModule);
#endif

                    var cr = csharp.CompileAssemblyFromSource(cp, LiteralSource);


                    if (cr == null)
                        throw new SfxGenerationException("Cannot compile the extraction logic!");

                    if (Verbose)
                        foreach (string output in cr.Output)
                            StatusMessageTextWriter.WriteLine(output);

                    if (cr.Errors.Count != 0)
                    {
                        using (TextWriter tw = new StreamWriter(sourceFile))
                        {
                            // first, the source we compiled
                            tw.Write(LiteralSource);

                            // now, append the compile errors
                            tw.Write("\n\n\n// ------------------------------------------------------------------\n");
                            tw.Write("// Errors during compilation: \n//\n");
                            string p = Path.GetFileName(sourceFile);

                            foreach (System.CodeDom.Compiler.CompilerError error in cr.Errors)
                            {
                                tw.Write(String.Format("//   {0}({1},{2}): {3} {4}: {5}\n//\n",
                                                       p,                                   // 0
                                                       error.Line,                          // 1
                                                       error.Column,                        // 2
                                                       error.IsWarning ? "Warning" : "error",   // 3
                                                       error.ErrorNumber,                   // 4
                                                       error.ErrorText));                  // 5
                            }
                        }
                        throw new SfxGenerationException(String.Format("Errors compiling the extraction logic!  {0}", sourceFile));
                    }

                    OnSaveEvent(ZipProgressEventType.Saving_AfterCompileSelfExtractor);

                    // Now, copy the resulting EXE image to the _writestream.
                    // Because this stub exe is being saved first, the effect will be to
                    // concatenate the exe and the zip data together.
                    using (System.IO.Stream input = System.IO.File.OpenRead(stubExe))
                    {
                        byte[] buffer = new byte[4000];
                        int n = 1;
                        while (n != 0)
                        {
                            n = input.Read(buffer, 0, buffer.Length);
                            if (n != 0)
                                WriteStream.Write(buffer, 0, n);
                        }
                    }
                }

                OnSaveEvent(ZipProgressEventType.Saving_AfterSaveTempArchive);
            }
            finally
            {
                try
                {
                    if (Directory.Exists(unpackedResourceDir))
                    {
                        try { Directory.Delete(unpackedResourceDir, true); }
                        catch (System.IO.IOException exc1)
                        {
                            StatusMessageTextWriter.WriteLine("Warning: Exception: {0}", exc1);
                        }
                    }
                    if (File.Exists(stubExe))
                    {
                        try { File.Delete(stubExe); }
                        catch (System.IO.IOException exc1)
                        {
                            StatusMessageTextWriter.WriteLine("Warning: Exception: {0}", exc1);
                        }
                    }
                }
                catch (System.IO.IOException) { }
            }

            return;

        }



        internal static string GenerateTempPathname(string dir, string extension)
        {
            string candidate = null;
            String AppName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            do
            {
                // workitem 13475
                string uuid = System.Guid.NewGuid().ToString();

                string Name = String.Format("{0}-{1}-{2}.{3}",
                        AppName, System.DateTime.Now.ToString("yyyyMMMdd-HHmmss"),
                                            uuid, extension);
                candidate = System.IO.Path.Combine(dir, Name);
            } while (System.IO.File.Exists(candidate) || System.IO.Directory.Exists(candidate));

            // The candidate path does not exist as a file or directory.
            // It can now be created, as a file or directory.
            return candidate;
        }

    }
#endif
}
