using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gorilla_Gang_Wars.Networking_Core;

public class GorillaGangMember : MonoBehaviour
{
    private static readonly List<GorillaGangMember>          _gangMembers = new();
    public static           IReadOnlyList<GorillaGangMember> GangMembers => _gangMembers;

    public static GorillaGangMember LocalGangMember =>
            GangMembers.FirstOrDefault(gangMember => gangMember.AssociatedRig.isLocal);

    public bool IsMaster { get; private set; }

    public VRRig AssociatedRig { get; private set; }

    private void Awake() => AssociatedRig = GetComponent<VRRig>();

    private void OnEnable()  => _gangMembers.Add(this);
    private void OnDisable() => _gangMembers.Remove(this);

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