using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using PhilLibX;

namespace CoDDataDumper
{
    class WorldWarII
    {
        /// <summary>
        /// Image Semantics
        /// </summary>
        public static Dictionary<uint, string> MaterialImageSemantics = new Dictionary<uint, string>()
        {
            { 0x10B47580, "Dynamic Grit Dirt" },
            { 0xA01A287F, "Dynamic Grit Fire" },
            { 0x34ECCCB3, "SpecMask/Gloss/Occlusion" },
            { 0x59D30D0F, "Normal Map" },
            { 0x6001F931, "Occlusion" },
            { 0xA0AB1041, "Color Map" },
            { 0xBAE31156, "Camo Mask" },
        };

        /// <summary>
        /// Sound Alias Struct
        /// </summary>
        unsafe struct SoundAlias
        {
            /// <summary>
            /// Pointer to Alias Name string
            /// </summary>
            public long NamePointer { get; set; }

            /// <summary>
            /// Pointer to Alias Data
            /// </summary>
            public long DataPointer { get; set; }

            /// <summary>
            /// Unknown Pointer (is 0 unless the byte after Entry Count is > 0 i.e. has data)
            /// </summary>
            public long UnknownPointer { get; set; }

            /// <summary>
            /// Number of Entries for this Alias
            /// </summary>
            public byte EntryCount { get; set; }

            /// <summary>
            /// Unknown Bytes (first byte may be a data count for the Pointer above)
            /// </summary>
            public fixed byte UnknownBytes[7];
        }

        /// <summary>
        /// Sound Alias Entry
        /// </summary>
        unsafe struct SoundAliasEntry
        {
            /// <summary>
            /// A Pointer to the Alias Name
            /// </summary>
            public long NamePointer { get; set; }

            /// <summary>
            /// Unknown Pointer (Sometimes pointers to another alias?)
            /// </summary>
            public long UnknownPointer { get; set; }

            /// <summary>
            /// Unknown Pointer (Sometimes pointers to another alias?)
            /// </summary>
            public long UnknownPointer2 { get; set; }

            /// <summary>
            /// Secondary Alias
            /// </summary>
            public long SecondaryPointer { get; set; }

            /// <summary>
            /// Unknown Pointer (Always null?)
            /// </summary>
            public long UnknownPointer3 { get; set; }

            /// <summary>
            /// Sound File Data
            /// </summary>
            public long SoundFilePointer { get; set; }

            /// <summary>
            /// Resulting Bytes, alias settings and other asset pointers
            /// </summary>
            public fixed byte UnknownBytes[0x120];
        }

        /// <summary>
        /// Sound Alias File Spec info
        /// </summary>
        unsafe struct SoundAliasFileSpec
        {
            /// <summary>
            /// Sound Type (Streamed, Primed, Loaded)
            /// </summary>
            public byte Type { get; set; }

            /// <summary>
            /// Sound Exists Or Not
            /// </summary>
            public byte Exists { get; set; }

            /// <summary>
            /// Unknown Bytes
            /// </summary>
            public fixed byte Padding[6];
        }

        /// <summary>
        /// Material Asset Info
        /// </summary>
        unsafe struct Material
        {
            /// <summary>
            /// A pointer to the name of this material
            /// </summary>
            public long NamePointer { get; set; }

            /// <summary>
            /// Unknown Bytes (Flags, settings, etc.)
            /// </summary>
            public fixed byte UnknownBytes[0x9A];

            /// <summary>
            /// Number of Images this Material has
            /// </summary>
            public byte ImageCount { get; set; }

            /// <summary>
            /// Unknown Bytes (Flags, settings, etc.)
            /// </summary>
            public fixed byte UnknownBytes1[0x15];

            /// <summary>
            /// A pointer to the Tech Set this Material uses
            /// </summary>
            public long TechniqueSetPointer { get; set; }

            /// <summary>
            /// A pointer to this Material's Image table
            /// </summary>
            public long ImageTablePointer { get; set; }

            /// <summary>
            /// UnknownPointer (Probably settings that changed based off TechSet)
            /// </summary>
            public long UnknownPointer { get; set; }

            /// <summary>
            /// Null Bytes
            /// </summary>
            public long Padding { get; set; }

            /// <summary>
            /// Unknown Bytes (Flags, settings, etc.)
            /// </summary>
            public fixed byte UnknownBytes2[0x90];
        }

        /// <summary>
        /// Material Image
        /// </summary>
        unsafe struct MaterialImage
        {
            /// <summary>
            /// Semantic Hash/Usage
            /// </summary>
            public uint SemanticHash { get; set; }

            /// <summary>
            /// Unknown Int (It's possible the semantic hash is actually 64bit, and this is apart of the actual hash)
            /// </summary>
            public uint UnknownInt { get; set; }

            /// <summary>
            /// Pointer to the Image Asset
            /// </summary>
            public long ImagePointer { get; set; }
        }

        /// <summary>
        /// Game Offsets
        /// </summary>
        public static DBGameInfo[] GameOffsetsMP =
        {
            new DBGameInfo(0xC053701, 0xEACC40, 0),
        };

        /// <summary>
        /// Game Offsets
        /// </summary>
        public static DBGameInfo[] GameOffsetsSP =
        {
            new DBGameInfo(0xC05370, 0xEACC40, 0),
        };

        /// <summary>
        /// Exports Data from WW2
        /// </summary>
        public static bool Process(bool isMP = true)
        {
            // Get base address because ASLR
            long baseAddress = GameLoader.Reader.GetBaseAddress();

            // Loop, depending on SP and MP because 2 EXE's is cool 
            foreach (var gameOffset in isMP ? GameOffsetsMP : GameOffsetsSP )
            {
                // Get Data (Models pool is used for verification)
                long modelsPoolPointer   = GameLoader.Reader.ReadInt64(baseAddress + gameOffset.AssetPoolAddress + 0x8 * 0xA);
                long soundPoolPointer    = GameLoader.Reader.ReadInt64(baseAddress + gameOffset.AssetPoolAddress + 0x8 * 0x16);
                long materialPoolPointer = GameLoader.Reader.ReadInt64(baseAddress + gameOffset.AssetPoolAddress + 0x8 * 0xD);

                // Check first model name
                if(GameLoader.Reader.ReadNullTerminatedString(GameLoader.Reader.ReadInt64(modelsPoolPointer + 8)) == "empty_model")
                {
                    // Read Pool Sizes
                    int soundPoolSize = GameLoader.Reader.ReadInt32(baseAddress + gameOffset.AssetPoolSizesAddress + 0x4 * 0x16);
                    int materialPoolSize = GameLoader.Reader.ReadInt32(baseAddress + gameOffset.AssetPoolSizesAddress + 0x4 * 0xD);

                    // Export Supported Assets
                    ExportSoundPool(soundPoolPointer, soundPoolSize);
                    ExportMaterialPool(materialPoolPointer, materialPoolSize);

                    // We done it lads
                    return true;
                }
            }

            // Scan memory for matching instructions
            var dbAssetsScan = GameLoader.Reader.FindBytes(new byte?[] { 0x4A, 0x8B, 0xAC, null, null, null, null, null, 0x48, 0x85, 0xED }, baseAddress, baseAddress + GameLoader.Reader.GetModuleMemorySize(), true);
            var dbSizesScan  = GameLoader.Reader.FindBytes(new byte?[] { 0x83, 0xBC, null, null, null, null, null, 0x01, 0x7F, 0x48 }, baseAddress, baseAddress + GameLoader.Reader.GetModuleMemorySize(), true);

            // Check for Matches
            if(dbAssetsScan.Length > 0 && dbSizesScan.Length > 0)
            {
                // Set Data
                var gameOffset = new DBGameInfo(
                    GameLoader.Reader.ReadInt32(dbAssetsScan[0] + 4) + baseAddress,
                    GameLoader.Reader.ReadInt32(dbSizesScan[0] + 3) + baseAddress,
                    0);
                // Get Data (Models pool is used for verification)
                long modelsPoolPointer = GameLoader.Reader.ReadInt64(gameOffset.AssetPoolAddress + 0x8 * 0xA);
                long soundPoolPointer = GameLoader.Reader.ReadInt64(gameOffset.AssetPoolAddress + 0x8 * 0x16);
                long materialPoolPointer = GameLoader.Reader.ReadInt64(gameOffset.AssetPoolAddress + 0x8 * 0xD);
                // Check first model name
                if (GameLoader.Reader.ReadNullTerminatedString(GameLoader.Reader.ReadInt64(modelsPoolPointer + 8)) == "empty_model")
                {
                    // Read Pool Sizes
                    int soundPoolSize = GameLoader.Reader.ReadInt32(gameOffset.AssetPoolSizesAddress + 0x4 * 0x16);
                    int materialPoolSize = GameLoader.Reader.ReadInt32(gameOffset.AssetPoolSizesAddress + 0x4 * 0xD);
                    // Export Supported Assets
                    ExportSoundPool(soundPoolPointer, soundPoolSize);
                    ExportMaterialPool(materialPoolPointer, materialPoolSize);
                    // We done it lads
                    return true;
                }
            }

            // We Failed Lads
            return false;
        }

        /// <summary>
        /// Exports Sound Alias Data 
        /// </summary>
        static void ExportMaterialPool(long assetPoolPointer, int assetPoolSize)
        {
            // Info
            Printer.WriteLine("INFO", "Exporting basic material information....");
            // Create Folder
            Directory.CreateDirectory("WWII\\DBMaterials");
            // Addresses (+8 to skip free header)
            long address = assetPoolPointer + 8;
            long endAddress = address + 8 + assetPoolSize * Marshal.SizeOf<Material>();
            // Count Tracker
            int materialsProcessed = 0;
            // Loop and process the pool
            for (int i = 0; i < assetPoolSize; i++)
            {
                // Read Material
                var materialAsset = GameLoader.Reader.ReadStruct<Material>(address + (i * Marshal.SizeOf<Material>()));
                // Check is this asset entry empty
                if ((materialAsset.NamePointer > address && materialAsset.NamePointer < endAddress) || materialAsset.NamePointer == 0)
                    continue;
                // Get Name, purge prefix and invalid characters
                string materialName = GameLoader.Reader.ReadNullTerminatedString(materialAsset.NamePointer).Split('/').Last().Replace("*", "");
                // Create output
                using (StreamWriter writer = new StreamWriter(String.Format("WWII\\DBMaterials\\{0}.txt", materialName)))
                {
                    // Write Techset Name 
                    writer.WriteLine("Techinque Set - {0}", GameLoader.Reader.ReadNullTerminatedString(GameLoader.Reader.ReadInt64(materialAsset.TechniqueSetPointer)));
                    // Write Images
                    for(int j = 0; j < materialAsset.ImageCount; j++)
                    {
                        // Material Image Struct
                        var materialImage = GameLoader.Reader.ReadStruct<MaterialImage>(materialAsset.ImageTablePointer + (j * Marshal.SizeOf<MaterialImage>()));
                        // Write Data
                        writer.WriteLine("{0} - {1}", GetImageSemantic(materialImage.SemanticHash).PadRight(32), GameLoader.Reader.ReadNullTerminatedString(GameLoader.Reader.ReadInt64(materialImage.ImagePointer)));
                    }
                }
                // Increment 
                materialsProcessed++;
            }
            // Done
            Printer.WriteLine("INFO", String.Format("Exported {0} materials successfully", materialsProcessed));
        }

        /// <summary>
        /// Gets Image Semantic from List
        /// </summary>
        static string GetImageSemantic(uint hash)
        {
            return MaterialImageSemantics.TryGetValue(hash, out string result) ? result : "Unknown Semantic: " + hash.ToString("X");
        }

        /// <summary>
        /// Exports Sound Alias Data 
        /// </summary>
        static void ExportSoundPool(long assetPoolPointer, int assetPoolSize)
        {
            // Info
            Printer.WriteLine("INFO", "Exporting Sound Aliases.....");

            // Create Folder
            Directory.CreateDirectory("WWII\\DBSound");

            // Addresses (+8 to skip free header)
            long address = assetPoolPointer + 8;
            long endAddress = address + 8 + assetPoolSize * Marshal.SizeOf<SoundAlias>();

            // Tracker
            int soundAliasesProcessed = 0;

            // Create Alias Output
            using (StreamWriter writer = new StreamWriter(String.Format("WWII\\DBSound\\S2_LoadedAliases.txt")))
            {
                writer.WriteLine("Name,FileSpec,Secondary");
                // Loop and process the pool
                for (int i = 0; i < assetPoolSize; i++)
                {
                    // Read Sound Asset
                    var soundAsset = GameLoader.Reader.ReadStruct<SoundAlias>(address + (i * Marshal.SizeOf<SoundAlias>()));
                    // Check is this asset entry empty
                    if ((soundAsset.NamePointer > address && soundAsset.NamePointer < endAddress) || soundAsset.NamePointer == 0)
                        continue;
                    // Sound Alias Name
                    string soundAliasName = GameLoader.Reader.ReadNullTerminatedString(soundAsset.NamePointer);
                    string secondaryAlias = "";
                    string fileSpec = "";
                    // Loop and process entries
                    for (byte j = 0; j < soundAsset.EntryCount; j++)
                    {
                        // Read Entry
                        var soundAliasEntry = GameLoader.Reader.ReadStruct<SoundAliasEntry>(soundAsset.DataPointer + (j * Marshal.SizeOf<SoundAliasEntry>()));
                        // Set Data
                        secondaryAlias = soundAliasEntry.SecondaryPointer > 0 ? GameLoader.Reader.ReadNullTerminatedString(soundAliasEntry.SecondaryPointer) : "";
                        fileSpec = GetAliasFileSpec(soundAliasEntry);
                        // Write
                        writer.WriteLine("{0},{1},{2}", soundAliasName, fileSpec, secondaryAlias);
                    }
                    // Increment
                    soundAliasesProcessed++;
                }
            }
            // Done
            Printer.WriteLine("INFO", String.Format("Exported {0} sound aliases successfully", soundAliasesProcessed));
        }

        /// <summary>
        /// Gets Alias File Spec Data
        /// </summary>
        static string GetAliasFileSpec(SoundAliasEntry entry)
        {
            // Get data and set file spec
            var soundFileSpecInfo = GameLoader.Reader.ReadStruct<SoundAliasFileSpec>(entry.SoundFilePointer);
            string fileSpec = "";
            // Check does it exist
            if (soundFileSpecInfo.Exists != 0)
            {
                // Switch Type, append wav for visual purposes
                switch (soundFileSpecInfo.Type)
                {
                    // Loaded
                    case 1:
                        fileSpec = GameLoader.Reader.ReadNullTerminatedString(GameLoader.Reader.ReadInt64(GameLoader.Reader.ReadInt64(entry.SoundFilePointer + 8))) + ".wav";
                        break;
                    // Primed
                    case 2:
                        fileSpec = Path.Combine(
                            GameLoader.Reader.ReadNullTerminatedString(GameLoader.Reader.ReadInt64(entry.SoundFilePointer + 0x10)),
                            GameLoader.Reader.ReadNullTerminatedString(GameLoader.Reader.ReadInt64(entry.SoundFilePointer + 0x24)) + ".wav");
                        break;
                    // Streamed
                    case 3:
                        fileSpec = Path.Combine(
                            GameLoader.Reader.ReadNullTerminatedString(GameLoader.Reader.ReadInt64(entry.SoundFilePointer + 0x18)),
                            GameLoader.Reader.ReadNullTerminatedString(GameLoader.Reader.ReadInt64(entry.SoundFilePointer + 0x20)) + ".wav");
                        break;
                    // Unknown
                    default:
                        break;
                }
            }
            // Ship her back
            return fileSpec;
        }
    }
}
