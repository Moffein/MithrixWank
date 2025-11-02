using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MithrixWank.Modules
{
    public static class Assets
    {
        public static AssetBundle mainAssetBundle;
        internal static void LoadAssetBundle()
        {
            if (mainAssetBundle == null)
            {
                mainAssetBundle = AssetBundle.LoadFromFile(Files.GetPathToFile("AssetBundles", "mithrixwankassetbundle"));
            }
        }
    }
}
