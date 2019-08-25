using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlockbenchToPmodel.Blockbench;
using BlockbenchToPmodel.PM3D;

namespace BlockbenchToPmodel
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

            var model = Model.FromFile(args[0]);
            model.Write(args[0] + ".pm3d");
        }
    }
}