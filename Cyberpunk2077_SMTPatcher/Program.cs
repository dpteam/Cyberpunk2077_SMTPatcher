using System;
using System.IO;
using System.Reflection;

namespace Cyberpunk2077_SMTPatcher
{
	public static class Program
	{
		static void Main(string[] args)
		{
			string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string cp77exe = "Cyberpunk2077.exe";
			if (File.Exists(cp77exe + ".smt-patcher-bak"))
			{
				Console.WriteLine("A backup was found. Already patched?");
				Console.ReadKey();
			}
			File.Copy(assemblyPath + Path.DirectorySeparatorChar + cp77exe, assemblyPath + Path.DirectorySeparatorChar + cp77exe + ".smt-patcher-bak", false);
			Console.WriteLine("Backup created");
			byte[] sourceBytes = StringHexToByteArray("753033C9B8010000000FA28BC8C1F908");
			byte[] targetBytes = StringHexToByteArray("EB3033C9B8010000000FA28BC8C1F908");
			BinaryReplace(cp77exe + ".smt-patcher-bak", sourceBytes, cp77exe, targetBytes);
			Console.WriteLine("SMT Pattern found and replaced. Cyberpunk 2077 patched successfuly.");
			Console.WriteLine("Press any key to exit.");
			Console.ReadKey();
		}

		public static void BinaryReplace(string sourceFile, byte[] sourceSeq, string targetFile, byte[] targetSeq)
		{
			FileStream sourceStream = File.OpenRead(sourceFile);
			FileStream targetStream = File.Create(targetFile);

			try
			{
				int b;
				long foundSeqOffset = -1;
				int searchByteCursor = 0;

				while ((b = sourceStream.ReadByte()) != -1)
				{
					if (sourceSeq[searchByteCursor] == b)
					{
						if (searchByteCursor == sourceSeq.Length - 1)
						{
							targetStream.Write(targetSeq, 0, targetSeq.Length);
							searchByteCursor = 0;
							foundSeqOffset = -1;
						}
						else
						{
							if (searchByteCursor == 0)
							{
								foundSeqOffset = sourceStream.Position - 1;
							}

							++searchByteCursor;
						}
					}
					else
					{
						if (searchByteCursor == 0)
						{
							targetStream.WriteByte((byte)b);
						}
						else
						{
							targetStream.WriteByte(sourceSeq[0]);
							sourceStream.Position = foundSeqOffset + 1;
							searchByteCursor = 0;
							foundSeqOffset = -1;
						}
					}
				}
			}
			finally
			{
				sourceStream.Dispose();
				targetStream.Dispose();
			}
		}

		public static byte[] StringHexToByteArray(String hex)
		{
			int NumberChars = hex.Length;
			byte[] bytes = new byte[NumberChars / 2];
			for (int i = 0; i < NumberChars; i += 2)
			bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
			return bytes;
		}
	}
}
