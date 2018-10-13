using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using PhilLibX;

namespace CoDDataDumper
{
    /// <summary>
    /// Black Ops 4 Logic
    /// </summary>
    public partial class BlackOps4
    {
        /// <summary>
        /// Image Semantics
        /// </summary>
        public static Dictionary<uint, string> MaterialImageSemantics = new Dictionary<uint, string>();

        /// <summary>
        /// Max Hide Tags Count (Assume same as T7 (32))
        /// </summary>
        public const int HideTagsCount = 0x20;

        /// <summary>
        /// Max External Attachment Unique Count
        /// </summary>
        public const int AttachmentTableCount = 0x20;

        /// <summary>
        /// Max Animation Count 
        /// </summary>
        public const int AnimationTableCount = 0x140;

        /// <summary>
        /// Max Alias Count for a Weapon
        /// </summary>
        public const int SoundAliasCount = 0x3C;

        /// <summary>
        /// Material Image Struct
        /// </summary>
        unsafe struct MaterialImage
        {
            /// <summary>
            /// A pointer to the image asset
            /// </summary>
            public long ImagePointer { get; set; }

            /// <summary>
            /// Semantic Hash (i.e. colorMap, colorMap00, etc.) Varies from MTL type, base ones like colorMap are always the same
            /// </summary>
            public uint SemanticHash { get; set; }

            /// <summary>
            /// Unknown Float (Always 1.0?)
            /// </summary>
            public float UnknownFloat { get; set; }

            /// <summary>
            /// Unknown Float (Always 1.0?)
            /// </summary>
            public float UnknownFloat2 { get; set; }

            /// <summary>
            /// End Bytes (Usage, etc.)
            /// </summary>
            public fixed byte Padding[0xC];
        }

        /// <summary>
        /// Material Asset Info
        /// </summary>
        unsafe struct Material
        {
            /// <summary>
            /// Asset Hash
            /// </summary>
            public ulong Hash { get; set; }

            /// <summary>
            /// Unknown Bytes (Settings/Flags)
            /// </summary>
            public fixed byte Padding[0x28];

            /// <summary>
            /// A pointer to the Tech Set this Material uses
            /// </summary>
            public long TechniqueSetPointer { get; set; }

            /// <summary>
            /// A pointer to this Material's Image table
            /// </summary>
            public long ImageTablePointer { get; set; }

            /// <summary>
            /// Unknown Bytes (Settings Pointers, etc.)
            /// </summary>
            public fixed byte Padding2[0xF0];

            /// <summary>
            /// Number of Images this Material has
            /// </summary>
            public byte ImageCount { get; set; }

            /// <summary>
            /// Unknown Bytes (Settings/Flags)
            /// </summary>
            public fixed byte Padding3[0x7];
        }

        /// <summary>
        /// Hide Tags used by Attachments and Weapon Assets
        /// </summary>
        unsafe struct HideTagTable
        {
            /// <summary>
            /// Hide Tags Indices
            /// </summary>
            public fixed int Tags[HideTagsCount];
        }

        /// <summary>
        /// Hide Tags used by Attachments and Weapon Assets
        /// </summary>
        unsafe struct AnimationTable
        {
            /// <summary>
            /// Animation Asset Pointers
            /// </summary>
            public fixed long Animations[AnimationTableCount];
        }

        /// <summary>
        /// Attachment Uniques used by Weapons
        /// </summary>
        unsafe struct AttachmentTable
        {
            /// <summary>
            /// Attachment Asset Pointers
            /// </summary>
            public fixed long Attachment[AttachmentTableCount];
        }

        /// <summary>
        /// Weapon Asset
        /// </summary>
        unsafe struct Weapon
        {
            /// <summary>
            /// Null Padding 
            /// </summary>
            public ulong Null { get; set; }

            /// <summary>
            /// Asset Hash
            /// </summary>
            public ulong Hash { get; set; }

            /// <summary>
            /// Null Padding 
            /// </summary>
            public ulong Padding { get; set; }

            /// <summary>
            /// Asset Hash
            /// </summary>
            public ulong UnknownHash { get; set; }

            /// <summary>
            /// Null Padding 
            /// </summary>
            public ulong Padding2 { get; set; }

            /// <summary>
            /// Asset Hash
            /// </summary>
            public ulong DisplayNameHash { get; set; }

            /// <summary>
            /// Unknown Hashes/Nulls
            /// </summary>
            public fixed byte UnknownHashes[0x30];

            /// <summary>
            /// Sound Alias Hashes
            /// </summary>
            public fixed long SoundAliasHashes[SoundAliasCount];

            /// <summary>
            /// Unknown Pointers
            /// </summary>
            public fixed byte Padding3[0x630];

            /// <summary>
            /// Unknown Floats
            /// </summary>
            public fixed float UnknownFloats[4];

            /// <summary>
            /// Unknown Pointer (seems to point to a null character, could be a string pointer)
            /// </summary>
            public long UnknownPointer { get; set; }

            /// <summary>
            /// A pointer to the Animation Table
            /// </summary>
            public long AnimationTablePointer { get; set; }

            /// <summary>
            /// Unknown Pointer
            /// </summary>
            public long UnknownPointer1 { get; set; }

            /// <summary>
            /// Null Padding
            /// </summary>
            public long NullPadding { get; set; }

            /// <summary>
            /// Pointer to Hide Tags String Indices
            /// </summary>
            public long HideTagsPointer { get; set; }

            /// <summary>
            /// Unknown Bytes/Pointers (Some floats, and probably settings pointers)
            /// </summary>
            public fixed byte Padding4[0x148];

            /// <summary>
            /// A pointer to the pointer to the Model Asset
            /// </summary>
            public long AttachmentTablePointer { get; set; }

            /// <summary>
            /// Unknown Pointers
            /// </summary>
            public fixed byte Padding6[0x20];

            /// <summary>
            /// A pointer to the pointer to the Model Asset
            /// </summary>
            public long ViewmodelPointer { get; set; }

            /// <summary>
            /// Unknown Pointers
            /// </summary>
            public fixed byte Padding7[0x10];

            /// <summary>
            /// Skip attachment models (we handle them separately below since C# doesn't like fixed buffers for any type >:[)
            /// </summary>
            public fixed byte Padding8[0x140];

            /// <summary>
            /// Unknown Pointers
            /// </summary>
            public fixed byte Padding9[0xF0];

            /// <summary>
            /// A pointer to the pointer to the Model Asset
            /// </summary>
            public long WorldmodelPointer { get; set; }

            /// <summary>
            /// Unknown Bytes (Remaining settings, pointers, etc.)
            /// </summary>
            public fixed byte Padding10[0x808];
        }

        /// <summary>
        /// Attachment Unique Model
        /// </summary>
        unsafe struct AttachmentUniqueModel
        {
            /// <summary>
            /// A pointer to the pointer to the Model Asset
            /// </summary>
            public long ModelPointer { get; set; }

            /// <summary>
            /// A pointer to the pointer to the Model Asset
            /// </summary>
            public long ADSModelPointer { get; set; }

            /// <summary>
            /// A pointer to the Hide Tags Table
            /// </summary>
            public long TagPointer { get; set; }

            /// <summary>
            /// A pointer to the Position Data
            /// </summary>
            public long PositionPointer { get; set; }

            /// <summary>
            /// A pointer to the Rotation
            /// </summary>
            public long RotationPointer { get; set; }
        }

        unsafe struct AttachmentUnique
        {
            /// <summary>
            /// Asset Hash
            /// </summary>
            public ulong Null { get; set; }

            /// <summary>
            /// Asset Hash
            /// </summary>
            public ulong Hash { get; set; }

            /// <summary>
            /// Unknown Bytes
            /// </summary>
            public fixed byte Padding3[0x98];

            /// <summary>
            /// Pointer to Hide Tags String Indices
            /// </summary>
            public long HideTagsPointer { get; set; }

            /// <summary>
            /// Unknown Pointer
            /// </summary>
            public long UnknownPointer1 { get; set; }

            /// <summary>
            /// Unknown Int 
            /// </summary>
            public int UnknownInt { get; set; }

            /// <summary>
            /// Unknown Int 
            /// </summary>
            public int UnknownInt1 { get; set; }

            /// <summary>
            /// Unknown Int 
            /// </summary>
            public int UnknownInt2 { get; set; }

            /// <summary>
            /// Null Padding
            /// </summary>
            public int Padding4 { get; set; }

            /// <summary>
            /// Skip models (we handle them separately below since C# doesn't like fixed buffers for any type >:[)
            /// </summary>
            public fixed byte Padding5[0x198];

            /// <summary>
            /// Skip null bytes
            /// </summary>
            public fixed byte Padding6[0x50];

            /// <summary>
            /// Skip attachment models (we handle them separately below since C# doesn't like fixed buffers for any type >:[)
            /// </summary>
            public fixed byte Padding7[0x140];

            /// <summary>
            /// Skip null bytes
            /// </summary>
            public fixed byte Padding8[0x80];

            /// <summary>
            /// A pointer to the Animation Table
            /// </summary>
            public long AnimationTablePointer { get; set; }

            /// <summary>
            /// Skip Unknown bytes (A lot of 0xFF values at the end, might correlate with values that are usually -1 in APE for T7?)
            /// </summary>
            public fixed byte Padding9[0x438];
        }

        /// <summary>
        /// An Internal Weapon Attachment Model
        /// </summary>
        struct AttachmentModel
        {
            /// <summary>
            /// A pointer to the pointer to the Model Asset
            /// </summary>
            public long ModelPointer { get; set; }

            /// <summary>
            /// A pointer to the string index of the tag string
            /// </summary>
            public long TagPointer { get; set; }

            /// <summary>
            /// A pointer to the XYZ Position Data
            /// </summary>
            public long PositionPointer { get; set; }

            /// <summary>
            /// A pointer to the XYZ Rotation Data
            /// </summary>
            public long RotationPointer { get; set; }
        }

        /// <summary>
        /// Internal Weapon Attachment Location Data
        /// </summary>
        struct AttachmentLocationData
        {
            /// <summary>
            /// X Value
            /// </summary>
            public float X { get; set; }

            /// <summary>
            /// Y Value
            /// </summary>
            public float Y { get; set; }

            /// <summary>
            /// Z Value
            /// </summary>
            public float Z { get; set; }
        }

        /// <summary>
        /// Asset Pool Data
        /// </summary>
        struct AssetPool
        {
            /// <summary>
            /// A pointer to the asset pool
            /// </summary>
            public long PoolPointer { get; set; }

            /// <summary>
            /// Entry Size
            /// </summary>
            public int AssetSize { get; set; }

            /// <summary>
            /// Max Asset Count/Pool Size
            /// </summary>
            public int PoolSize { get; set; }

            /// <summary>
            /// Null Padding
            /// </summary>
            public int Padding { get; set; }

            /// <summary>
            /// Numbers of Assets in this Pool
            /// </summary>
            public int AssetCount { get; set; }

            /// <summary>
            /// Next Free Header/Slot
            /// </summary>
            public long NextSlot { get; set; }
        }

        /// <summary>
        /// Basic Sound Data
        /// </summary>
        struct Sound
        {
            /// <summary>
            /// A pointer to the SAB Name String
            /// </summary>
            public long SABNamePointer { get; set; }

            /// <summary>
            /// Null Padding
            /// </summary>
            public long Padding { get; set; }

            /// <summary>
            /// Pool Hash
            /// </summary>
            public ulong Hash { get; set; }

            /// <summary>
            /// Sound Pool Name Pointer
            /// </summary>
            public long NamePointer { get; set; }

            /// <summary>
            /// Language Pointer (i.e. English, etc.)
            /// </summary>
            public long LanguagePointer { get; set; }

            /// <summary>
            /// Language ID Pointer (i.e. en, etc.)
            /// </summary>
            public long LanguageIDPointer { get; set; }

            /// <summary>
            /// Number of Aliases 
            /// </summary>
            public long AliasCount { get; set; }

            /// <summary>
            /// Pointer to the start of the Sound Alias Data/First Entry
            /// </summary>
            public long FirstEntryPointer { get; set; }

            /// <summary>
            /// Pointer to the end of this pool
            /// </summary>
            public long EndPointer { get; set; }
        }

        /// <summary>
        /// Sound Alias Data
        /// </summary>
        struct SoundAlias
        {
            /// <summary>
            /// Null Padding
            /// </summary>
            public long Padding { get; set; }

            /// <summary>
            /// Sound Alias Hash
            /// </summary>
            public ulong Hash { get; set; }

            /// <summary>
            /// Pointer to the Sound Alias
            /// </summary>
            public long DataPointer { get; set; }

            /// <summary>
            /// Number of Entries for this Alias
            /// </summary>
            public int EntryCount { get; set; }

            /// <summary>
            /// Unknown Int
            /// </summary>
            public int UnknownInt { get; set; }

            /// <summary>
            /// Unknown Long
            /// </summary>
            public long Unknown { get; set; }
        }

        /// <summary>
        /// Localized String Data
        /// </summary>
        struct Localized
        {
            /// <summary>
            /// Pointer to the Localized String
            /// </summary>
            public long StringPointer { get; set; }

            /// <summary>
            /// Padding
            /// </summary>
            public long Padding { get; set; }

            /// <summary>
            /// Localized Hash
            /// </summary>
            public ulong Hash { get; set; }
        }

        /// <summary>
        /// Loaded Localized Strings
        /// </summary>
        public static Dictionary<ulong, string> LocalizedStrings = new Dictionary<ulong, string>();

        /// <summary>
        /// Attachment Indices (for Internal Weapon Attachments)
        /// </summary>
        public static Dictionary<int, string> WeaponAttachmentIndices = new Dictionary<int, string>()
        {
            { 0,        "scope_view" },
            { 1,        "scope_ads_view" },
            { 4,        "clip_view" },
            { 5,        "scope_world" },
            { 9,        "clip_world" },
        };

        /// <summary>
        /// Animation Indices 
        /// </summary>
        public static Dictionary<int, string> WeaponAnimationIndices = new Dictionary<int, string>()
        {
            { 39,       "jump_land" },
            { 47,       "walk_f" },
            { 153,      "gunbutt_swipe" },
            { 157,      "gunbutt_swipe" },
            { 158,      "gunbutt_swipe" },
            { 170,      "gunbutt_swipe" },
            { 171,      "gunbutt_swipe" },
            { 173,      "gunbutt_swipe" },
            { 174,      "gunbutt_swipe" },
            { 175,      "gunbutt_swipe" },
            { 128,      "idle" },
            { 132,      "idle_empty" },
            { 134,      "fire" },
            { 135,      "fire_unknown" },
            { 256,      "fire_ads" },
            { 151,      "fire_last" },
            { 152,      "rechamber" },
            { 190,      "reload" },
            { 191,      "reload_loop" },
            { 194,      "reload_in" },
            { 195,      "reload_out" },
            { 192,      "Reload Empty" },
            { 206,      "first_raise" },
            { 202,      "pullout" },
            { 207,      "putaway" },
            { 211,      "pullout_empty" },
            { 212,      "putaway_empty" },
            { 215,      "pullout_quick" },
            { 216,      "putaway_quick" },
            { 217,      "sprint_pullout" },
            { 218,      "sprint_putaway" },
            { 219,      "pullout_alt" },
            { 220,      "putaway_alt" },
            { 224,      "sprint_in" },
            { 225,      "sprint_loop" },
            { 227,      "sprint_out" },
            { 240,      "crawl_in" },
            { 241,      "crawl_f" },
            { 242,      "crawl_b" },
            { 243,      "crawl_r" },
            { 244,      "crawl_in" },
            { 245,      "crawl_out" },
            { 253,      "fire_ads_unknown" },
            { 255,      "fire_rechamber_ads" },
            { 257,      "rechamber_ads" },
            { 260,      "inspect" },
            { 264,      "swim_uw_out" },
            { 266,      "swim_uw_in" },
            { 267,      "swim_uw_fire" },
            { 268,      "swim_uw_fire_ads" },
            { 269,      "swim_uw_idle" },
            { 275,      "swim_uw_sprint_in" },
            { 276,      "swim_uw_sprint_loop" },
            { 277,      "swim_tw_sprint_loop" },
            { 278,      "swim_uw_sprint_out" },
            { 308,      "ads_base_up" },
            { 309,      "ads_base_down" },
        };

        /// <summary>
        /// Game Offsets
        /// </summary>
        public static DBGameInfo[] GameOffsets =
        {
            new DBGameInfo(0x917FBD0, 0, 0x7FE3620),
            new DBGameInfo(0x7A6CAA0, 0, 0x6866220),
        };

        /// <summary>
        /// String Table Address
        /// </summary>
        public static long StringTableAddress = 0;

        /// <summary>
        /// Gets name of attachment by index
        /// </summary>
        public static string GetAttachmentName(int attachmentIndex)
        {
            return WeaponAttachmentIndices.TryGetValue(attachmentIndex, out string result) ? result : attachmentIndex.ToString();
        }

        /// <summary>
        /// Gets Animation Usage by Index
        /// </summary>
        public static string GetAnimationUsage(int animationIndex)
        {
            return WeaponAnimationIndices.TryGetValue(animationIndex, out string result) ? result : animationIndex.ToString();
        }

        /// <summary>
        /// Initiates Black Ops 4 and loads in Address Info
        /// </summary>
        public static bool Process(bool isMP = true)
        {
            // Get Base Address for ASLR and Scans
            long baseAddress = GameLoader.Reader.GetBaseAddress();

            // Loop Cached Offsets
            foreach (var gameOffset in GameOffsets)
            {
                // Load Asset Pool Data
                var materialPoolData    = GameLoader.Reader.ReadStruct<AssetPool>(baseAddress + gameOffset.AssetPoolAddress + 0x20 * 6);
                var attachmentPoolData  = GameLoader.Reader.ReadStruct<AssetPool>(baseAddress + gameOffset.AssetPoolAddress + 0x20 * 0x1C);
                var weaponPoolData      = GameLoader.Reader.ReadStruct<AssetPool>(baseAddress + gameOffset.AssetPoolAddress + 0x20 * 0x14);
                var soundPoolData       = GameLoader.Reader.ReadStruct<AssetPool>(baseAddress + gameOffset.AssetPoolAddress + 0x20 * 0xA);
                var localizedPoolData   = GameLoader.Reader.ReadStruct<AssetPool>(baseAddress + gameOffset.AssetPoolAddress + 0x20 * 0x10);
                var xmodelPoolData      = GameLoader.Reader.ReadStruct<AssetPool>(baseAddress + gameOffset.AssetPoolAddress + 0x20 * 0x4);

                // Check XModel Hash
                if (GameLoader.Reader.ReadUInt64(xmodelPoolData.PoolPointer + 8) == 0x04647533e968c910)
                {
                    // Set String Table
                    StringTableAddress = baseAddress + gameOffset.StringTableAddress;

                    // Load Localized Strings (Required to identify weapons)
                    LoadLocalizedStrings(localizedPoolData);

                    // Export Supported Asset Pools
                    ExportAttachmentDataT8(attachmentPoolData);
                    ExportWeaponData(weaponPoolData);
                    ExportSoundAliases(soundPoolData);

                    // We done it lads
                    return true;
                }
            }

            // Scan memory
            var dbAssetsScan = GameLoader.Reader.FindBytes(new byte?[] { 0x48, 0x89, 0x5C, 0x24, null, 0x57, 0x48, 0x83, 0xEC, null, 0x0F, 0xB6, 0xF9, 0x48, 0x8D, 0x05 }, baseAddress, baseAddress + GameLoader.Reader.GetModuleMemorySize(), true);
            var strTableScan = GameLoader.Reader.FindBytes(new byte?[] { 0x48, 0x8B, 0x53, null, 0x48, 0x85, 0xD2, 0x74, null, 0x48, 0x8B, 0x03, 0x48, 0x89, 0x02, 0x4C, 0x8D, 0x1D }, baseAddress, baseAddress + GameLoader.Reader.GetModuleMemorySize(), true);

            // Check for Matches
            if(dbAssetsScan.Length > 0 && strTableScan.Length > 0)
            {

                // Set Data
                var gameOffset = new DBGameInfo(
                    GameLoader.Reader.ReadInt32(dbAssetsScan[0] + 0x10) + dbAssetsScan[0] + 0x14,
                    0,
                    GameLoader.Reader.ReadInt32(strTableScan[0] + 0x12) + strTableScan[0] + 0x16);

                // Load Asset Pool Data
                var materialPoolData   = GameLoader.Reader.ReadStruct<AssetPool>(gameOffset.AssetPoolAddress + 0x20 * 6);
                var attachmentPoolData = GameLoader.Reader.ReadStruct<AssetPool>(gameOffset.AssetPoolAddress + 0x20 * 0x1C);
                var weaponPoolData     = GameLoader.Reader.ReadStruct<AssetPool>(gameOffset.AssetPoolAddress + 0x20 * 0x14);
                var soundPoolData      = GameLoader.Reader.ReadStruct<AssetPool>(gameOffset.AssetPoolAddress + 0x20 * 0xA);
                var localizedPoolData  = GameLoader.Reader.ReadStruct<AssetPool>(gameOffset.AssetPoolAddress + 0x20 * 0x10);
                var xmodelPoolData     = GameLoader.Reader.ReadStruct<AssetPool>(gameOffset.AssetPoolAddress + 0x20 * 0x4);

                // Check XModel Hash
                if (GameLoader.Reader.ReadUInt64(xmodelPoolData.PoolPointer + 8) == 0x04647533e968c910)
                {
                    // Set String Table
                    StringTableAddress = gameOffset.StringTableAddress;

                    // Load Localized Strings (Required to identify weapons)
                    LoadLocalizedStrings(localizedPoolData);

                    // Export Supported Asset Pools
                    ExportAttachmentDataT8(attachmentPoolData);
                    ExportWeaponData(weaponPoolData);
                    ExportSoundAliases(soundPoolData);

                    // We done it lads
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Decrypts a charact from a string
        /// </summary>
        static byte Decrypt(byte input, byte key)
        {
            // If our key equals the input
            if (input == key)
                return input;

            return (byte)(input ^ key);
        }

        /// <summary>
        /// Loads Localized Strings from Black Ops 4's Memory
        /// </summary>
        static void LoadLocalizedStrings(AssetPool assetPool)
        {
            // Info
            Printer.WriteLine("INFO", "Loading Localized Strings.....");

            // Set Addresses
            long address = assetPool.PoolPointer;
            long endAddress = assetPool.PoolSize * assetPool.AssetSize + address;

            // Tracker
            long localizedLoaded = 0;

            using(StreamWriter writer = new StreamWriter("output.txt"))
            // Loop
            for (int i = 0; i < assetPool.PoolSize; i++)
            {
                // Read Localized Entry
                var data = GameLoader.Reader.ReadStruct<Localized>(address + (i * assetPool.AssetSize));

                // Check for null entry/empty slot
                if ((data.StringPointer > address && data.StringPointer < endAddress) || data.StringPointer == 0)
                    continue;

                // Read Data
                byte[] stringInfo = GameLoader.Reader.ReadBytes(data.StringPointer, 2);
                byte[] result = GameLoader.Reader.ReadBytes(data.StringPointer + 2, stringInfo[1] - 1);

                // Set Key
                byte xorKey = stringInfo[0];

                switch (xorKey)
                {
                    case 165:
                        for (int x = 0; x < result.Length; x++, xorKey--) result[x] = Decrypt(result[x], xorKey);
                        break;
                    case 175:
                        for (int x = 0; x < result.Length; x++, xorKey++) result[x] = Decrypt(result[x], xorKey);
                        break;
                    case 185:
                        for (int x = 0; x < result.Length; x++, xorKey -= (byte)(x - 1 + 1)) result[x] = Decrypt(result[x], xorKey);
                        break;
                    case 189:
                        for (int x = 0; x < result.Length; x++, xorKey += (byte)(x - 1 + 1)) result[x] = Decrypt(result[x], xorKey); ;
                        break;
                }

                // Add 
                LocalizedStrings[data.Hash] = Encoding.ASCII.GetString(result);

                    writer.WriteLine("{0}, {1}, {2}", data.Hash, address + (i * assetPool.AssetSize), LocalizedStrings[data.Hash]);

                // Increment
                    localizedLoaded++;
            }
            // Done
            Printer.WriteLine("INFO", String.Format("Loaded {0} Localized Strings successfully", localizedLoaded));
        }

        /// <summary>
        /// Exports Material Data from Black Ops 4
        /// </summary>
        static void ExportMaterialData(AssetPool assetPool)
        {
            // Info
            Printer.WriteLine("INFO", "Exporting Materials.....");

            // Create Dir
            Directory.CreateDirectory("BO4\\DBMaterials");

            // Set Addresses
            long address = assetPool.PoolPointer;
            long endAddress = assetPool.PoolSize * assetPool.AssetSize + address;

            // Tracker
            long materialsExported = 0;

            // Loop
            for (int i = 0; i < assetPool.PoolSize; i++)
            {
                // Load Sound Bank
                var materialAsset = GameLoader.Reader.ReadStruct<Material>(address + (i * assetPool.AssetSize));

                // Check for null bank
                if (((long)materialAsset.Hash > address && (long)materialAsset.Hash < endAddress) || (long)materialAsset.Hash == 0)
                    continue;

                // Output to file for each bank
                using (StreamWriter writer = new StreamWriter(Path.Combine("BO4\\DBMaterials", "xmaterial_" + materialAsset.Hash.ToString("x") + ".txt")))
                {
                    writer.WriteLine("TechSet : {0:x}", GameLoader.Reader.ReadUInt64(materialAsset.TechniqueSetPointer));

                    // Write Techset Name 
                    writer.WriteLine("Techinque Set - {0}", GameLoader.Reader.ReadNullTerminatedString(GameLoader.Reader.ReadInt64(materialAsset.TechniqueSetPointer)));

                    // Write Images
                    for (int j = 0; j < materialAsset.ImageCount; j++)
                    {
                        // Material Image Struct
                        var materialImage = GameLoader.Reader.ReadStruct<MaterialImage>(materialAsset.ImageTablePointer + (j * Marshal.SizeOf<MaterialImage>()));

                        // Write Data
                        writer.WriteLine("{0} - ximage_{1:x}", GetImageSemantic(materialImage.SemanticHash).PadRight(32), GameLoader.Reader.ReadUInt64(materialImage.ImagePointer + 0x20));
                    }
                }

                // Increment
                materialsExported++;
            }

            // Done
            Printer.WriteLine("INFO", String.Format("Exported {0} Materials successfully", materialsExported));
        }

        /// <summary>
        /// Gets Image Semantic from List
        /// </summary>
        static string GetImageSemantic(uint hash)
        {
            return MaterialImageSemantics.TryGetValue(hash, out string result) ? result : "Unknown Semantic: " + hash.ToString("X");
        }

        /// <summary>
        /// Exports Attachment Assets and Basic Info about them
        /// </summary>
        unsafe static void ExportAttachmentDataT8(AssetPool assetPool)
        {
            // Info
            Printer.WriteLine("INFO", "Exporting Attachments.....");

            // Create Output
            Directory.CreateDirectory("BO4\\DBAttachmentUnique");

            // Set Addresses
            long address = assetPool.PoolPointer;
            long endAddress = assetPool.PoolSize * assetPool.AssetSize + address;

            // Tracker
            long attachmentsExported = 0;

            // Loop
            for (int i = 0; i < assetPool.PoolSize; i++)
            {
                // Read Asset
                var data = GameLoader.Reader.ReadStruct<AttachmentUnique>(address + (i * assetPool.AssetSize));

                // Check for null asset
                if (((long)data.Null > address && (long)data.Null < endAddress) || (long)data.Hash == 0)
                    continue;

                // Try write the attachment, if we fail, move on
                try
                {
                    // Greyhound Search String
                    string greyhoundSearchString = "";

                    // Dump Output
                    using (StreamWriter writer = new StreamWriter(Path.Combine("BO4\\DBAttachmentUnique", String.Format("xattachment_{0:x}.txt", data.Hash))))
                    {
                        // Write Models
                        writer.WriteLine("// XModels");

                        // Each AttachmentUnique Asset can have 2 attachments
                        for (int k = 0; k < 2; k++)
                        {
                            // 4 Models (2 sets of View and World)
                            for (int j = 0; j < 4; j++)
                            {
                                // Read Model Data 
                                var model = GameLoader.Reader.ReadStruct<AttachmentUniqueModel>(address + (i * assetPool.AssetSize) + (k == 0 ? 0xC8 : 0x1C0) + 0x28 * j);

                                // Main Model Pointers
                                long modelPointer = GameLoader.Reader.ReadInt64(model.ModelPointer);
                                long adsModelPointer = GameLoader.Reader.ReadInt64(model.ADSModelPointer);

                                // Only write if we have a match
                                if (modelPointer > 0)
                                {
                                    // Read Location Data
                                    var position = GameLoader.Reader.ReadStruct<AttachmentLocationData>(model.PositionPointer);
                                    var rotation = GameLoader.Reader.ReadStruct<AttachmentLocationData>(model.RotationPointer);

                                    // Add to search string
                                    greyhoundSearchString += (GameLoader.Reader.ReadUInt64(modelPointer + 8) & 0xFFFFFFFFFFFFFFF).ToString("x") + ",";
                                    greyhoundSearchString += (GameLoader.Reader.ReadUInt64(adsModelPointer + 8) & 0xFFFFFFFFFFFFFFF).ToString("x") + ",";

                                    // Write Data
                                    writer.WriteLine("Model           =   xmodel_{0:x}", GameLoader.Reader.ReadUInt64(modelPointer + 8) & 0xFFFFFFFFFFFFFFF);
                                    if (adsModelPointer > 0)
                                        writer.WriteLine("ADS Model       =   xmodel_{0:x}", GameLoader.Reader.ReadUInt64(adsModelPointer + 8) & 0xFFFFFFFFFFFFFFF);
                                    writer.WriteLine("Tag             =   {0}",
                                        GetString(GameLoader.Reader.ReadInt32(model.TagPointer))
                                        );
                                    writer.WriteLine("Position        =   Inches : ({0:0.0000}, {1:0.0000}, {2:0.0000}) CM : ({3:0.0000}, {4:0.0000}, {5:0.0000})",
                                        position.X,
                                        position.Y,
                                        position.Z,
                                        position.X * 2.54,
                                        position.Y * 2.54,
                                        position.Z * 2.54
                                        );
                                    writer.WriteLine("Rotation        =   ({0:0.0000}, {1:0.0000}, {2:0.0000})\n",
                                        rotation.X,
                                        rotation.Y,
                                        rotation.Z
                                        );
                                }
                            }
                        }

                        // Read Hide Tags (32 Max)
                        var hideTags = GameLoader.Reader.ReadStruct<HideTagTable>(data.HideTagsPointer);
                        writer.WriteLine("// Hide Tags");
                        for (int j = 0; j < HideTagsCount; j++)
                            if (hideTags.Tags[j] > 0)
                                writer.WriteLine("{0}", GetString(hideTags.Tags[j]));

                        // Write Spacer
                        writer.WriteLine();

                        // 10 Possible Internal Attachment Models
                        for (int j = 0; j < 10; j++)
                        {
                            // Read Internal Attachment Data
                            var attachmentModel = GameLoader.Reader.ReadStruct<AttachmentModel>(address + (i * assetPool.AssetSize) + 0x2B0 + (0x20 * j));

                            // Model Pointer
                            long modelPointer = GameLoader.Reader.ReadInt64(attachmentModel.ModelPointer);

                            // Only write if we have a match
                            if (modelPointer > 0)
                            {
                                // Write Index/Name (use Index for name)
                                writer.WriteLine("Attachment: {0}", GetAttachmentName(j));

                                // Read Location Data (Rotation/Offset)
                                var position = GameLoader.Reader.ReadStruct<AttachmentLocationData>(attachmentModel.PositionPointer);
                                var rotation = GameLoader.Reader.ReadStruct<AttachmentLocationData>(attachmentModel.RotationPointer);

                                // Add to search string
                                greyhoundSearchString += (GameLoader.Reader.ReadUInt64(modelPointer + 8) & 0xFFFFFFFFFFFFFFF).ToString("x") + ",";

                                // Write Data
                                writer.WriteLine("Model           =   xmodel_{0:x}", GameLoader.Reader.ReadUInt64(modelPointer + 8) & 0xFFFFFFFFFFFFFFF);
                                writer.WriteLine("Tag             =   {0}",
                                    GetString(GameLoader.Reader.ReadInt32(attachmentModel.TagPointer))
                                    );
                                writer.WriteLine("Position        =   Inches : ({0:0.0000}, {1:0.0000}, {2:0.0000}) CM : ({3:0.0000}, {4:0.0000}, {5:0.0000})",
                                    position.X,
                                    position.Y,
                                    position.Z,
                                    position.X * 2.54,
                                    position.Y * 2.54,
                                    position.Z * 2.54
                                    );
                                writer.WriteLine("Rotation        =   ({0:0.0000}, {1:0.0000}, {2:0.0000})\n",
                                    rotation.X,
                                    rotation.Y,
                                    rotation.Z
                                    );
                            }
                        }

                        // Write Animation Table
                        var animationTable = GameLoader.Reader.ReadStruct<AnimationTable>(data.AnimationTablePointer);
                        writer.WriteLine("// xAnims");
                        for (int j = 0; j < AnimationTableCount; j++)
                        {
                            if (animationTable.Animations[j] > 0)
                            {
                                writer.WriteLine("xAnim {1}        =   xanim_{0:x}", GameLoader.Reader.ReadInt64(animationTable.Animations[j] + 120) & 0xFFFFFFFFFFFFFFF, j.ToString().PadRight(16));

                                // Add to search string
                                greyhoundSearchString += (GameLoader.Reader.ReadUInt64(animationTable.Animations[j] + 120) & 0xFFFFFFFFFFFFFFF).ToString("x") + ",";
                            }
                        }

                        // Write Search String
                        writer.WriteLine("\n// Greyhound Search String\n{0}", greyhoundSearchString);

                        // Increment
                        attachmentsExported++;
                    }
                }
                // Failed, move on
                catch { }
            }

            // Done
            Printer.WriteLine("INFO", String.Format("Exported {0} Attachments successfully", attachmentsExported));
        }

        /// <summary>
        /// Exports Weapon Data
        /// </summary>
        unsafe static void ExportWeaponData(AssetPool assetPool)
        {
            // Info
            Printer.WriteLine("INFO", "Exporting Weapons.....");

            // Create Dir
            Directory.CreateDirectory("BO4\\DBWeapons");

            // Set Addresses
            long address = assetPool.PoolPointer;
            long endAddress = assetPool.PoolSize * assetPool.AssetSize + address;

            // Tracker
            int weaponsExported = 0;

            // Loop
            for (int i = 0; i < assetPool.PoolSize; i++)
            {
                // Load Weapon
                var weaponAsset = GameLoader.Reader.ReadStruct<Weapon>(address + (i * assetPool.AssetSize));

                // Check for Null Asset
                if (((long)weaponAsset.Null > address && (long)weaponAsset.Null < endAddress) || (long)weaponAsset.Hash == 0)
                    continue;

                // Try write this weapon, if we fail, move on
                try
                {
                    // Check do we have the display hash (I haven't see this return false)
                    if (LocalizedStrings.TryGetValue(weaponAsset.DisplayNameHash, out string displayName))
                    {
                        // Greyhound Search String
                        string greyhoundSearchString = "";

                        // Open Writer
                        using (StreamWriter writer = new StreamWriter(Path.Combine("BO4\\DBWeapons", String.Format("{0}_{1:x}.txt", displayName, weaponAsset.Hash))))
                        {
                            // Read Model Pointers
                            long viewmodelPtr = GameLoader.Reader.ReadInt64(weaponAsset.ViewmodelPointer);
                            long worldmodelPtr = GameLoader.Reader.ReadInt64(weaponAsset.WorldmodelPointer);

                            // Add to search string
                            greyhoundSearchString += (GameLoader.Reader.ReadUInt64(viewmodelPtr + 8) & 0xFFFFFFFFFFFFFFF).ToString("x") + ",";
                            greyhoundSearchString += (GameLoader.Reader.ReadUInt64(worldmodelPtr + 8) & 0xFFFFFFFFFFFFFFF).ToString("x") + ",";

                            // Dump 'em
                            writer.WriteLine("// XModels");
                            writer.WriteLine("Viewmodel                 =   xmodel_{0:x}", viewmodelPtr > 0 ? GameLoader.Reader.ReadUInt64(viewmodelPtr + 8) & 0xFFFFFFFFFFFFFFF : 0);
                            writer.WriteLine("Worldmodel                =   xmodel_{0:x}\n", worldmodelPtr > 0 ? GameLoader.Reader.ReadUInt64(worldmodelPtr + 8) & 0xFFFFFFFFFFFFFFF : 0);

                            // Read Hide Tags (32 Max)
                            var hideTags = GameLoader.Reader.ReadStruct<HideTagTable>(weaponAsset.HideTagsPointer);
                            writer.WriteLine("// Hide Tags");
                            for (int j = 0; j < HideTagsCount; j++)
                                if (hideTags.Tags[j] > 0)
                                    writer.WriteLine("{0}", GetString(hideTags.Tags[j]));

                            // Write Spacer
                            writer.WriteLine();

                            // 10 Possible Internal Attachment Models
                            for (int j = 0; j < 10; j++)
                            {
                                // Read Internal Attachment Data
                                var attachmentModel = GameLoader.Reader.ReadStruct<AttachmentModel>(address + (i * assetPool.AssetSize) + 0xA30 + (0x20 * j));

                                // Model Pointer
                                long modelPointer = GameLoader.Reader.ReadInt64(attachmentModel.ModelPointer);

                                // Only write if we have a match
                                if (modelPointer > 0)
                                {
                                    // Write Index/Name (use Index for name)
                                    writer.WriteLine("Attachment: {0}", GetAttachmentName(j));

                                    // Read Location Data (Rotation/Offset)
                                    var position = GameLoader.Reader.ReadStruct<AttachmentLocationData>(attachmentModel.PositionPointer);
                                    var rotation = GameLoader.Reader.ReadStruct<AttachmentLocationData>(attachmentModel.RotationPointer);

                                    // Add to search string
                                    greyhoundSearchString += (GameLoader.Reader.ReadUInt64(modelPointer + 8) & 0xFFFFFFFFFFFFFFF).ToString("x") + ",";

                                    // Write Data
                                    writer.WriteLine("Model           =   xmodel_{0:x}", GameLoader.Reader.ReadUInt64(modelPointer + 8) & 0xFFFFFFFFFFFFFFF);
                                    writer.WriteLine("Tag             =   {0}",
                                        GetString(GameLoader.Reader.ReadInt32(attachmentModel.TagPointer))
                                        );
                                    writer.WriteLine("Position        =   Inches : ({0:0.0000}, {1:0.0000}, {2:0.0000}) CM : ({3:0.0000}, {4:0.0000}, {5:0.0000})",
                                        position.X,
                                        position.Y,
                                        position.Z,
                                        position.X * 2.54,
                                        position.Y * 2.54,
                                        position.Z * 2.54
                                        );
                                    writer.WriteLine("Rotation        =   ({0:0.0000}, {1:0.0000}, {2:0.0000})\n",
                                        rotation.X,
                                        rotation.Y,
                                        rotation.Z
                                        );
                                }
                            }

                            // Write Animation Table
                            var animationTable = GameLoader.Reader.ReadStruct<AnimationTable>(weaponAsset.AnimationTablePointer);
                            writer.WriteLine("// xAnims");
                            for (int j = 0; j < AnimationTableCount; j++)
                            {
                                if (animationTable.Animations[j] > 0)
                                {
                                    writer.WriteLine("xAnim {1}        =   xanim_{0:x}", GameLoader.Reader.ReadInt64(animationTable.Animations[j] + 120) & 0xFFFFFFFFFFFFFFF, GetAnimationUsage(j).PadRight(32));

                                    // Add to 
                                    greyhoundSearchString += (GameLoader.Reader.ReadInt64(animationTable.Animations[j] + 120) & 0xFFFFFFFFFFFFFFF).ToString("x") + ",";
                                }
                            }

                            // Write Spacer
                            writer.WriteLine();

                            // Write Attachments
                            var attachmentTable = GameLoader.Reader.ReadStruct<AttachmentTable>(weaponAsset.AttachmentTablePointer);
                            writer.WriteLine("// Attachments");
                            for (int j = 0; j < AttachmentTableCount; j++)
                                if (attachmentTable.Attachment[j] > 0)
                                    writer.WriteLine("Attachment {1}        =   {0:x}", GameLoader.Reader.ReadInt64(attachmentTable.Attachment[j] + 8) & 0xFFFFFFFFFFFFFFF, j.ToString().PadRight(16));

                            // Write Spacer
                            writer.WriteLine();

                            // Write Sound Aliases
                            writer.WriteLine("// Sound Aliases");
                            for (int j = 0; j < SoundAliasCount; j++)
                            {
                                if (weaponAsset.SoundAliasHashes[j] > 0)
                                    writer.WriteLine("Sound Alias {1}  =   {0:x}", weaponAsset.SoundAliasHashes[j] & 0xFFFFFFFFFFFFFFF, j.ToString().PadRight(16));
                            }

                            // Write Search String
                            writer.WriteLine("\n// Greyhound Search String\n{0}", greyhoundSearchString);

                            // Increment
                            weaponsExported++;
                        }
                    }
                }
                // Failed, move on
                catch { }
            }

            // Done
            Printer.WriteLine("INFO", String.Format("Exported {0} Weapons successfully", weaponsExported));
        }

        /// <summary>
        /// Exports Sound Aliases from Black Ops 4
        /// </summary>
        static void ExportSoundAliases(AssetPool assetPool)
        {
            // Info
            Printer.WriteLine("INFO", "Exporting Sound Aliases.....");

            // Create Dir
            Directory.CreateDirectory("BO4\\DBSound");

            // Set Addresses
            long address = assetPool.PoolPointer;
            long endAddress = assetPool.PoolSize * assetPool.AssetSize + address;

            // Tracker
            long soundBanksExported = 0;
            long soundAliasesExported = 0;

            // Try parse the aliases, if we fail, move on
            try
            {
                // Loop
                for (int i = 0; i < assetPool.PoolSize; i++)
                {
                    // Load Sound Bank
                    var soundPool = GameLoader.Reader.ReadStruct<Sound>(address + (i * assetPool.AssetSize));

                    // Check for null bank
                    if ((soundPool.SABNamePointer > address && soundPool.SABNamePointer < endAddress) || soundPool.SABNamePointer == 0)
                        continue;

                    // Output to file for each bank
                    using (StreamWriter writer = new StreamWriter(Path.Combine("BO4\\DBSound", GameLoader.Reader.ReadNullTerminatedString(soundPool.SABNamePointer)) + ".txt"))
                    {
                        // Write CSV Header
                        writer.WriteLine("Name,Secondary,FileSpec,FileSpecSustain,FileSpecRelease");

                        // Loop Alias Count
                        for (int j = 0; j < soundPool.AliasCount; j++)
                        {
                            // Read Alias
                            var sound = GameLoader.Reader.ReadStruct<SoundAlias>(soundPool.FirstEntryPointer + (j * 40));

                            // Loop Entry Count
                            for (int k = 0; k < sound.EntryCount; k++)
                            {
                                // Get Sound Alias Entry Buffer
                                byte[] soundAliasBuffer = GameLoader.Reader.ReadBytes(sound.DataPointer + (k * 400), 400);

                                // Write String Data
                                writer.WriteLine("{0:x},{1:x},{2:x},{3:x},{4:x}",
                                    BitConverter.ToUInt64(soundAliasBuffer, 8) & 0xFFFFFFFFFFFFFFF,
                                    BitConverter.ToUInt64(soundAliasBuffer, 40) & 0xFFFFFFFFFFFFFFF,
                                    BitConverter.ToUInt64(soundAliasBuffer, 80) & 0xFFFFFFFFFFFFFFF,
                                    BitConverter.ToUInt64(soundAliasBuffer, 96) & 0xFFFFFFFFFFFFFFF,
                                    BitConverter.ToUInt64(soundAliasBuffer, 112) & 0xFFFFFFFFFFFFFFF
                                    );
                            }
                        }
                    }

                    // Increment
                    soundBanksExported++;
                    soundAliasesExported += soundPool.AliasCount;
                }
            }
            catch { }

            // Done
            Printer.WriteLine("INFO", String.Format("Exported {0} Sound Aliases from {1} Sound Banks successfully", soundAliasesExported, soundBanksExported));
        }

        /// <summary>
        /// Gets a string from Black Ops 4's String Pool
        /// </summary>
        public static string GetString(int index)
        {
            // Check for 0 index, return empty string (which is essentially what string 0 is)
            if (index == 0)
                return "";

            // Read Data
            byte[] stringInfo = GameLoader.Reader.ReadBytes(StringTableAddress + (index * 16) + 16, 2);
            byte[] result = GameLoader.Reader.ReadBytes(StringTableAddress + (index * 16) + 18, stringInfo[1] - 1);

            // Set Key
            byte xorKey = stringInfo[0];

            switch (xorKey)
            {
                case 165:
                    for (int x = 0; x < result.Length; x++, xorKey--) result[x] = Decrypt(result[x], xorKey);
                    break;
                case 175:
                    for (int x = 0; x < result.Length; x++, xorKey++) result[x] = Decrypt(result[x], xorKey);
                    break;
                case 185:
                    for (int x = 0; x < result.Length; x++, xorKey -= (byte)(x - 1 + 1)) result[x] = Decrypt(result[x], xorKey);
                    break;
                case 189:
                    for (int x = 0; x < result.Length; x++, xorKey += (byte)(x - 1 + 1)) result[x] = Decrypt(result[x], xorKey); ;
                    break;
            }

            // Return it as string
            return Encoding.ASCII.GetString(result);
        }
    }
}
