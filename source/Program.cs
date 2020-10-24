using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace LargeShipPatcher
{
    class Program
    {
        static List<GamePatch> knownVersions = new List<GamePatch>()
        {
            new GamePatch("alpha 10", new Version(0, 10, 0), "build 2"),
            new GamePatch("alpha 10", new Version(0, 10, 0), "build 4")
        };

        static void Main(string[] args)
        {
            string jarPath;
            string rootPath;
            string backupName;
            string backupPath;
            bool doBackup = false;


            Console.WriteLine($"           LargeShipPatcher {Assembly.GetExecutingAssembly().GetName().Version} for Space Haven");
            Console.WriteLine($"--------------------------------------------------------------------------");
            Console.WriteLine($"               IMPORTANT !");
            Console.WriteLine($"Execute this patch as admin / sudo from the same folder as spacehaven.jar");
            Console.WriteLine($"--------------------------------------------------------------------------");
            Console.WriteLine($"                FEATURES");
            Console.WriteLine($"- Allow ships of size 3x2, 2x3 and 3x3 to be build");
            Console.WriteLine($"- Increase the available ship points from 8 to 14");
            Console.WriteLine($"- Increase sector size from 8x8 to 10x10");
            Console.WriteLine($"- Allow changing the amount of system points per ship point");
            Console.WriteLine($"- Features can be tweaked by editing the \"LargeShipPatcher.xml\" file");
            Console.WriteLine($"--------------------------------------------------------------------------");



            if (File.Exists("spacehaven.jar"))
            {
                rootPath = Directory.GetCurrentDirectory();
                jarPath = Path.Combine(rootPath, "spacehaven.jar");
            }
            else
            {
                while (true)
                {
                    Console.WriteLine("Please input the path to \"spacehaven.jar\" :");
                    rootPath = Console.ReadLine();

                    if (!Directory.Exists(rootPath))
                    {
                        rootPath = Path.GetDirectoryName(rootPath);
                    }

                    jarPath = Path.Combine(rootPath, "spacehaven.jar");

                    if (File.Exists(jarPath))
                        break;

                    Console.WriteLine($"Could not find \"{jarPath}\"...");
                }
            }

            try
            {
                using (File.OpenWrite(jarPath)) { }
                File.WriteAllText(Path.Combine(rootPath, "LargeShipPatcher_ReadMe.txt"), Configs.ResourceManager.GetString("ReadMe", CultureInfo.InvariantCulture));
            }
            catch (Exception)
            {
                Console.WriteLine($"Error : write permission denied, make sure the patcher has full rights to write in this folder");
                Console.ReadLine();
                return;
            }

            string tempFilePath = Path.Combine(Path.GetDirectoryName(jarPath), "spacehaven.jar.temp");
            File.Copy(jarPath, tempFilePath, true);

            int entriesCount = 0;
            bool error = false;
            string versionString;
            string versionNameString;
            string buildString = string.Empty;
            GamePatch matchingPatch = null;
            using (ZipArchive jarFile = ZipFile.Open(tempFilePath, ZipArchiveMode.Update))
            {
                ZipArchiveEntry versionFile = jarFile.GetEntry("version.txt");
                
                Version version;
                using (TextReader versionReader = new StreamReader(versionFile.Open()))
                {
                    try
                    {
                        versionString = versionReader.ReadLine();
                        if (versionString == null) throw new Exception();
                    }
                    catch (Exception)
                    {
                        versionString = string.Empty;
                    }

                    try
                    {
                        versionNameString = versionReader.ReadLine();
                        if (versionNameString == null) throw new Exception();
                    }
                    catch (Exception)
                    {
                        versionNameString = string.Empty;
                    }

                    versionString = versionString.Trim();
                    versionNameString = versionNameString.Trim();

                    try
                    {
                        version = new Version(versionString);
                    }
                    catch (Exception)
                    {
                        version = null;
                        Console.WriteLine($"Can't parse \"{versionString}\", game version unknown !");
                    }
                }
                     
                ZipArchiveEntry mainClass = jarFile.GetEntry("fi/bugbyte/spacehaven/MainClass.class");
                
                using (TextReader mainClassReader = new StreamReader(mainClass.Open(), System.Text.Encoding.ASCII))
                {
                    string classText = mainClassReader.ReadToEnd();
                    foreach (GamePatch knownVersion in knownVersions.FindAll(p => p.version == version))
                    {
                        // note : this can fail, for example if there is a build 1 and the game is build 11, it will match the build 1
                        if (classText.Contains(knownVersion.build, StringComparison.OrdinalIgnoreCase))
                        {
                            buildString = knownVersion.build;
                            matchingPatch = knownVersion;
                            break;
                        }
                    }
                }

                List<GamePatch> matchingPatches = new List<GamePatch>();
                if (version != null)
                {
                    matchingPatches.AddRange(knownVersions.FindAll(p => p.version == version));
                }
                else
                {
                    version = new Version(int.MaxValue, int.MaxValue);
                }

                if (matchingPatches.Count == 0)
                {
                    matchingPatches = knownVersions.FindAll(p => p.version < version).OrderByDescending(i => i.version).Take(2).ToList();
                    matchingPatches.AddRange(knownVersions.FindAll(p => p.version > version).OrderBy(i => i.version).Take(2));
                }

                Console.WriteLine($"Found Space Haven {(string.IsNullOrEmpty(versionNameString) ? "RELEASE UNKNOWN" : versionNameString)}, version {versionString} - {(string.IsNullOrEmpty(buildString) ? "BUILD UNKNOWN" : buildString)}");
                Console.WriteLine($"--------------------------------------------------------------------------");
                if (matchingPatch == null)
                {
                    startPatchSelect:
                    Console.WriteLine($"Unable to find a matching patch for this version");
                    Console.WriteLine($"You can use another available patch but this can cause issues and crashes.");
                    for (int i = 0; i < matchingPatches.Count; i++)
                    {
                        Console.WriteLine($"[{i}] {matchingPatches[i]}");
                    }
                    Console.WriteLine($"Input a number for the patch you want to use and press enter");
                    if (!int.TryParse(Console.ReadLine(), out int index) || index < 0 || index > matchingPatches.Count - 1)
                    {
                        Console.WriteLine($"--------------------------------------------------------------------------");
                        Console.WriteLine("Error : bad input");
                        Console.WriteLine($"--------------------------------------------------------------------------");
                        goto startPatchSelect;
                    }

                    matchingPatch = matchingPatches[index];
                    Console.WriteLine("");
                    Console.WriteLine($"--------------------------------------------------------------------------");
                }

                backupName = "spacehaven.jar_" + versionString + "_" + buildString.Replace(" ", "") + "_" + DateTime.Now.ToString("yyMMddHHmm");
                backupPath = Path.Combine(Path.GetDirectoryName(jarPath), backupName);

                Console.WriteLine("Ready to patch - Do you want to keep a backup ? (y/n)");
                if (Console.ReadKey(true).KeyChar == 'y')
                    doBackup = true;

                using (ZipArchive patchFile = new ZipArchive(new MemoryStream(matchingPatch.Patch)))
                {
                    foreach (ZipArchiveEntry patchedEntry in patchFile.Entries)
                    {
                        if (!patchedEntry.FullName.EndsWith(".class"))
                            continue;

                        try
                        {
                            jarFile.GetEntry(patchedEntry.FullName).Delete();
                            ZipArchiveEntry replacedEntry = jarFile.CreateEntry(patchedEntry.FullName, CompressionLevel.Fastest);
                            patchedEntry.Open().CopyTo(replacedEntry.Open());
                            Console.WriteLine($"Patching {patchedEntry.FullName}");
                            entriesCount++;
                        }
                        catch (Exception)
                        {
                            Console.WriteLine($"Error while patching class {patchedEntry.FullName}...");
                            error = true;
                        }
                    }
                }
            } // note : file is written to here, when the ZipArchive object is being disposed of.

            Console.WriteLine($"--------------------------------------------------------------------------");

            if (!error)
            {
                File.WriteAllText(Path.Combine(rootPath, "LargeShipPatcher.xml"), Configs.ResourceManager.GetString("LargeShipPatcher", CultureInfo.InvariantCulture));
                Console.WriteLine($"Done : {entriesCount} classes patched");
                if (doBackup)
                {
                    File.Copy(jarPath, backupPath, true);
                    Console.WriteLine($"Backup created : \"{backupName}\"");
                }
                File.Move(tempFilePath, jarPath, true);

                Console.WriteLine($"File written to {jarPath}");
            }
            else
            {
                Console.WriteLine($"Error while patching - no change was done");
                File.Delete(tempFilePath);
            }
            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }




        private class GamePatch
        {
            public string versionName;
            public Version version;
            public string build;

            public GamePatch(string versionName, Version version, string build)
            {
                this.versionName = versionName;
                this.version = version;
                this.build = build;
            }

            public override string ToString()
            {
                return versionName + " - " + version.ToString(3) + " - " + build;
            }

            public byte[] Patch
            {
                get
                {
                    string patchName = version.ToString(3) + "_" + build;
                    return (byte[])Patchs.ResourceManager.GetObject(patchName);
                }
            }

        }


    }
}
