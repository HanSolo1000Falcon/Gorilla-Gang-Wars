using System.Collections.Generic;
using System.Linq;
using Gorilla_Gang_Wars.Tools;
using Gorilla_Gang_Wars.Types;
using UnityEngine;

namespace Gorilla_Gang_Wars.Networking_Core;

public class NetworkGunCallbacks : MonoBehaviour
{
    private float         lastTimeGunSpawned;
    private float         startOfGracePeriod;
    private bool          isGracePeriod;
    public  Dictionary<GameObject, GunType> SpawnedGuns   = [];
    
    public static NetworkGunCallbacks Instance { get; private set; }

    private void Update()
    {
        if (isGracePeriod && Time.time - startOfGracePeriod > 10f)
            isGracePeriod = false;
    }

    private void Awake()
    {
        Instance           = this;
        startOfGracePeriod = Time.time;
        isGracePeriod      = true;
    }

    public void OnShot(VRRig                shooter, VRRig shot, GunType gunType, float distance) { }
    public void OnMasterTransition(float    timeSinceLastGunSpawn)                            { }

    public void OnGunSpawnRequested(Vector3 gunPosition, Quaternion gunRotation, GunType gun)
    {
        if (!isGracePeriod && Time.time        - lastTimeGunSpawned <
            GorillaGangMember.GunSpawnCooldown - GorillaGangMember.GunSpawnCooldown / 4f)
        {
            GorillaGangMember.GangMembers.FirstOrDefault(gangMember => gangMember.IsMaster)?.AssociatedRig.RemoveGangMember(); // anti cheat thingy
            return;
        }
        
        lastTimeGunSpawned = Time.time;
        
        // temporary
        Debug.Log("spawning cube at the position " + gunPosition + " and the rotation " + gunRotation + ".");
        GameObject foo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        foo.transform.localScale = Vector3.one;
        foo.transform.position   = gunPosition;
        foo.transform.rotation   = gunRotation;

        if (foo.TryGetComponent(out Renderer rend))
        {
            rend.material.shader = Shader.Find("GorillaTag/UberShader");
            rend.material.color = Color.white;
        }
        
        SpawnedGuns.Add(foo, gun);
    }
}