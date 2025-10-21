using ExitGames.Client.Photon;
using Gorilla_Gang_Wars.Types;
using Photon.Pun;
using Photon.Realtime;

namespace Gorilla_Gang_Wars.Networking_Core;

public static class PhotonDummy
{
    public static bool RaiseEvent(NetworkEvents eventId, object content, RaiseEventOptions raiseEventOptions,
                                  SendOptions   sendOptions) => PhotonNetwork.RaiseEvent((byte)eventId, content,
            raiseEventOptions, sendOptions);
}