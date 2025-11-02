using RoR2;
using UnityEngine.Networking;
using UnityEngine;
using RoR2.CharacterAI;

namespace MithrixWank.Content.Common
{
    public class SwapToLunarTeamComponent : MonoBehaviour
    {
        public void Start()
        {
            if (NetworkServer.active)
            {
                TeamComponent tc = GetComponent<TeamComponent>();
                if (tc && tc.teamIndex == TeamIndex.Monster)
                {
                    tc.teamIndex = TeamIndex.Lunar;
                }

                CharacterBody cb = GetComponent<CharacterBody>();
                if (cb)
                {
                    if (cb.master && cb.master.teamIndex == TeamIndex.Monster)
                    {
                        cb.master.teamIndex = TeamIndex.Lunar;
                        if (cb.master.aiComponents != null)
                        {
                            foreach (BaseAI ai in cb.master.aiComponents)
                            {
                                ai.UpdateTargets();
                            }
                        }
                    }
                }
            }

            Destroy(this);
        }
    }
}
