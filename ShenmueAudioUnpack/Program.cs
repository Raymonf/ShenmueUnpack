using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ShenmueAudioUnpack
{
    class Program
    {
        static uint BytesToAddress(byte[] b)
        {
            return (uint)(b[3] << 24 | b[2] << 16 | b[1] << 8 | b[0]);
        }

        static void Main(string[] args)
        {
            Console.Write("Path: ");
            var path = Console.ReadLine().Replace("\"", "");
            var file = File.OpenRead(path);

            var pointerList = new List<SoundPointer>();

            using (var reader = new BinaryReader(file))
            {
                var header = reader.ReadUInt32();
                if (header != 0x00534641)
                {
                    throw new Exception("Not a Shenmue AFS file.");
                }

                var numFiles = reader.ReadUInt32();
                for (uint i = 0; i < numFiles; i++)
                {
                    pointerList.Add(new SoundPointer()
                    {
                        Address = BytesToAddress(reader.ReadBytes(4)),
                        Size = BytesToAddress(reader.ReadBytes(4))
                    });
                }

                for (int i = 0; i < pointerList.Count; i++)
                {
                    var ptr = pointerList[i];
                    reader.BaseStream.Seek(ptr.Address, SeekOrigin.Begin);

                    var bytes = reader.ReadBytes((int)ptr.Size);

                    if (bytes[0] == 'R' && bytes[1] == 'I' && bytes[2] == 'F' && bytes[3] == 'F')
                    {
                        // Should be audio
                        File.WriteAllBytes(i + ".xwma", bytes);
                    }
                    else
                    {
                        // Might be subtitle data
                        File.WriteAllBytes(i + ".bin", bytes);
                    }

                    Console.WriteLine($"{i + 1}/{pointerList.Count}");
                }
            }
            
            Console.ReadKey();
        }
    }
}
