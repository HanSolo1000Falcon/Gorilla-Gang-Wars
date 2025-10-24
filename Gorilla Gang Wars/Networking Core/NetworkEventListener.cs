using System;
using ExitGames.Client.Photon;
using Gorilla_Gang_Wars.Tools;
using Gorilla_Gang_Wars.Types;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Gorilla_Gang_Wars.Networking_Core;

public class NetworkEventListener : MonoBehaviour
{
    private void OnEnable()  => PhotonNetwork.NetworkingClient.EventReceived += OnEventReceived;
    private void OnDisable() => PhotonNetwork.NetworkingClient.EventReceived -= OnEventReceived;

    private void OnEventReceived(EventData eventData)
    {
        if (!Enum.IsDefined(typeof(NetworkEvents), eventData.Code))
            return;
        
        VRRig sender = GorillaParent.instance.vrrigs.Find(rig => rig.OwningNetPlayer.ActorNumber == eventData.Sender);
        if (sender == null || !sender.IsGangMember()) return;

        object[] data;
        if (eventData.Parameters.TryGetValue(ParameterCode.Data, out object rawData))
        {
            if (rawData is object[] rawDataArray)
            {
                data = rawDataArray;
            }
            else
            {
                Debug.LogError("Data is not an object array, what the hell?\nEvent code: " + eventData.Code);

                return;
            }
        }
        else
        {
            Debug.LogError("Data could not be retrieved from event, what the hell?");

            return;
        }

        NetworkEvents networkEvent = (NetworkEvents)eventData.Code;
        
        switch (networkEvent)
        {
            case NetworkEvents.ShootEvent:
                VRRig shooter = GorillaParent.instance.vrrigs.Find(rig => rig.OwningNetPlayer.ActorNumber == eventData.Sender);
                VRRig shot = GorillaParent.instance.vrrigs.Find(rig => rig.OwningNetPlayer.ActorNumber == (int)data[2]);
                NetworkGunCallbacks.Instance.OnShot(shooter, shot, (GunType)data[0], (float)data[1]);

                break;

            case NetworkEvents.SpawnGunEvent:
                if (!sender.AssociatedGangMember().IsMaster)
                {
                    Debug.Log("gng yous is not master :v:");
                    break;
                }
                
                Debug.Log("FUBBAKKY");
                Vector3    gunPosition = (Vector3)data[0];
                Quaternion gunRotation = (Quaternion)data[1];
                GunType    gun         = (GunType)data[2];
                NetworkGunCallbacks.Instance.OnGunSpawnRequested(gunPosition, gunRotation, gun);

                break;
        }
    }
}