using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace object_traversal
{
    class Program2 : Program
    {
        public static Node n;

    }
    class Program
    {
        [DllImport("__Internal")]
        static extern IntPtr mono_unity_liveness_calculation_from_statics_managed(IntPtr typeHandle);

        [DllImport("__Internal")]
        static extern IntPtr mono_unity_liveness_calculation_from_root_managed(IntPtr rootHandle, IntPtr typeHandle);

        static void Main(string[] args)
        {
            Test1();
            Test2();
            Test3();
            Test4();
            Test5();
            Test6();
            Test7();
            Test8();
        }

        static void VerifyObjects( string label, int expectedCount)
        {
            IntPtr typeHandle = (IntPtr)GCHandle.Alloc(typeof(Node));
            IntPtr gchandle = mono_unity_liveness_calculation_from_statics_managed(typeHandle);

            object[] vals = (object[])((GCHandle)gchandle).Target;

            ((GCHandle)typeHandle).Free();
            ((GCHandle)gchandle).Free();
            if (vals.Length != expectedCount)
            {
                Console.WriteLine("FAILED: {0} expected {1} actual {2}", label, expectedCount, vals.Length);
            }
            else
            {
                Console.WriteLine("PASSED: {0}", label);
            }
        }

        static void VerifyObjects(string label, int expectedCount, object root)
        {
            IntPtr typeHandle = (IntPtr)GCHandle.Alloc(typeof(Node));
            IntPtr rootHandle = (IntPtr)GCHandle.Alloc(root);
            IntPtr gchandle = mono_unity_liveness_calculation_from_root_managed(rootHandle, typeHandle);

            object[] vals = (object[])((GCHandle)gchandle).Target;

            if (typeHandle != IntPtr.Zero)
                ((GCHandle)typeHandle).Free();
            ((GCHandle)rootHandle).Free();
            ((GCHandle)gchandle).Free();
            if (vals.Length != expectedCount)
            {
                Console.WriteLine("FAILED: {0} expected {1} actual {2}", label, expectedCount, vals.Length);
            }
            else
            {
                Console.WriteLine("PASSED: {0}", label);
            }
        }

        static void Test1()
        {
            b = "a" + 1.ToString();
            n = new Node();
            n2 = new Node(new Node(), n);

            VerifyObjects("Test1", 3);
        }

        static void Test2()
        {
            b = "a" + 1.ToString();
            n = new Node();
            n2 = new Node(new Node(), new Node());

            VerifyObjects("Test2", 4);
        }

        static void Test3()
        {
            b = "a" + 1.ToString();
            n = new NodeDerived();
            n2 = new Node(new NodeDerived(), null);
            nnot = new NodeNotDerived();

            VerifyObjects("Test3", 3);
        }

        static void Test4()
        {
            Node root = new Node();

            VerifyObjects("Test4", 1, root);
        }

        static void Test5()
        {
            Node root = new Node(new Node(), new NodeDerived());

            VerifyObjects("Test5", 3, root);
        }

        static void Test6()
        {
            Node useMeTwice = new Node();
            Node root = new Node(useMeTwice, useMeTwice);

            VerifyObjects("Test6", 2, root);
        }

        static void Test7()
        {
            Node useMeTwice = new Node();
            Node root = new NodeDerived(useMeTwice, useMeTwice, new Node());

            VerifyObjects("Test7", 3, root);
        }

        static void Test8()
        {
            Program.n = null;
            Program.n2 = null;
            Program.nnot = null;
            Program2.n = new Node();

            VerifyObjects("Test8", 1);
        }

        static int a = 0;
        static string b;
        public static Node n;
        public static Node n2;
        public static NodeNotDerived nnot;
    }

    class Node
    {
        Node left;
        Node right;

        public Node()
        {
        }

        public Node(Node l, Node r)
        {
            this.left = l;
            this.right = r;
        }
    }

    class NodeDerived : Node
    {
        Node d;

        public NodeDerived()
        {
        }

        public NodeDerived(Node l, Node r, Node d) : base (l, r)
        {
            this.d = d;
        }
    }

    class NodeNotDerived
    {
    }
}
