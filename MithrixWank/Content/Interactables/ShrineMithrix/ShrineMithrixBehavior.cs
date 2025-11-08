using HG;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace MithrixWank.Content.Interactables.ShrineMithrix
{
    [RequireComponent(typeof(PurchaseInteraction))]
    public class ShrineMithrixBehavior : MonoBehaviour
    {
        public CharacterSpawnCard lunarScavSpawncard = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/ScavLunar/cscScavLunar.asset").WaitForCompletion();
        public float refreshDuration = 2f;
        public Transform dropletOrigin;
        public GameObject rewardEffectPrefab;
        public Color rewardEffectColor;

        public float chanceGreen = 25f;
        public float chanceRed = 5f;
        public float chanceWhite = 70f;

        private Xoroshiro128Plus rng;
        private PurchaseInteraction purchaseInteraction;

        private bool waitingForRefresh = false;
        private float refreshTimer = 0f;

        //No access to Unity Events, so use this hacky workaround.
        private void PurchaseInteractionHack(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            //This is solely a hacky workaround for detecting when players lost beads
            bool isMithrixShrine = self == purchaseInteraction && purchaseInteraction != null;
            Inventory activatorInventory = null;
            int initialBeadCount = -1;
            if (isMithrixShrine)
            {
                CharacterBody body = activator.GetComponent<CharacterBody>();
                if (body && body.inventory)
                {
                    activatorInventory = body.inventory;
                    initialBeadCount = activatorInventory.GetItemCount(RoR2Content.Items.LunarTrinket);
                }
            }

            orig(self, activator);
            
            if (isMithrixShrine)
            {
                OnPurchaseServer();

                //Cursed code
                if (initialBeadCount > 0 && activatorInventory && initialBeadCount > activatorInventory.GetItemCount(RoR2Content.Items.LunarTrinket))
                {
                    SpawnTwistedScavServer();
                }
            }
        }

        private void Awake()
        {
            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteractionHack;
            purchaseInteraction = GetComponent<PurchaseInteraction>();
        }

        private void OnDestroy()
        {
            On.RoR2.PurchaseInteraction.OnInteractionBegin -= PurchaseInteractionHack;
        }

        private void Start()
        {
            if (NetworkServer.active)
            {
                rng = new Xoroshiro128Plus(Run.instance.treasureRng.nextUlong);
            }
        }

        private void FixedUpdate()
        {
            if (waitingForRefresh)
            {
                refreshTimer -= Time.fixedDeltaTime;
                if (refreshTimer <= 0f)
                {
                    purchaseInteraction.SetAvailable(true);
                    waitingForRefresh = false;
                }
            }
        }

        public void OnPurchaseServer()
        {
            if (!NetworkServer.active) return;
            purchaseInteraction.SetAvailable(false);
            waitingForRefresh = true;
            refreshTimer = refreshDuration;

            PickupIndex pickupIndex = PickupIndex.none;

            PickupDropTable dropTable;
            float random = rng.RangeFloat(0f, chanceWhite + chanceGreen + chanceRed);
            if (random <= chanceRed)
            {
                dropTable = Addressables.LoadAssetAsync<PickupDropTable>("RoR2/Base/Common/dtTier3Item.asset").WaitForCompletion();
            }
            else if (random <= chanceRed + chanceGreen)
            {
                dropTable = Addressables.LoadAssetAsync<PickupDropTable>("RoR2/Base/Common/dtTier2Item.asset").WaitForCompletion();
            }
            else
            {
                dropTable = Addressables.LoadAssetAsync<PickupDropTable>("RoR2/Base/Common/dtTier1Item.asset").WaitForCompletion();
            }
            pickupIndex = dropTable.GenerateDrop(rng);

            PickupDropletController.CreatePickupDroplet(pickupIndex, dropletOrigin.position, dropletOrigin.forward * 5f);
            EffectManager.SpawnEffect(rewardEffectPrefab, new EffectData { origin = transform.position, rotation = Quaternion.identity, scale = 1f, color = rewardEffectColor }, true);
        }

        public void SpawnTwistedScavServer()
        {
            if (!NetworkServer.active || !lunarScavSpawncard) return;

            DirectorSpawnRequest request = new DirectorSpawnRequest(lunarScavSpawncard,
                new DirectorPlacementRule()
                {
                    minDistance = 10,
                    maxDistance = 60,
                    placementMode = DirectorPlacementRule.PlacementMode.Random,
                    position = transform.position,
                    preventOverhead = false
                }, rng)
            {
                ignoreTeamMemberLimit = true,
                teamIndexOverride = TeamIndex.Lunar,
                onSpawnedServer = SetSpecialScaling
            };
            if (DirectorCore.instance) DirectorCore.instance.TrySpawnObject(request);
        }

        private void SetSpecialScaling(SpawnCard.SpawnResult spawnResult)
        {
            if (!spawnResult.spawnedInstance) return;

            Inventory spawnInv = spawnResult.spawnedInstance.GetComponent<Inventory>();
            if (!spawnInv) return;

            //Calculate Special Scaling
            float healthFactor = 1f;
            float damageFactor = 1f;
            healthFactor += Run.instance.difficultyCoefficient / 2.5f;
            damageFactor += Run.instance.difficultyCoefficient / 30f;
            int playerFactor = Mathf.Max(1, Run.instance.livingPlayerCount);
            healthFactor *= Mathf.Pow((float)playerFactor, 0.5f);

            int healthBoostCount = Mathf.FloorToInt((healthFactor - 1f) * 10);
            if (healthBoostCount > 0) spawnInv.GiveItem(RoR2Content.Items.BoostHp, healthBoostCount);

            int damageBoostCount = Mathf.FloorToInt((damageFactor - 1f) * 10);
            if (damageBoostCount > 0) spawnInv.GiveItem(RoR2Content.Items.BoostDamage, damageBoostCount);

            if (spawnInv.GetItemCount(RoR2Content.Items.UseAmbientLevel) <= 0) spawnInv.GiveItem(RoR2Content.Items.UseAmbientLevel);
        }
    }
}
