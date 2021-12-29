using Mirror;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerSpawner : NetworkBehaviour
{
    [Header("Visual")]
    [SerializeField] private List<Material> colorMaterials;
    
    [Header("Spawn Settings")] 
    [Range(1f, 20f)]
    [SerializeField] private float spawnRadius;

    private NetworkEntity networkEntity;

    private void Awake()
    {
        networkEntity = GetComponent<NetworkEntity>();
    }

    public override void OnStartLocalPlayer()
    {
        CmdRequestColor();
        CmdRequestSpawnPosition();
    }

    [Command]
    private void CmdRequestSpawnPosition()
    {
        var randomPoint = Random.insideUnitCircle * spawnRadius;

        transform.position = new Vector3(randomPoint.x, transform.position.y, randomPoint.y);
    }

    [Command]
    private void CmdRequestColor()
    {
        if (colorMaterials.Count == 0)
            return;
        
        var index = (NetworkManager.singleton.numPlayers - 1) % colorMaterials.Count;
        
        RpcReceiveColor(colorMaterials[index].color);
    }

    [ClientRpc]
    private void RpcReceiveColor(Color color)
    {
        if (hasAuthority)
            networkEntity.CmdUpdateColor(color);
    }
}