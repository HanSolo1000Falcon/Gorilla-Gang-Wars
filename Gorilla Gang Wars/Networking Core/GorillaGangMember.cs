using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Gorilla_Gang_Wars.Types;
using GorillaLocomotion;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Gorilla_Gang_Wars.Networking_Core;

public class GorillaGangMember : MonoBehaviour
{
    private const float YPos = 41f;

    private const float FirstXPos = -72f;
    private const float FirstZPos = -46.5f;

    private const float SecondXPos = -37f;
    private const float SecondZPos = -80f;

    public const            float                   GunSpawnCooldown = 15f;
    private static readonly List<GorillaGangMember> _gangMembers     = [];

    private       float                            lastGunSpawnTime;
    public static IReadOnlyList<GorillaGangMember> GangMembers => _gangMembers;

    public static GorillaGangMember LocalGangMember =>
            GangMembers.FirstOrDefault(gangMember => gangMember.AssociatedRig.isLocal);

    public bool IsMaster { get; private set; }

    public VRRig AssociatedRig { get; private set; }

    private void Awake() => AssociatedRig = GetComponent<VRRig>();

    private void Update()
    {
        HandleGunSpawning();
    }

    private void HandleGunSpawning()
    {
        if (!AssociatedRig.isLocal || !IsMaster)
            return;

        if (Time.time - lastGunSpawnTime < GunSpawnCooldown)
            return;

        lastGunSpawnTime = Time.time;
        while (true)
        {
            float   xPos     = Random.Range(FirstXPos, SecondXPos);
            float   zPos     = Random.Range(FirstZPos, SecondZPos);
            Vector3 spawnPos = new(xPos, YPos, zPos);

            if (!Physics.Raycast(spawnPos, Vector3.down, out RaycastHit hit, 100f,
                        GTPlayer.Instance.locomotionEnabledLayers))
                continue;

            GunType    gun      = GetRandomGun();
            Quaternion rotation = Quaternion.LookRotation(hit.normal);
            Vector3    position = hit.point;
            PhotonNetwork.RaiseEvent((byte)NetworkEvents.SpawnGunEvent,
                    new object[] { position, rotation, gun, },
                    new RaiseEventOptions { Receivers = ReceiverGroup.All, }, SendOptions.SendReliable);

            break;
        }
    }

    private void OnEnable()
    {
        _gangMembers.Add(this);
        foreach (GorillaGangMember gangMember in GangMembers)
            gangMember.PerformMasterCalculations();
    }
    
    private void OnDisable()
    {
        _gangMembers.Remove(this);
        foreach (GorillaGangMember gangMember in GangMembers)
            gangMember.PerformMasterCalculations();
    }


    private GunType GetRandomGun() =>
            GunType.Glock19; // temporary when testing, will add actual weighted assignments sooner or later

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