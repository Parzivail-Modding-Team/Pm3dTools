using System.IO;
using System.Text;

namespace Pm3dTools
{
	static class Extensions
	{
		/// <summary>
		/// Writes a null-terminated string to the stream
		/// </summary>
		/// <param name="stream">The stream to write to</param>
		/// <param name="s">The string to write</param>
		public static void WriteNtString(this BinaryWriter stream, string s)
		{
			var bytes = Encoding.UTF8.GetBytes(s);
			stream.Write(bytes);
			stream.Write((byte) 0);
		}

		public static string ReadNtString(this BinaryReader stream)
		{
			var sb = new StringBuilder();

			byte b;
			while ((b = stream.ReadByte()) != 0)
				sb.Append((char) b);

			return sb.ToString();
		}
	}
}