using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Gorilla_Gang_Wars.Types;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Gorilla_Gang_Wars.Networking_Core;

public class GorillaGangMember : MonoBehaviour
{
    public const           float                   GunSpawnCooldown = 15f;
    private static readonly List<GorillaGangMember> _gangMembers = new();

    private       float                            lastGunSpawnTime;
    public static IReadOnlyList<GorillaGangMember> GangMembers => _gangMembers;

    public static GorillaGangMember LocalGangMember =>
            GangMembers.FirstOrDefault(gangMember => gangMember.AssociatedRig.isLocal);

    public bool IsMaster { get; private set; }

    public VRRig AssociatedRig { get; private set; }

    private void Awake() => AssociatedRig = GetComponent<VRRig>();

    private void Update()
    {
        if (!AssociatedRig.isLocal || !IsMaster)
            return;

        if (Time.time - lastGunSpawnTime > GunSpawnCooldown)
        {
            lastGunSpawnTime = Time.time;
            
        }
    }

    private void OnEnable()  => _gangMembers.Add(this);
    private void OnDisable() => _gangMembers.Remove(this);

    public void TransitionMaster(int newMasterActorNumber)
    {
        if (!AssociatedRig.isLocal || !IsMaster)
            return;

        RaiseEventOptions raiseEventOptions = new() { TargetActors = [newMasterActorNumber,], };
        object[]          data              = [lastGunSpawnTime,];
        PhotonNetwork.RaiseEvent((byte)NetworkEvents.MasterTransitionEvent, data, raiseEventOptions,
                SendOptions.SendReliable);
    }

    public void PerformMasterCalculations()
    {
        bool isLocalLowest = true;
        foreach (GorillaGangMember gangMember in GangMembers)
        {
            if (gangMember == this)
                continue;

            if (gangMember.AssociatedRig.OwningNetPlayer.ActorNumber < AssociatedRig.OwningNetPlayer.ActorNumber)
            {
                isLocalLowest = false;

                break;
            }
        }

        IsMaster = isLocalLowest;
    }
}