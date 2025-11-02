using JetBrains.Annotations;
using R2API;
using RoR2;
using RoR2.Hologram;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace MithrixWank.Content.Interactables.ShrineMithrix
{
    public class ShrineMithrixContent : ContentBase<ShrineMithrixContent>
    {
        public static List<String> guaranteedStages = new List<String>
        {
            "itskymeadow"
        };

        public static List<String> forbiddenStages = new List<String>
        {

        };

        public static InteractableSpawnCard iscShrineMithrix;
        public static GameObject ShrineMithrixInteractable;

        public override string ConfigCategoryString => "Shrine of Mithrix";

        public override string ConfigOptionName => "Enable Content";

        public override string ConfigDescriptionString => "Enables the Shrine of Mithrix";

        protected override void Setup()
        {
            base.Setup();
            CreateShrinePrefab();
            AddToSpawnpool();
        }

        //Force spawn the shrine, ignoring the director
        private void AddToSpawnpool()
        {
            SceneDirector.onPrePopulateSceneServer += SceneDirector_onPrePopulateSceneServer;
        }

        private void SceneDirector_onPrePopulateSceneServer(SceneDirector sceneDirector)
        {
            bool shouldSpawn = false;

            SceneDef currentScene = SceneCatalog.GetSceneDefForCurrentScene();
            if (currentScene)
            {
                bool isForbidden = forbiddenStages.Contains(currentScene.baseSceneName);
                bool forceSpawn = guaranteedStages.Contains(currentScene.baseSceneName);
                bool isStageFive = currentScene.stageOrder == 5;

                shouldSpawn = forceSpawn || (isStageFive && !isForbidden);
            }

            if (shouldSpawn && sceneDirector.interactableCredit >= iscShrineMithrix.directorCreditCost)
            {
                sceneDirector.interactableCredit -= iscShrineMithrix.directorCreditCost;
                DirectorPlacementRule placementRule = new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Random
                };
                GameObject gameObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(iscShrineMithrix, placementRule, sceneDirector.rng));
            }
        }

        private void CreateSpawncard()
        {
            InteractableSpawnCard isc = ScriptableObject.CreateInstance<InteractableSpawnCard>();
            isc.prefab = ShrineMithrixInteractable;
            isc.sendOverNetwork = true;
            isc.hullSize = HullClassification.Human;//Copy TP card for this
            isc.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;
            isc.requiredFlags = RoR2.Navigation.NodeFlags.TeleporterOK;
            isc.occupyPosition = true;
            isc.directorCreditCost = 10;    //ShrineCleanse is 5
            isc.slightlyRandomizeOrientation = false;

            iscShrineMithrix = isc;
        }

        private void CreateShrinePrefab()
        {
            if (ShrineMithrixInteractable) return;
            GameObject prefab = Modules.Assets.mainAssetBundle.LoadAsset<GameObject>("ShrineMithrix");
            ChildLocator childLocator = prefab.GetComponent<ChildLocator>();

            Transform statueBase = childLocator.FindChild("StatueBase");
            Transform statueMoon = childLocator.FindChild("StatueMoon");
            Transform statuePlatform = childLocator.FindChild("StatuePlatform");
            Transform statueDebris = childLocator.FindChild("StatueDebris");
            GameObject symbol = childLocator.FindChild("Symbol").gameObject;

            //Statue Materials
            Material lunarShrineMaterial = Addressables.LoadAssetAsync<Material>("RoR2/Base/moon/matMoonBridge.mat").WaitForCompletion();
            statueBase.GetComponent<MeshRenderer>().material = lunarShrineMaterial;
            statueMoon.GetComponent<MeshRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/LunarExploder/matLunarExploderCore.mat").WaitForCompletion();
            statuePlatform.GetComponent<MeshRenderer>().material = lunarShrineMaterial;
            statueDebris.GetComponent<MeshRenderer>().material = lunarShrineMaterial;

            //Symbol Materials
            symbol.GetComponent<MeshRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/LunarWisp/matLunarWispBombChargeTrail.mat").WaitForCompletion();
            symbol.AddComponent<Billboard>();

            //Net Setup, since this is PrefabAPI we ignore contentPack
            prefab.AddComponent<NetworkIdentity>();
            PrefabAPI.RegisterNetworkPrefab(prefab);

            //ModelLocator
            ModelLocator modelLocator = prefab.AddComponent<ModelLocator>();
            modelLocator.modelTransform = childLocator.FindChild("Model");
            modelLocator.modelBaseTransform = childLocator.FindChild("Base");

            //Highlights
            Highlight highlight = prefab.AddComponent<Highlight>();
            highlight.targetRenderer = statueBase.GetComponent<Renderer>();

            //PurchaseInteraction
            PurchaseInteraction purchase = prefab.AddComponent<PurchaseInteraction>();
            purchase.costType = CostTypeIndex.LunarItemOrEquipment;
            purchase.displayNameToken = "MITHRIXWANK_SHRINE_MITHRIX_NAME";
            purchase.contextToken = "MITHRIXWANK_SHRINE_MITHRIX_CONTEXT";
            purchase.available = true;
            purchase.cost = 1;
            purchase.automaticallyScaleCostWithDifficulty = false;
            purchase.isShrine = true;
            purchase.shouldProximityHighlight = true;
            //TODO: Check on-purchase events

            //Hologram
            HologramProjector hologram = prefab.AddComponent<HologramProjector>();
            hologram.displayDistance = 30f; //15 is ShrineChance, but this shrine is way bigger
            hologram.hologramPivot = childLocator.FindChild("HologramPivot");

            prefab.AddComponent<GenericDisplayNameProvider>().displayToken = "MITHRIXWANK_SHRINE_MITHRIX_NAME";

            //Shrine Behavior
            ShrineMithrixBehavior shrine = prefab.AddComponent<ShrineMithrixBehavior>();
            shrine.refreshDuration = 2f;
            shrine.rewardEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/ShrineUseEffect.prefab").WaitForCompletion();
            shrine.rewardEffectColor = new Color32(114, 255, 236, 255);
            shrine.dropletOrigin = childLocator.FindChild("DropletOrigin");

            prefab.AddComponent<PurchaseAvailabilityIndicator>().indicatorObject = symbol;

            //DitherModel?

            GenericInspectInfoProvider inspectInfo = prefab.AddComponent<GenericInspectInfoProvider>();
            InspectDef inspectDef = ScriptableObject.CreateInstance<InspectDef>();
            inspectDef.Info = new RoR2.UI.InspectInfo();
            inspectDef.Info.Visual = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/ShrineIcon.png").WaitForCompletion();
            inspectDef.Info.TitleToken = "MITHRIXWANK_SHRINE_MITHRIX_NAME";
            inspectDef.Info.DescriptionToken = "MITHRIXWANK_SHRINE_MITHRIX_DESCRIPTION";
            inspectDef.Info.FlavorToken = "MITHRIXWANK_SHRINE_MITHRIX_FLAVOR";
            inspectInfo.InspectInfo = inspectDef;

            //EntityLocator, skip debris
            statueBase.gameObject.AddComponent<EntityLocator>().entity = prefab;
            statueMoon.gameObject.AddComponent<EntityLocator>().entity = prefab;
            statuePlatform.gameObject.AddComponent<EntityLocator>().entity = prefab;

            ShrineMithrixInteractable = prefab;
            CreateSpawncard();
        }
    }
}
