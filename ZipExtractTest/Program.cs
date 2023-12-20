using System;
using System.IO;
using System.IO.Compression;

namespace ZipExtractTest {
	internal class Program {
		static void Main(string[] args) {

			var dest = Directory.GetCurrentDirectory();
			dest = Path.Combine(dest, "output");

			zip("Winrar zip", "Winrar.zip",  dest);
			zip("VisualStudio zip", "VS_MSBS_task.zip",  dest); //exception
			

			Console.WriteLine("Done");
			Console.ReadKey();
		}

		private static void zip(string name, string zipFile, string dest) {
			Console.WriteLine($"-----------------{name}--------------------------------------------------------");
			if(Directory.Exists(dest))
				Directory.Delete(dest, true);
			Directory.CreateDirectory(dest);
			try {
				extractArchive(zipFile, dest);
			}catch (Exception ex) {
				Console.WriteLine(ex);
			}
			Console.WriteLine($"-----------------FINISHED------------------------------------------------------");
		}

		public static void extractArchive(string zipFile, string destination) {
			destination = Path.GetFullPath(destination);
			if (!destination.EndsWith(Path.DirectorySeparatorChar.ToString()))
				destination += Path.DirectorySeparatorChar;

			using (var arc = ZipFile.OpenRead(zipFile)) {
				foreach (var e in arc.Entries) {
					var des = Path.GetFullPath(
						Path.Combine(destination + e.FullName));
					if (des.EndsWith(Path.DirectorySeparatorChar.ToString())) {
						if (!Directory.Exists(des)) Directory.CreateDirectory(des);
						continue;
					}
					if (File.Exists(des) || !des.StartsWith(destination)) continue;
					var dir = Path.GetDirectoryName(des);
					Directory.CreateDirectory(dir);
					Console.WriteLine($@"extracting  ""{e}"" to ""{des}""");
					e.ExtractToFile(des);
				}
			}
		}
	}
}
