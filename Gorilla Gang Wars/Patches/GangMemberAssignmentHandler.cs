using System.Collections.Generic;
using System.Reflection;
using ExitGames.Client.Photon;
using Gorilla_Gang_Wars.Networking_Core;
using Gorilla_Gang_Wars.Tools;
using Gorilla_Gang_Wars.Types;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Color = System.Drawing.Color;

namespace Gorilla_Gang_Wars.Patches;

public static class GangMemberAssignmentHandler
{
    private static readonly List<VRRig> spawnedRigs = new();

    [HarmonyPatch(typeof(VRRig), "SetColor")]
    private static class VRRigSetColorPatch
    {
        private static void Postfix(VRRig __instance, Color color) // yuck, that spelling of colour is awful
        {
            if (__instance.isLocal || spawnedRigs.Contains(__instance))
                return;

            spawnedRigs.Add(__instance);

            if (__instance.OwningNetPlayer.GetPlayerRef().CustomProperties.ContainsKey(Constants.PluginName))
                __instance.AddGangMember();
        }
    }

    [HarmonyPatch]
    private static class VRRigCacheRemoveRigFromGorillaParentPatch
    {
        private static IEnumerable<MethodBase> TargetMethods() =>
                [AccessTools.Method("VRRigCache:RemoveRigFromGorillaParent"),];

        private static void Postfix(NetPlayer player, VRRig vrrig)
        {
            spawnedRigs.Remove(vrrig);
            
            if (vrrig.IsGangMember())
                vrrig.RemoveGangMember();
        }
    }
}