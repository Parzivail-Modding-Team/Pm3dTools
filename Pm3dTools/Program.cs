using System;
using System.IO;

namespace Pm3dTools
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Expected at least one filename.");
                return;
            }

            var model = Pm3DModel.FromObjLods(args);
            model.Write(Path.Combine(Path.GetDirectoryName(args[0])!, Path.GetFileNameWithoutExtension(args[0])!) + ".pm3d");
        }
    }
}