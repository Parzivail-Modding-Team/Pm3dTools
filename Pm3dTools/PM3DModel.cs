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
	    private const int FileVersion = 0x05;
        private static readonly byte[] HeaderMagic = { (byte)'P', (byte)'m', (byte)'3', (byte)'D' };

        public IList<Pm3DLod> Lods { get; }

		private Pm3DModel(IList<Pm3DLod> lods)
        {
            Lods = lods;
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
					.ToDictionary(grouping => grouping.Key ?? "", grouping => grouping.ToList());

				lods.Add(new Pm3DLod(objects, obj.Vertices, obj.VertexNormals, obj.TextureVertices));
			}

            return new Pm3DModel(lods);
        }

        public static Pm3DModel FromFile(string filename)
        {
	        using var b = new BinaryReader(File.Open(filename, FileMode.Open));

	        var header = b.ReadBytes(HeaderMagic.Length);

	        if (!header.SequenceEqual(HeaderMagic))
		        throw new InvalidDataException("Not a Pm3D file");

	        var version = b.ReadInt32();
	        if (version != FileVersion)
		        throw new InvalidDataException($"Not a Pm3Dv{FileVersion} file");

	        var lodCount = b.ReadInt32();
	        var lods = new List<Pm3DLod>();
	        
	        for (var i = 0; i < lodCount; i++)
	        {
				var numVerts = b.ReadInt32();
				var numNorms = b.ReadInt32();
				var numUvs = b.ReadInt32();
				var numObjects = b.ReadInt32();

				var verts = ReadVerts(b, numVerts);
				var norms = ReadNormals(b, numNorms);
				var uvs = ReadUvs(b, numUvs);
				var objects = ReadObjects(b, numObjects);
				
				lods.Add(new Pm3DLod(objects, verts, norms, uvs));
	        }

	        return new Pm3DModel(lods);
        }

        private static Dictionary<string, List<ObjFace>> ReadObjects(BinaryReader b, int num)
        {
	        var d = new Dictionary<string, List<ObjFace>>();
	        
	        for (var i = 0; i < num; i++)
	        {
		        var faces = new List<ObjFace>();
		        var objectName = b.ReadNtString();
		        var numFaces = b.ReadInt32();

		        for (var j = 0; j < numFaces; j++)
		        {
			        var face = new ObjFace();
			        var material = (FaceMaterial) b.ReadByte();
			        var numVerts = b.ReadInt32();

			        face.MaterialName = GetMaterialName(material);

			        for (var k = 0; k < numVerts; k++)
			        {
				        var v = b.Read7BitEncodedInt();
				        var n = b.Read7BitEncodedInt();
				        var t = b.Read7BitEncodedInt();
				        
				        face.Vertices.Add(new ObjTriplet(v + 1, t + 1, n + 1));
			        }
			        
			        faces.Add(face);
		        }

		        d[objectName] = faces;
	        }

	        return d;
        }

        private static List<ObjVertex> ReadVerts(BinaryReader b, int num)
        {
	        var l = new List<ObjVertex>();

	        for (var i = 0; i < num; i++) 
		        l.Add(new ObjVertex(b.ReadSingle(), b.ReadSingle(), b.ReadSingle()));

	        return l;
        }

        private static List<ObjVector3> ReadNormals(BinaryReader b, int num)
        {
	        var l = new List<ObjVector3>();

	        for (var i = 0; i < num; i++) 
		        l.Add(new ObjVector3(b.ReadSingle(), b.ReadSingle(), b.ReadSingle()));

	        return l;
        }

        private static List<ObjVector3> ReadUvs(BinaryReader b, int num)
        {
	        var l = new List<ObjVector3>();

	        for (var i = 0; i < num; i++) 
		        l.Add(new ObjVector3(b.ReadSingle(), b.ReadSingle(), 0));

	        return l;
        }

        public void Write(string filename, float scaleFactor)
        {
	        using var b = new BinaryWriter(File.Open(filename, FileMode.Create));

	        b.Write(HeaderMagic);
	        b.Write(FileVersion);

			b.Write(Lods.Count);

			foreach (var lod in Lods)
			{
				b.Write(lod.Verts.Count);
				b.Write(lod.Normals.Count);
				b.Write(lod.Uvs.Count);
				b.Write(lod.Objects.Count);

				foreach (var vert in lod.Verts) WriteVert(b, vert, scaleFactor);
				foreach (var norm in lod.Normals) WriteNormal(b, norm);
				foreach (var uv in lod.Uvs) WriteUv(b, uv);

				WriteObjects(b, lod.Objects);
			}
        }

        private static void WriteVert(BinaryWriter b, ObjVertex vert, float scaleFactor)
        {
	        b.Write(vert.Position.X * scaleFactor);
            b.Write(vert.Position.Y * scaleFactor);
            b.Write(vert.Position.Z * scaleFactor);
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
            foreach (var (objectName, faces) in objects)
            {
                b.WriteNtString(objectName);
                b.Write(faces.Count); // number of faces in object
                foreach (var face in faces)
                {
                    if (face.Vertices.Count is > 4 or < 3)
                        throw new InvalidDataException("Expected triangles and quads only");

                    b.Write(GetMaterial(face.MaterialName));
                    Console.WriteLine($"{objectName} -> {face.MaterialName}");
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
	        return materialName switch
	        {
		        "MAT_DIFFUSE_OPAQUE" => (byte) FaceMaterial.DiffuseOpaque,
		        "MAT_DIFFUSE_CUTOUT" => (byte) FaceMaterial.DiffuseCutout,
		        "MAT_DIFFUSE_TRANSLUCENT" => (byte) FaceMaterial.DiffuseTranslucent,
		        "MAT_EMISSIVE" => (byte) FaceMaterial.Emissive,
		        _ => throw new InvalidDataException("Expected material name to be one of: MAT_DIFFUSE_OPAQUE, MAT_DIFFUSE_CUTOUT, MAT_DIFFUSE_TRANSLUCENT, MAT_EMISSIVE")
	        };
        }

        private static string GetMaterialName(FaceMaterial material)
        {
	        return material switch
	        {
		        FaceMaterial.DiffuseOpaque => "MAT_DIFFUSE_OPAQUE",
		        FaceMaterial.DiffuseCutout => "MAT_DIFFUSE_CUTOUT",
		        FaceMaterial.DiffuseTranslucent => "MAT_DIFFUSE_TRANSLUCENT",
		        FaceMaterial.Emissive => "MAT_EMISSIVE",
				_ => throw new ArgumentException()
	        };
        }
    }
}
