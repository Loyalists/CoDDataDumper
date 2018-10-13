﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoDDataDumper
{
    /// <summary>
    /// Game DB Data
    /// </summary>
    public struct DBGameInfo
    {
        /// <summary>
        /// Pointer to the Asset Pools
        /// </summary>
        public long AssetPoolAddress { get; set; }

        /// <summary>
        /// Pointer to the Asset Pool Sizes
        /// </summary>
        public long AssetPoolSizesAddress { get; set; }

        /// <summary>
        /// Pointer to the String Table
        /// </summary>
        public long StringTableAddress { get; set; }

        /// <summary>
        /// Initializes DBGameInfo
        /// </summary>
        /// <param name="assetPoolPtr">Pool Pointer</param>
        /// <param name="assetPoolSizesPtr">Sizes Pointer</param>
        /// <param name="strTablePtr">String Table Pointer</param>
        public DBGameInfo(long assetPoolPtr, long assetPoolSizesPtr, long strTablePtr)
        {
            AssetPoolAddress = assetPoolPtr;
            AssetPoolSizesAddress = assetPoolSizesPtr;
            StringTableAddress = strTablePtr;
        }
    }
}
