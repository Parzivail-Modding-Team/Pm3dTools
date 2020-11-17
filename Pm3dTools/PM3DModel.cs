using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using JeremyAnsel.Media.WavefrontObj;

namespace Pm3dTools
{
	class Pm3DLod
	{
		public readonly Dictionary<string, List<ObjFace>> Objects;
		public readonly IList<ObjVertex> Verts;
		public readonly IList<ObjVector3> Normals;
		public readonly IList<ObjVector3> Uvs;

		public Pm3DLod(Dictionary<string, List<ObjFace>> objects, IList<ObjVertex> verts, IList<ObjVector3> normals, IList<ObjVector3> uvs)
		{
			Objects = objects;
			Verts = verts;
			Normals = normals;
			Uvs = uvs;
		}
	}

    class Pm3DModel
    {
        private const int FileVersion = 0x03;
        private readonly byte[] _headerMagic = { (byte)'P', (byte)'m', (byte)'3', (byte)'D' };

		private readonly IList<Pm3DLod> _lods;

		private Pm3DModel(IList<Pm3DLod> lods)
        {
            _lods = lods;
        }

        public static Pm3DModel FromObjLods(params string[] filenames)
        {
			var lods = new List<Pm3DLod>();

			foreach (var filename in filenames)
			{
				var obj = ObjFile.FromFile(filename);

				var objects = obj
					.Faces
					.GroupBy(face => face.ObjectName)
					.ToDictionary(grouping => grouping.Key, grouping => grouping.ToList());

				var verts = obj.Vertices;
				var normals = obj.VertexNormals;
				var uvs = obj.TextureVertices;

				lods.Add(new Pm3DLod(objects, verts, normals, uvs));
			}

            return new Pm3DModel(lods);
        }

        public void Write(string filename)
        {
	        using var b = new BinaryWriter(File.Open(filename, FileMode.Create));

	        b.Write(_headerMagic);
	        b.Write(FileVersion);

			b.Write(_lods.Count);

			foreach (var lod in _lods)
			{
				b.Write(lod.Verts.Count);
				b.Write(lod.Normals.Count);
				b.Write(lod.Uvs.Count);
				b.Write(lod.Objects.Count);

				foreach (var vert in lod.Verts) WriteVert(b, vert);
				foreach (var norm in lod.Normals) WriteNormal(b, norm);
				foreach (var uv in lod.Uvs) WriteUv(b, uv);

				WriteObjects(b, lod.Objects);
			}
        }

        private static void WriteVert(BinaryWriter b, ObjVertex vert)
        {
            b.Write(vert.Position.X);
            b.Write(vert.Position.Y);
            b.Write(vert.Position.Z);
        }

        private static void WriteNormal(BinaryWriter b, ObjVector3 norm)
        {
            b.Write(norm.X);
            b.Write(norm.Y);
            b.Write(norm.Z);
        }

        private static void WriteUv(BinaryWriter b, ObjVector3 uv)
        {
            b.Write(uv.X);
            b.Write(uv.Y);
        }

        private static void WriteObjects(BinaryWriter b, Dictionary<string, List<ObjFace>> objects)
        {
            foreach (var pair in objects)
            {
                b.WriteNtString(pair.Key);
                b.Write(pair.Value.Count); // number of faces in object
                foreach (var face in pair.Value)
                {
                    if (face.Vertices.Count > 4 || face.Vertices.Count < 3)
                        throw new InvalidDataException("Expected triangles and quads only");

                    b.Write(GetMaterial(face.MaterialName));
                    Console.WriteLine($"{pair.Key} -> {face.MaterialName}");
                    b.Write(face.Vertices.Count); // number of verts in face
                    foreach (var vertex in face.Vertices)
                    {
                        // OBJ model pointers are 1-indexed
                        b.Write7BitEncodedInt(vertex.Vertex - 1);
                        b.Write7BitEncodedInt(vertex.Normal - 1);
                        b.Write7BitEncodedInt(vertex.Texture - 1);
                    }
                }
            }
        }

        private static byte GetMaterial(string materialName)
        {
            switch (materialName)
            {
                case "MAT_DIFFUSE_OPAQUE":
                    return (byte) FaceMaterial.DiffuseOpaque;
                case "MAT_DIFFUSE_CUTOUT":
                    return (byte) FaceMaterial.DiffuseCutout;
                case "MAT_DIFFUSE_TRANSLUCENT":
                    return (byte) FaceMaterial.DiffuseTranslucent;
                case "MAT_EMISSIVE":
                    return (byte) FaceMaterial.Emissive;
                default:
                    throw new InvalidDataException("Expected material name to be one of: MAT_DIFFUSE_OPAQUE, MAT_DIFFUSE_CUTOUT, MAT_DIFFUSE_TRANSLUCENT, MAT_EMISSIVE");
            }
        }
    }
}
