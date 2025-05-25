using HarmonyLib;
using Photon.Pun;
using System.Reflection;

namespace SyncUpgrades.Patches
{
    [HarmonyPatch(typeof(ItemHealthPack))]
    internal static class ItemHealthPackPatch
    {
        [HarmonyPatch("UsedRPC")]
        [HarmonyPostfix]
        private static void UsedRPC_Postfix(ItemHealthPack __instance)
        {
            // Only host propagates the heal to everyone else
            if (!PhotonNetwork.IsMasterClient)
                return;

            // Reflection to get private fields
            var itemToggleField = AccessTools.Field(typeof(ItemHealthPack), "itemToggle");
            var playerTogglePhotonIDField = AccessTools.Field(typeof(ItemToggle), "playerTogglePhotonID");
            var itemToggle = itemToggleField.GetValue(__instance);
            int userViewId = (int)playerTogglePhotonIDField.GetValue(itemToggle);

            // Loop over all players and heal everyone except the user
            foreach (var avatar in SemiFunc.PlayerGetAll())
            {
                // Defensive null check for avatar and its photonView
                if (avatar == null)
                    continue;
                var photonView = avatar.photonView;
                if (photonView == null)
                    continue;

                if (photonView.ViewID != userViewId)
                {
                    avatar.playerHealth.HealOther(__instance.healAmount, true);
                }
            }
        }
    }
}
