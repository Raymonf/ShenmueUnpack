using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ShenmueArchiveUnpack
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Path: ");
            var path = Console.ReadLine().Replace("\"", "").Replace(".tad", "").Replace(".tac", "");

            var listing = File.OpenRead(path + ".tad");
            var data = File.OpenRead(path + ".tac");

            using (var dataReader = new BinaryReader(data))
            {
                using (var listReader = new BinaryReader(listing))
                {
                    // TAD header
                    listReader.ReadBytes(0x2C); // skip

                    // Root entry
                    listReader.ReadInt32(); // 0?

                    var numFiles = listReader.ReadUInt64();
                    listReader.ReadUInt32(); // numFiles again but 32bit?

                    for (ulong i = 0; i < numFiles; i++)
                    {
                        var hash = BitConverter.ToString(listReader.ReadBytes(16)).Replace("-", "").ToLower();
                        //listReader.ReadBytes(12); // skip 12
                        
                        var address = listReader.ReadInt64();
                        //Console.WriteLine("Address: {0}", address);

                        var size = listReader.ReadInt64();

                        dataReader.BaseStream.Seek(address, SeekOrigin.Begin);
                        var bytes = dataReader.ReadBytes((int)size);

                        var ext = ".bin";
                        if (bytes.Length >= 3 && bytes[0] == 'D' && bytes[1] == 'D' && bytes[2] == 'S')
                        {
                            ext = ".dds";
                        }
                        else if (bytes.Length >= 4 && bytes[0] == 'D' && bytes[1] == 'X' && bytes[2] == 'B' && bytes[3] == 'C')
                        {
                            ext = ".hlsl";
                        }

                        File.WriteAllBytes(hash + ext, bytes);

                        Console.WriteLine($"{i + 1}/{numFiles}");
                    }
                }

                Console.ReadKey();
            }
        }
    }
}
