using HG;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace MithrixWank.Content.Interactables.ShrineMithrix
{
    [RequireComponent(typeof(PurchaseInteraction))]
    public class ShrineMithrixBehavior : MonoBehaviour
    {
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
            orig(self, activator);
            
            if (self == purchaseInteraction && purchaseInteraction != null)
            {
                OnPurchaseServer();
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

            PickupDropletController.CreatePickupDroplet(pickupIndex, dropletOrigin.position, dropletOrigin.forward * 5f);
            EffectManager.SpawnEffect(rewardEffectPrefab, new EffectData { origin = transform.position, rotation = Quaternion.identity, scale = 1f, color = rewardEffectColor }, true);
        }
    }
}
