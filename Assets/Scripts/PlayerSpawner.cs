using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    [Header("Visual")]
    [SerializeField] private Renderer mainRenderer;
    [SerializeField] private List<Material> colorMaterials;
    
    [Header("Spawn Settings")] 
    [Range(1f, 20f)]
    [SerializeField] private float spawnRadius;

    [SyncVar(hook = nameof(OnColorIndexUpdated))] 
    private int currentColorIndex;

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

        currentColorIndex = index;
        mainRenderer.material = colorMaterials[index];
    }

    private void OnColorIndexUpdated(int oldIndex, int newIndex)
    {
        mainRenderer.material = colorMaterials[newIndex];
    }
}