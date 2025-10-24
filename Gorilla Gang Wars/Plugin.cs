using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using ExitGames.Client.Photon;
using Gorilla_Gang_Wars.Networking_Core;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace Gorilla_Gang_Wars;

[BepInPlugin(Constants.PluginGuid, Constants.PluginName, Constants.PluginVersion)]
public class Plugin : BaseUnityPlugin
{
    public const string PlayerPropertiesKey = "Gorilla Gang Wars Player Properties";

    private readonly Harmony    harmony = new(Constants.PluginGuid);
    private          GameObject componentHolder;

    private bool hasModInitialized;

    private void Start()
    {
        GorillaTagger.OnPlayerSpawned(OnGameInitialized);
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { Constants.PluginName, true }, });
    }

    public static void UpdatePlayerProperties(int currentHealth)
    {
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
                { { PlayerPropertiesKey, $"Health;{currentHealth}" }, });
    }

    private void OnGameInitialized()
    {
        NetworkSystem.Instance.OnJoinedRoomEvent        += (Action)OnJoinedRoom;
        NetworkSystem.Instance.OnReturnedToSinglePlayer += (Action)OnReturnedToSinglePlayer;
    }

    private void OnJoinedRoom()
    {
        if (NetworkSystem.Instance.GameModeString.Contains("MODDED") && !hasModInitialized)
            InitializeMod();
    }

    private void OnReturnedToSinglePlayer()
    {
        if (hasModInitialized)
            DeInitializeMod();
    }

    private void InitializeMod()
    {
        componentHolder = new GameObject("Gorilla Gang Wars Centre Of Operations (GGWCOP)");
        componentHolder.AddComponent<NetworkEventListener>();
        componentHolder.AddComponent<NetworkGunCallbacks>();

        harmony.PatchAll();
        hasModInitialized = true;

        VRRig.LocalRig.AddComponent<GorillaGangMember>();
        UpdatePlayerProperties(100);
    }

    private void DeInitializeMod()
    {
        Destroy(componentHolder);

        harmony.UnpatchSelf();
        
        foreach (GameObject gun in NetworkGunCallbacks.Instance.SpawnedGuns.Keys)
            Destroy(gun);
        
        NetworkGunCallbacks.Instance.SpawnedGuns.Clear();

        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { PlayerPropertiesKey, null }, });
        hasModInitialized = false;
    }
}