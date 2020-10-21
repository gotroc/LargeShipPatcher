using System;
using System.IO;
using System.IO.Compression;

namespace LargeShipPatcher
{
    class Program
    {
        static string latestPatch = "0.10.0";

        static void Main(string[] args)
        {
            string jarPath;

            Console.WriteLine($"         LargeShipPatcher for Space Haven " + latestPatch);
            Console.WriteLine($"------------------------------------------------------------------");
            Console.WriteLine($"This patch allow ships of size 3x2, 2x3 and 3x3 to be build");
            Console.WriteLine($"It also increase the available ship points from 8 to 16");
            Console.WriteLine($"Execute it as admin / sudo from the same folder as spacehaven.jar");
            Console.WriteLine($"------------------------------------------------------------------");

            if (File.Exists("spacehaven.jar"))
            {
                jarPath = Path.Combine(Directory.GetCurrentDirectory(), "spacehaven.jar");
                Console.WriteLine($"Patching {jarPath}...");
            }
            else
            {
                while (true)
                {
                    Console.WriteLine("Please input the path to \"spacehaven.jar\" :");
                    jarPath = Console.ReadLine();

                    if (!Directory.Exists(jarPath))
                    {
                        jarPath = Path.GetDirectoryName(jarPath);
                    }

                    jarPath = Path.Combine(jarPath, "spacehaven.jar");

                    if (File.Exists(jarPath))
                        break;

                    Console.WriteLine($"Could not find \"{jarPath}\"...");
                }
            }

            try
            {
                using (File.OpenWrite(jarPath)) { }
            }
            catch (Exception)
            {
                Console.WriteLine($"Error : write permission denied, is the file open ?");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("-----------------------------------------------------------");
            Console.WriteLine("Do you want to make a backup ? (y/n)");

            if (Console.ReadKey(true).KeyChar == 'y')
            {
                string destPath = Path.Combine(Path.GetDirectoryName(jarPath), "spacehaven.jar.backup");
                File.Copy(jarPath, destPath, true);
                Console.WriteLine($"Backup created : \"{destPath}\"");
            }
            Console.WriteLine($"-----------------------------------------------------------");

            using (ZipArchive jarFile = ZipFile.Open(jarPath, ZipArchiveMode.Update))
            {
                ZipArchiveEntry versionFile = jarFile.GetEntry("version.txt");
                TextReader tr = new StreamReader(versionFile.Open());
                string version = tr.ReadLine().Trim();

                Console.WriteLine($"Found Space Haven version {version}");

                object patch = Patchs.ResourceManager.GetObject(version);
                if (patch == null)
                {
                    Console.WriteLine($"No patch available for this version...");
                    Console.WriteLine($"You can use the latest available patch but this may cause crashes.");
                    Console.WriteLine($"Do you wish to apply the {latestPatch} patch ? (y/n) ?");
                    if (Console.ReadKey(true).KeyChar != 'y')
                    {
                        return;
                    }
                    version = latestPatch;
                    patch = Patchs.ResourceManager.GetObject(version);
                }

                Console.WriteLine("-----------------------------------------------------------");
                Console.WriteLine("Patching...");

                using (ZipArchive patchFile = new ZipArchive(new MemoryStream((byte[])patch)))
                {
                    foreach (ZipArchiveEntry patchedEntry in patchFile.Entries)
                    {
                        try
                        {
                            jarFile.GetEntry(patchedEntry.FullName).Delete();
                            ZipArchiveEntry replacedEntry = jarFile.CreateEntry(patchedEntry.FullName, CompressionLevel.Fastest);
                            patchedEntry.Open().CopyTo(replacedEntry.Open());
                        }
                        catch (Exception)
                        {
                            Console.WriteLine($"Error while patching class {patchedEntry.FullName}...");
                        }
                    }
                }

                Console.WriteLine("-----------------------------------------------------------");
                Console.WriteLine("Done !");
                Console.WriteLine("Press any key to exit");
                Console.ReadLine();
            }
        }
    }
}
