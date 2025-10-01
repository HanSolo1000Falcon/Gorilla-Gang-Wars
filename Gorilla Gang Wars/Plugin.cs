using System;
using BepInEx;
using Gorilla_Gang_Wars.Networking_Core;
using HarmonyLib;
using UnityEngine;

namespace Gorilla_Gang_Wars;

[BepInPlugin(Constants.PluginGuid, Constants.PluginName, Constants.PluginVersion)]
public class Plugin : BaseUnityPlugin
{
    private GameObject componentHolder;

    private void Start()
    {
        GorillaTagger.OnPlayerSpawned(OnGameInitialized);
        Harmony harmony = new(Constants.PluginGuid);
        harmony.PatchAll();
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
        if (componentHolder != null)
            Destroy(componentHolder);
    }

    private void InitializeMod()
    {
        componentHolder = new GameObject("Gorilla Guns Centre Of Operations (GGCOF)");
        componentHolder.AddComponent<NetworkEventListener>();
    }
}