using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace JarPacker
{
    class Program
    {
        static void Main(string[] args)
        {
            string recafJarPath = Path.Combine(Directory.GetCurrentDirectory(), "recaf-edit.jar");
            string destJarPath = Path.Combine(Directory.GetCurrentDirectory(), "spacehaven.jar");
            string destZipPath = Path.Combine(Directory.GetCurrentDirectory(), "patch.zip");

            Console.WriteLine("Config : JarPacker.config");
            Console.WriteLine("Recaf JAR : recaf-edit.jar");
            Console.WriteLine("Destination JAR : spacehaven.jar");
            Console.WriteLine("---------------------------------------------------------");


            string[] classPaths = File.ReadAllLines(Path.Combine(Directory.GetCurrentDirectory(), "JarPacker.config"));


            using (ZipArchive recafJar = ZipFile.Open(recafJarPath, ZipArchiveMode.Read))
            {
                List<ZipArchiveEntry> classEntries = new List<ZipArchiveEntry>();
                foreach (string classPath in classPaths)
                {
                    ZipArchiveEntry classEntry = recafJar.GetEntry(classPath.Trim());
                    if (classEntry == null)
                    {
                        Console.WriteLine($"Can't find {classPath} in {recafJarPath}");
                        Console.ReadLine();
                        return;
                    }
                    classEntries.Add(classEntry);
                }

                Console.WriteLine("What do you want to do ?");
                Console.WriteLine("[1] Update spacehaven.jar");
                Console.WriteLine("[2] Create patch zip");
                switch (Console.ReadKey(true).KeyChar)
                {
                    case '1':
                        using (ZipArchive destJar = ZipFile.Open(destJarPath, ZipArchiveMode.Update))
                        {
                            foreach (ZipArchiveEntry classEntry in classEntries)
                            {
                                destJar.GetEntry(classEntry.FullName).Delete();
                                ZipArchiveEntry updatedEntry = destJar.CreateEntry(classEntry.FullName);
                                classEntry.Open().CopyTo(updatedEntry.Open());
                                Console.WriteLine($"Updated {classEntry.FullName}");
                            }
                        }
                        Console.WriteLine($"Updated all classes in {destJarPath}");
                        break;
                    case '2':
                        using (FileStream fileStream = new FileStream(destZipPath, FileMode.Create))
                        {
                            using (ZipArchive destZip = new ZipArchive(fileStream, ZipArchiveMode.Update))
                            {
                                foreach (ZipArchiveEntry classEntry in classEntries)
                                {
                                    ZipArchiveEntry newEntry = destZip.CreateEntry(classEntry.FullName);
                                    classEntry.Open().CopyTo(newEntry.Open());
                                    Console.WriteLine($"Created {classEntry.FullName}");

                                }
                            }
                        }
                        Console.WriteLine($"Zip patch written : {destZipPath}");

                        break;
                }
            }

            Console.ReadLine();
        }
    }
}
