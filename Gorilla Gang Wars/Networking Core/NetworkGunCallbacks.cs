using System.Collections.Generic;
using Gorilla_Gang_Wars.Types;
using UnityEngine;

namespace Gorilla_Gang_Wars.Networking_Core;

public class NetworkGunCallbacks : MonoBehaviour
{
    public static readonly List<NetworkGunCallbacks> RegisteredCallbacks = [];

    /// <summary>
    ///     A must-run method to register your callbacks.
    /// </summary>
    /// <param name="ngc">
    ///     Preferably just passing in 'this'
    /// </param>
    private void RegisterNetworkGunCallback(NetworkGunCallbacks ngc)
    {
        if (!RegisteredCallbacks.Contains(ngc))
            RegisteredCallbacks.Add(ngc);
    }

    /// <summary>
    ///     A must-run method to deregister your callbacks.
    /// </summary>
    /// <param name="ngc">
    ///     Preferably just passing in 'this'
    /// </param>
    private void DeregisterNetworkGunCallback(NetworkGunCallbacks ngc)
    {
        if (RegisteredCallbacks.Contains(ngc))
            RegisteredCallbacks.Remove(ngc);
    }

    public virtual void OnShot(VRRig             shooter, VRRig shot, GunType gunType, float distance) { }
    public virtual void OnMasterTransition(float timeSinceLastGunSpawn) { }
    public virtual void OnGunSpawnRequested(Vector3 gunPosition, Quaternion gunRotation, GunType gun) { }
}