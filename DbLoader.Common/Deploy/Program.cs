using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using NuGet;

namespace Deploy {
    internal class Program {
        private static void Main(string[] args) {
            var dir = new DirectoryInfo(".\\bin\\Debug\\net461");
            var nuspecSourceFile = "Sprockets.DistributedIndexing.xml";
            var pb = new PackageBuilder(File.OpenRead(nuspecSourceFile), dir.FullName);


            Console.WriteLine("ID:{0} ", pb.Id);
            Console.WriteLine("Version:{0} ", pb.Version);
            Console.WriteLine("Building from {0}", dir.FullName);
            pb.Files.Clear();
            foreach (var dll in dir.GetFiles("sprockets*.dll", SearchOption.AllDirectories))
                pb.Files.Add(new PhysicalPackageFile {
                    SourcePath = dll.FullName,
                    TargetPath = "\\lib\\" + dll.Name
                });

            var dropDir = Environment.ExpandEnvironmentVariables(
                $"C:\\DevProjects\\NuGetPackages\\{pb.Id}\\{pb.Version}\\");
            Directory.CreateDirectory(dropDir);

            var packageName = Path.Combine(dropDir, $"{pb.Id}.{pb.Version}.nupkg");

            using (var drop = File.Open(packageName, FileMode.OpenOrCreate, FileAccess.ReadWrite)) {
                drop.SetLength(0);
                pb.Save(drop);
            }
            File.Copy(nuspecSourceFile, Path.Combine(dropDir, $"{pb.Id}.nuspec"), true);
            using (var srcForHash = File.Open(packageName, FileMode.Open, FileAccess.Read))
            using (var sha = new SHA512Managed()) {
                var hash = Convert.ToBase64String(sha.ComputeHash(srcForHash));
                using (var dstForHash =
                    File.Open(packageName + ".sha512", FileMode.OpenOrCreate, FileAccess.ReadWrite)) {
                    dstForHash.SetLength(0);
                    var hashByteString = Encoding.ASCII.GetBytes(hash);
                    dstForHash.Write(hashByteString, 0, hashByteString.Length);
                }
            }
        }
    }
}