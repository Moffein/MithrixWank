using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MithrixWank.Modules
{
    public class PluginContentPack : IContentPackProvider
    {
        public static ContentPack content = new ContentPack();
        public static List<GameObject> networkedObjectPrefabs = new List<GameObject>();
        public static List<GameObject> bodyPrefabs = new List<GameObject>();
        public static List<GameObject> masterPrefabs = new List<GameObject>();
        public static List<BuffDef> buffDefs = new List<BuffDef>();
        public static List<EffectDef> effectDefs = new List<EffectDef>();
        public static List<NetworkSoundEventDef> networkSoundEventDefs = new List<NetworkSoundEventDef>();

        public string identifier => "MithrixWank.content";

        internal void Initialize()
        {
            ContentManager.collectContentPackProviders += ContentManager_collectContentPackProviders;
        }

        private void ContentManager_collectContentPackProviders(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(this);
        }

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(content, args.output);
            yield break;
        }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            content.networkedObjectPrefabs.Add(networkedObjectPrefabs.ToArray());
            content.bodyPrefabs.Add(bodyPrefabs.ToArray());
            content.masterPrefabs.Add(masterPrefabs.ToArray());
            content.buffDefs.Add(buffDefs.ToArray());
            content.effectDefs.Add(effectDefs.ToArray());
            content.networkSoundEventDefs.Add(networkSoundEventDefs.ToArray());
            yield break;
        }
    }
}
