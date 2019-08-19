using System;
using System.Collections.Generic;
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

            var model = Model.FromFile(args[0]);
            model.Write(args[0] + ".pm3d");
        }
    }
}