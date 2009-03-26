using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;

namespace Test_NUnit_MySql
{
    [TestFixture]
    public class MetalTest
    {
        static string GetSqlMetalPath()
        {
            string path = "../../../SqlMetal/bin/SqlMetal.exe";
            if (!File.Exists(path))
                throw new NUnit.Framework.IgnoreException("SqlMetal not found");
            return path;
        }

        static string GetCompilerPath()
        {
            string windowsDir = Environment.GetEnvironmentVariable("SystemRoot");
            string frameworkDir = Path.Combine(windowsDir, "Microsoft.Net/Framework/v3.5");
            if (!Directory.Exists(frameworkDir))
                throw new NUnit.Framework.IgnoreException("Framework dir not found");

            string cscExe = Path.Combine(frameworkDir, "csc.exe");
            if (!File.Exists(cscExe))
                throw new NUnit.Framework.IgnoreException("csc.exe not found in framework dir");
            return cscExe;
        }

        [Test]
        public void GenerateFromDbml()
        {
            //1. gather prerequisites (compiler, sqlmetal etc)
            string cscExe = GetCompilerPath();
            string sqlMetal = GetSqlMetalPath();

            //2. run SqlMetal to generate 'bin/Northwind_temp.cs'
            string currDir = Directory.GetCurrentDirectory();
            string mysqlExampleDir = "../../../Example/DbLinq.Mysql.Example/nwind";
            bool ok = Directory.Exists(mysqlExampleDir);
            string args1 = string.Format(" -provider=MySql -namespace:nwind -code:Northwind_temp.cs -sprocs {0}/Northwind_from_mysql.dbml"
                , mysqlExampleDir);

            ProcessRunner p1 = new ProcessRunner();
            int sqlMetalExitCode = p1.Run(sqlMetal, args1, 5000);
            Assert.IsTrue(sqlMetalExitCode == 0, "Got SqlMetal.exe error exit code " + sqlMetalExitCode);

            //3. make sure generated code compiles with 'bin/Northwind_temp.cs'
            Directory.SetCurrentDirectory("..");
            string dependencies = @"/r:bin\nunit.framework.dll /r:bin\DbLinq.dll /r:bin\DbLinq.mysql.dll /r:bin\Mysql.data.dll";
            string cscArgs = @"/nologo /target:library /d:MYSQL /out:bin/SqlMetal_test.dll  bin/Northwind_temp.cs  ReadTest.cs  WriteTest.cs  TestBase.cs  "
                + dependencies;

            ProcessRunner p2 = new ProcessRunner();
            int cscExitCode = p2.Run(cscExe, cscArgs, 5000);
            Console.Out.WriteLine("csc exitCode:" + cscExitCode + ",  output: " + p2._stdout);
            Assert.IsTrue(cscExitCode == 0, "csc.exe failed with exit code " + cscExitCode);

            Directory.SetCurrentDirectory(currDir);
        }

        [Test]
        public void GetHashCode_should_not_throw()
        {
            nwind.Customer customer = new nwind.Customer();

            //bug: GetHashCode sometimes throws NullPointerException because of null _customerID
            int hashCode = customer.GetHashCode();
        }

        #region helper class ProcessRunner - launches jobs, reads output
        class ProcessRunner
        {
            delegate void StreamHandler(StreamReader s, bool isStdout);

            public string _stdout;
            public string _stderr;

            public int Run(string exe, string args, int timeout)
            {
                ProcessStartInfo psi = new ProcessStartInfo(exe, args);
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                Process p = Process.Start(psi);
                new StreamHandler(readOutput).BeginInvoke(p.StandardOutput, true, null, null);
                new StreamHandler(readOutput).BeginInvoke(p.StandardError, false, null, null);
                bool exitOk = p.WaitForExit(timeout);
                Assert.IsTrue(exitOk, "Expected app to exit cleanly");
                return p.ExitCode;
            }

            void readOutput(StreamReader reader, bool isStdout)
            {
                try
                {
                    string result = reader.ReadToEnd();

                    if (isStdout)
                        _stdout = result;
                    else
                        _stderr = result;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("read failed: " + ex);
                }
            }
        }
        #endregion

    }
}
