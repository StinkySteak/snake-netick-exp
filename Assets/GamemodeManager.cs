using Netick.Unity;
using UnityEngine;

public class GamemodeManager : NetworkEventsListener
{
    public NetworkObject PlayerPrefab;

    public override void OnPlayerConnected(NetworkSandbox sandbox, Netick.NetworkPlayer player)
    {
        if (!Sandbox.IsServer) return;

        Sandbox.NetworkInstantiate(PlayerPrefab, Vector3.zero, Quaternion.identity, player);
    }
}
