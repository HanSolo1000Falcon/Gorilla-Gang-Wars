using ExitGames.Client.Photon;
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
        object[] data;
        if (eventData.Parameters.TryGetValue(ParameterCode.Data, out object rawData))
        {
            if (rawData is object[] rawDataArray)
            {
                data = rawDataArray;
            }
            else
            {
                Debug.LogError("Data is not an object array, what the hell?");

                return;
            }
        }
        else
        {
            Debug.LogError("Data could not be retrieved from event, what the hell?");

            return;
        }

        switch (eventData.Code)
        {
            case (byte)NetworkEvents.ShootEvent:
                VRRig shooter =
                        GorillaParent.instance.vrrigs.Find(rig => rig.OwningNetPlayer.ActorNumber == eventData.Sender);

                foreach (NetworkGunCallbacks ngc in NetworkGunCallbacks.RegisteredCallbacks)
                    ngc.OnShot(shooter, (GunType)data[0], (float)data[1]);

                break;
        }
    }
}