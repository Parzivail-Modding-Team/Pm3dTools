using System;
using System.IO;

namespace Pm3dTools
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Expected filename.");
                return;
            }

            args[0] = Path.GetDirectoryName(args[0]) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(args[0]);

            var model = Pm3DModel.FromFile(args[0]);
            model.Write(args[0] + ".pm3d");
        }
    }
}