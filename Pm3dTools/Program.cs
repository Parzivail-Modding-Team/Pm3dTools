using System;
using System.Collections.Generic;
using System.IO;
using JeremyAnsel.Media.WavefrontObj;

namespace Pm3dTools
{
	class Program
	{
		private const float ModelScaleFactor = 1; //10 / 16f;

		static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				Console.WriteLine("Expected at least one argument.");
				return;
			}
			
			// const int texWidth = 128;
			// const int texHeight = 128;
			//
			// const float quantX = 1f / texWidth;
			// const float quantY = 1f / texHeight;
			//
			// foreach (var file in Directory.GetFiles(args[0], "*.pm3d"))
			// {
			// 	var model = Pm3DModel.FromFile(file);
			//
			// 	foreach (var lod in model.Lods)
			// 	{
			// 		for (var i = 0; i < lod.Uvs.Count; i++)
			// 		{
			// 			var uv = lod.Uvs[i];
			// 			var u = SnapIfClose(uv.X, quantX);
			// 			var v = SnapIfClose(uv.Y, quantY);
			// 	
			// 			Console.WriteLine($"{uv.X},{uv.Y} ({uv.X * texWidth},{uv.Y * texHeight}) -> {u},{v} ({u * texWidth},{v * texHeight})");
			// 	
			// 			lod.Uvs[i] = new ObjVector3(u, v, 0);
			// 		}
			// 	}
			//
			// 	model.Write(file, 1);
			//
			// 	Console.WriteLine($"Wrote {file}");
			// }
			
			var model = Pm3DModel.FromObjLods(args);
			model.Write(Path.Combine(Path.GetDirectoryName(args[0])!, Path.GetFileNameWithoutExtension(args[0])!) + ".pm3d", ModelScaleFactor);
			
			Console.WriteLine("Done");
		}

		private static float SnapIfClose(float x, float n)
		{
			var fract = (x / n) % 1;
			if (fract < 0.1 || fract > 0.9)
				return MathF.Round(x / n) * n;
			return x;
		}
	}
}