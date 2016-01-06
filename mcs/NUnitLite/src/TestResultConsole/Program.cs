using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace TestResultConsole.cs
{
    class Program
    {
        static int port = 9000;

        static void Main(string[] args)
        {
            TcpListener listener = new TcpListener(port);

            listener.Start();
            Console.WriteLine("Waiting for test to begin...");
            TcpClient client = listener.AcceptTcpClient();

            Console.WriteLine("Connected to test runner...");
            Console.WriteLine();

            NetworkStream ns = client.GetStream();
            TextReader rdr = new StreamReader(ns);

            try
            {
                while (client.Connected)
                {
                    string data = rdr.ReadLine();
                    Console.WriteLine(data);
                }
            }
            catch (IOException e)
            {
                if (client.Connected)
                    Console.WriteLine(e.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            client.Close();
            listener.Stop();
        }
    }
}

