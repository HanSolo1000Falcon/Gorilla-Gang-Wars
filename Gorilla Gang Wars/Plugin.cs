using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using Gorilla_Gang_Wars.Networking_Core;
using HarmonyLib;
using UnityEngine;

namespace Gorilla_Gang_Wars;

[BepInPlugin(Constants.PluginGuid, Constants.PluginName, Constants.PluginVersion)]
public class Plugin : BaseUnityPlugin
{
    private readonly Harmony    harmony = new(Constants.PluginGuid);
    private          GameObject componentHolder;

    private bool hasModInitialized;

    private void Start()
    {
        GorillaTagger.OnPlayerSpawned(OnGameInitialized);
    }

    private void OnGameInitialized()
    {
        NetworkSystem.Instance.OnJoinedRoomEvent        += (Action)OnJoinedRoom;
        NetworkSystem.Instance.OnReturnedToSinglePlayer += (Action)OnReturnedToSinglePlayer;
    }

    private void OnJoinedRoom()
    {
        if (NetworkSystem.Instance.GameModeString.Contains("MODDED") && componentHolder == null)
            InitializeMod();
    }

    private void OnReturnedToSinglePlayer()
    {
        if (hasModInitialized)
            DeInitializeMod();
    }

    private void InitializeMod()
    {
        componentHolder = new GameObject("Gorilla Guns Centre Of Operations (GGCOF)");
        componentHolder.AddComponent<NetworkEventListener>();

        harmony.PatchAll();
        hasModInitialized = true;

        VRRig.LocalRig.AddComponent<GorillaGangMember>();
    }

    private void DeInitializeMod()
    {
        Destroy(componentHolder);

        harmony.UnpatchSelf();

        List<GorillaGangMember> gangalangs = GorillaGangMember.GangMembers.ToList();
        foreach (GorillaGangMember gangMember in gangalangs)
            Destroy(gangMember);

        hasModInitialized = false;
    }
}