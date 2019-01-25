using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab3
{
    class Archiving
    {
        int symbol;
        BufferedStream bufferfilereader;
        FileStream filestream;
        StringBuilder builder;
        string[] symbolcodes;
        string writebyte;
        Tree methods;
        BinaryWriter binarywriter;
        string waytofile;
        FileStream arhfile;
        FileStream treefile;
        int iter;
        public Archiving()
        {
            waytofile= @"D:\2 курс\Tik\lab3\Oxxxymiron – Город под подошвой.mp3";
            filestream = new FileStream(waytofile,FileMode.Open,FileAccess.Read);
            bufferfilereader = new BufferedStream(filestream);
            arhfile = new FileStream(@"D:\2 курс\Tik\lab3\file.mp3", FileMode.Create,FileAccess.Write);
            treefile = new FileStream(@"D:\2 курс\Tik\lab3\tree.txt", FileMode.Create, FileAccess.Write);
            binarywriter = new BinaryWriter(treefile);
            symbolcodes = new string[256];
            builder = new StringBuilder();
        }
        public void GetByteThread(long[] list)
        {
            symbol = bufferfilereader.ReadByte();
            while (symbol!=-1)
            {
                list[symbol] += 1;
                symbol = bufferfilereader.ReadByte();
            }
            bufferfilereader.Position = 0;
        }
        public void WriteThread(BinaryTreeNode head)
        {
            methods = new Tree(head);
            foreach(var item in methods)
            { Console.WriteLine(item); }
            iter = 0;
            symbolcodes = methods.BinaryCodes;
            symbol = bufferfilereader.ReadByte();
            while (symbol!=-1)
            {
                builder.Append(symbolcodes[symbol]);
                symbol = bufferfilereader.ReadByte();
            }
            Console.WriteLine("String is ready");
            writebyte = builder.ToString();
            binarywriter.Write(writebyte);
            //bufferfilereader = new BufferedStream(treefile);
            #region Ускорить
            for (int i = 0; i < writebyte.Length / 8; i++)
            {
                builder.Clear();
                for (int x = 0; x < 8; x++)
                {
                    builder.Append(writebyte[i * 8 + x]);
                }
                arhfile.WriteByte(Convert.ToByte(builder.ToString(), 2));
            }
            #endregion
            builder.Clear();
            Console.WriteLine(iter);
            iter = (writebyte.Length / 8)*8+1;
            /////////////////////////////////////////////Запись последнего байта
            while(iter!=writebyte.Length)
            {
                builder.Append(writebyte[iter-1]);
                iter++;
            }
            iter = 0;
            while(builder.Length!=8)
            {
                builder.Append(1);
                iter++;
            }
            //////////////////////////////////////////////
            arhfile.WriteByte(Convert.ToByte(builder.ToString(), 2));
            Console.WriteLine(iter);
            Console.WriteLine("end");
            arhfile.Dispose();
            
            treefile.Dispose();
        }
    }
    class Decompression
    {
        int symbol;
        FileStream fullfile;
        string waytofile;
        FileStream txtfile;
        List<BinaryTreeNode> tree;
        BufferedStream reader;
        BinaryTreeNode current;
        public Decompression(List<BinaryTreeNode> list)
        {
            waytofile = @"D:\2 курс\Tik\lab3\tree.txt";
            tree = list;
            txtfile = new FileStream(waytofile,FileMode.Open,FileAccess.Read);
            reader = new BufferedStream(txtfile);
            fullfile = new FileStream(@"D:\2 курс\Tik\lab3\file2.mp3",FileMode.Create,FileAccess.Write);
            current = tree[0];
        }
        
        public void ReadFile()
        {
            symbol = reader.ReadByte();
            while (symbol!=-1)
            {
                if (symbol == '1')
                {
                    current = current.Right;
                    if (current.Name != -1)
                    {
                        fullfile.WriteByte((byte)current.Name);
                        current = tree[0];
                    }
                }
                else if (symbol == '0')
                {
                    current = current.Left;
                    if (current.Name != -1)
                    {
                        fullfile.WriteByte((byte)current.Name);
                        current = tree[0];
                    }
                }
                symbol = reader.ReadByte();
            }
            Console.WriteLine("ENDDDD");
        }

    }
    class Tree : IEnumerable
    {
        BinaryTreeNode _head;
        public string[] BinaryCodes
        {
            get;
        }
        public IEnumerator GetEnumerator()
        {
            return InOrderTraversal();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public Tree(BinaryTreeNode _head)
        {
            BinaryCodes = new string[256];
            this._head = _head;
        }
        IEnumerator InOrderTraversal()
        {
            if (_head != null)
            {
                Stack<BinaryTreeNode> stack = new Stack<BinaryTreeNode>();
                BinaryTreeNode current = _head;
                bool goLeftNext = true;
                stack.Push(current);
                while (stack.Count > 0)
                {
                    if (goLeftNext)
                    {
                        while (current.Left != null)
                        {
                            stack.Push(current);
                            current.Left.Code = current.Code + '0';
                            current = current.Left;
                        }
                    }
                    if (current.Name!=-1)
                    {
                        BinaryCodes[current.Name] = current.Code;
                        yield return current.Code;
                    }
                    if (current.Right != null)
                    {
                        current.Right.Code = current.Code + '1';
                        current = current.Right;
                        goLeftNext = true;
                    }
                    else
                    {
                        current = stack.Pop();
                        goLeftNext = false;
                    }
                }
            }
        }
    }
    class BinarySort : IComparer<BinaryTreeNode>
    {
        public int Compare(BinaryTreeNode x, BinaryTreeNode y)
        {
            return x.Value.CompareTo(y.Value);
        }
    }
    class BinaryTreeNode
    {
        private string code;
        public string Code
        {
            get { return code; }
            set { code = value; }
        }
        public BinaryTreeNode Parent { get; set; }
        public BinaryTreeNode(long value, int name)
        {
            Value = value;
            Name = name;
        }
        public BinaryTreeNode Right { get; set; }
        public BinaryTreeNode Left { get; set; }
        public long Value { get; private set; }
        public int Name { get; set; }

    }
    class Program
    {
        static void Main(string[] args)
        {
            int iter=0;
            Archiving write = new Archiving();
            BinaryTreeNode _head;
            BinarySort binarysort = new BinarySort();
            BinaryTreeNode newnode;
            List<BinaryTreeNode> list = new List<BinaryTreeNode>();
            long[] freequency = new long[256];
            write.GetByteThread(freequency);
            foreach (var element in freequency)
            {
                if (element != 0)
                {
                    list.Add(new BinaryTreeNode(element,(char)iter));
                }
                iter++;
            }
            list.Sort(binarysort);
            while (list.Count != 1)
            {
                newnode = new BinaryTreeNode(list[0].Value + list[1].Value, -1);
                list.Add(newnode);
                newnode.Left = list[1];
                newnode.Right = list[0];
                list[0].Parent = newnode;
                list[1].Parent = newnode;
                list.Remove(list[0]);
                list.Remove(list[0]);
                list.Sort(binarysort);
            }
            _head = list[0];
            write.WriteThread(_head);
            Decompression read = new Decompression(list);
            read.ReadFile();
            Console.ReadKey();

        }
    }
}
