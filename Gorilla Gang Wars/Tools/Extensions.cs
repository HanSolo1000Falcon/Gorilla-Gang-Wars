using System.Collections.Generic;
using ExitGames.Client.Photon;
using Gorilla_Gang_Wars.Networking_Core;
using Gorilla_Gang_Wars.Types;
using Photon.Realtime;
using UnityEngine;

namespace Gorilla_Gang_Wars.Tools;

public static class Extensions
{
    private static List<string> gangMembers = [];
    private static Dictionary<VRRig, GorillaGangMember> gangMemberRigs = [];

    public static bool IsGangMember(this VRRig     rig)    => rig.OwningNetPlayer.IsGangMember();
    public static bool IsGangMember(this NetPlayer player) => player.GetPlayerRef().IsGangMember();
    public static bool IsGangMember(this Player    player) => gangMembers.Contains(player.UserId);
    
    public static GorillaGangMember AssociatedGangMember(this VRRig rig) => gangMemberRigs[rig];
    
    public static VRRig GetRig(this Player player) => GorillaParent.instance.vrrigs.Find(rig => rig.OwningNetPlayer.UserId == player.UserId);
    public static VRRig GetRig(this NetPlayer player) => GorillaParent.instance.vrrigs.Find(rig => rig.OwningNetPlayer.UserId == player.UserId);

    public static void AddGangMember(this VRRig rig)
    {
        gangMembers.Add(rig.OwningNetPlayer.UserId);
        gangMemberRigs[rig] = rig.AddComponent<GorillaGangMember>();

        foreach (GorillaGangMember gangMember in GorillaGangMember.GangMembers)
            gangMember.PerformMasterCalculations();
        
        // ReSharper disable once InvertIf
        if (GorillaGangMember.LocalGangMember.IsMaster)
        {
            RaiseEventOptions receiverGroup = new()
                    { TargetActors = [rig.OwningNetPlayer.ActorNumber,], };

            foreach (KeyValuePair<GameObject, GunType> spawnedGun in NetworkGunCallbacks.Instance.SpawnedGuns)
                PhotonDummy.RaiseEvent(NetworkEvents.SpawnGunEvent,
                        new object[] { spawnedGun.Key.transform.position, spawnedGun.Key.transform.rotation, spawnedGun.Value, }, receiverGroup,
                        SendOptions.SendReliable);
        }
    }

    public static void RemoveGangMember(this VRRig rig)
    {
        if (rig.TryGetComponent(out GorillaGangMember gangMember)) Object.Destroy(gangMember);
        if (gangMembers.Contains(rig.OwningNetPlayer.UserId)) gangMembers.Remove(rig.OwningNetPlayer.UserId);
        gangMemberRigs.Remove(rig);
        foreach (GorillaGangMember gangMember2 in GorillaGangMember.GangMembers)
            gangMember2.PerformMasterCalculations();
    }
}