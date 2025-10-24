using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Gorilla_Gang_Wars.Tools;
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

    /// <summary>
    /// Can be the same as LocalGangMember, but is not guaranteed to be.
    /// </summary>
    // idfk why i added a summary but it feels proffesional ttype shit
    public static GorillaGangMember MasterGangMember => GangMembers.FirstOrDefault(gangMember => gangMember.IsMaster);
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
            PhotonDummy.RaiseEvent(NetworkEvents.SpawnGunEvent,
                    new object[] { position, rotation, gun, },
                    new RaiseEventOptions { Receivers = ReceiverGroup.All, }, SendOptions.SendReliable);

            break;
        }
    }

    private GunType GetRandomGun() =>
            GunType.Glock19; // temporary when testing, will add actual weighted assignments sooner or later

    public void PerformMasterCalculations()
    {
        if (PhotonNetwork.MasterClient.IsGangMember())
        {
            GorillaGangMember masterGangMember = PhotonNetwork.MasterClient.GetRig().AssociatedGangMember();
            masterGangMember.IsMaster = true;

            foreach (GorillaGangMember gangMember in GangMembers)
            {
                if (gangMember == masterGangMember)
                    continue;

                gangMember.IsMaster = false;
            }
        }
        else
        {
            int lowestActorNumber = int.MaxValue;
            GorillaGangMember selectedMaster = null;
            foreach (GorillaGangMember gangMember in GangMembers)
            {
                if (gangMember.AssociatedRig.OwningNetPlayer.ActorNumber >= lowestActorNumber)
                    continue;

                lowestActorNumber = gangMember.AssociatedRig.OwningNetPlayer.ActorNumber;
                selectedMaster    = gangMember;
            }
            
            foreach (GorillaGangMember gangMember in GangMembers)
                gangMember.IsMaster = gangMember == selectedMaster;
        }
    }
}