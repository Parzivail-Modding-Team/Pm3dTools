using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using BlockbenchToPmodel.Blockbench;
using Brotli;
using JeremyAnsel.Media.WavefrontObj;

namespace BlockbenchToPmodel.PM3D
{
    class Model
    {
        private const int FileVersion = 0x01;
        private readonly byte[] _headerMagic = { (byte)'P', (byte)'m', (byte)'3', (byte)'D' };

        private readonly BlockbenchModel _json;
        private readonly Dictionary<ModelObjectInfo, List<ObjFace>> _objects;
        private readonly IList<ObjVertex> _verts;
        private readonly IList<ObjVector3> _normals;
        private readonly IList<ObjVector3> _uvs;

        private Model(BlockbenchModel json, Dictionary<ModelObjectInfo, List<ObjFace>> objects, IList<ObjVertex> verts, IList<ObjVector3> normals, IList<ObjVector3> uvs)
        {
            _json = json;
            _objects = objects;
            _verts = verts;
            _normals = normals;
            _uvs = uvs;
        }

        public static Model FromFile(string filename)
        {
            ExpectFile($"{filename}.json");
            ExpectFile($"{filename}.obj");

            var json = BlockbenchModel.FromFile($"{filename}.json");
            var obj = ObjFile.FromFile($"{filename}.obj");

            var objects = obj
                .Faces
                .GroupBy(face => new ModelObjectInfo(face.ObjectName, face.MaterialName))
                .ToDictionary(grouping => grouping.Key, grouping => grouping.ToList());

            var verts = obj.Vertices;
            var normals = obj.VertexNormals;
            var uvs = obj.TextureVertices;

            return new Model(json, objects, verts, normals, uvs);
        }

        private static void ExpectFile(string filename)
        {
            if (File.Exists(filename))
                return;

            Console.WriteLine($"File not found: {filename}");
            Environment.Exit(0);
        }

        public void Write(string filename)
        {
            using (var fs = File.OpenWrite(filename))
            using (var bs = new BrotliStream(fs, CompressionMode.Compress))
            using (var b = new BinaryWriter(bs))
            {
                b.Write(_headerMagic);
                b.Write(FileVersion);

                b.WriteNtString(_json.Credit);

                var flags = ModelFlags.None;

                if (_json.AmbientOcclusion)
                    flags |= ModelFlags.AmbientOcclusion;

                b.Write((byte)flags);
                
                b.Write(_json.Textures.Count);
                b.Write(_verts.Count);
                b.Write(_normals.Count);
                b.Write(_uvs.Count);
                b.Write(_objects.Count);
                
                WriteTextures(b, _json.Textures);

                foreach (var vert in _verts) WriteVert(b, vert);
                foreach (var norm in _normals) WriteNormal(b, norm);
                foreach (var uv in _uvs) WriteUv(b, uv);

                WriteObjects(b, _objects);
            }
        }

        private static void WriteTextures(BinaryWriter b, Dictionary<string, string> textures)
        {
            foreach (var pair in textures)
            {
                b.WriteNtString(pair.Key);
                b.WriteNtString(pair.Value);
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

        private static void WriteObjects(BinaryWriter b, Dictionary<ModelObjectInfo, List<ObjFace>> objects)
        {
            foreach (var pair in objects)
            {
                b.WriteNtString(pair.Key.ObjectName);
                b.WriteNtString(pair.Key.MaterialName);
                b.Write(pair.Value.Count); // number of faces in object
                foreach (var face in pair.Value)
                {
                    b.Write(face.Vertices.Count); // number of verts in face
                    foreach (var vertex in face.Vertices)
                    {
                        b.Write(vertex.Vertex);
                        b.Write(vertex.Normal);
                        b.Write(vertex.Texture);
                    }
                }
            }
        }
    }
}
