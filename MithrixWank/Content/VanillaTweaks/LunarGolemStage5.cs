using HG;
using MithrixWank.Content.Common;
using R2API;
using RoR2;
using RoR2.Navigation;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MithrixWank.Content.VanillaTweaks
{
    public class LunarGolemStage5 : ContentBase<LunarGolemStage5>
    {
        public static GameObject LunarGolemStage5BodyObject;
        public static GameObject LunarGolemStage5MasterObject;
        public static CharacterSpawnCard LunarGolemStage5SpawnCard;
        public static DirectorCard lunarGolemStage5DirectorCard;

        public override string ConfigCategoryString => "Lunar Golems on Stage 5";

        public override string ConfigOptionName => "Enable Content";

        public override string ConfigDescriptionString => "Add Lunar Golems to Sky Meadow and Helminth Hatchery.";

        protected override void Setup()
        {
            base.Setup();
            CreateStage5LunarGolem();
            AddToStages();
        }

        private void AddToStages()
        {
            string[] toAdd = new string[]
            {
                "RoR2/Base/skymeadow/dccsSkyMeadowMonsters.asset",
                "RoR2/DLC1/itskymeadow/dccsITSkyMeadowMonsters.asset",
                "RoR2/DLC2/helminthroost/dccsHelminthRoostMonstersDLC2Only.asset"
            };

            foreach (string path in toAdd)
            {
                DirectorCardCategorySelection dccs = Addressables.LoadAssetAsync<DirectorCardCategorySelection>(path).WaitForCompletion();
                PluginUtils.AddMonsterDirectorCardToCategory(dccs, lunarGolemStage5DirectorCard, PluginUtils.MonsterCategories.Minibosses);
            }
        }

        private void CreateStage5LunarGolem()
        {
            LunarGolemStage5BodyObject = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarGolem/LunarGolemBody.prefab").WaitForCompletion().InstantiateClone("MithrixWank_LunarGolemStage5", true);
            DeathRewards dr = LunarGolemStage5BodyObject.GetComponent<DeathRewards>();
            dr.logUnlockableDef = null;
            Modules.PluginContentPack.bodyPrefabs.Add(LunarGolemStage5BodyObject);
            ModifyStats(LunarGolemStage5BodyObject);
            LunarGolemStage5BodyObject.AddComponent<SwapToLunarTeamComponent>();

            LunarGolemStage5MasterObject = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarGolem/LunarGolemMaster.prefab").WaitForCompletion().InstantiateClone("MithrixWank_LunarGolemStage5", true);
            LunarGolemStage5MasterObject.GetComponent<CharacterMaster>().bodyPrefab = LunarGolemStage5BodyObject;
            Modules.PluginContentPack.masterPrefabs.Add(LunarGolemStage5MasterObject);

            SetupSpawnCard();
        }

        private void ModifyStats(GameObject go)
        {
            CharacterBody cb = go.GetComponent<CharacterBody>();

            cb.baseMaxHealth = 1000f;    //Vanilla is 1615
            cb.levelMaxHealth = cb.baseMaxHealth * 0.3f;
        }

        private void SetupSpawnCard()
        {
            LunarGolemStage5SpawnCard = ScriptableObject.CreateInstance<CharacterSpawnCard>();
            LunarGolemStage5SpawnCard.name = "cscMithrixWankLunarGolemStage5";
            LunarGolemStage5SpawnCard.prefab = LunarGolemStage5MasterObject;
            LunarGolemStage5SpawnCard.sendOverNetwork = true;
            LunarGolemStage5SpawnCard.hullSize = HullClassification.Golem;
            LunarGolemStage5SpawnCard.nodeGraphType = MapNodeGroup.GraphType.Ground;
            LunarGolemStage5SpawnCard.requiredFlags = NodeFlags.None;
            LunarGolemStage5SpawnCard.forbiddenFlags = NodeFlags.NoCharacterSpawn;
            LunarGolemStage5SpawnCard.directorCreditCost = 115;    //115 is Elder Lemurian
            LunarGolemStage5SpawnCard.occupyPosition = false;
            LunarGolemStage5SpawnCard.loadout = new SerializableLoadout();
            LunarGolemStage5SpawnCard.noElites = false;
            LunarGolemStage5SpawnCard.forbiddenAsBoss = true;

            lunarGolemStage5DirectorCard = PluginUtils.BuildDirectorCard(LunarGolemStage5SpawnCard);
        }
    }
}
