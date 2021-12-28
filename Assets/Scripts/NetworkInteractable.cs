using Mirror;
using UnityEngine;
using System.Collections;

public class NetworkInteractable : NetworkBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private Color correctSyncColor;
    [SerializeField] private Color falseSyncColor;

    private Color originalColor;
    private Renderer mainRenderer;

    private void Awake()
    {
        mainRenderer  = GetComponent<Renderer>();
        originalColor = mainRenderer.material.color;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
            CmdCheckState();
    }

    [Command(requiresAuthority = false)]
    private void CmdCheckState()
    {
        var transformHash = HashTransform();

        RpcCheckState(transformHash);
    }

    [ClientRpc]
    private void RpcCheckState(string transformHash)
    {
        StopAllCoroutines();
        StartCoroutine(CheckState(transformHash));
    }

    private IEnumerator CheckState(string receivedHash)
    {
        var currentHash = HashTransform();

        mainRenderer.material.color = currentHash.Equals(receivedHash) ? correctSyncColor : falseSyncColor;

        yield return new WaitForSecondsRealtime(2f);

        mainRenderer.material.color = originalColor;
    }
    
    private string HashTransform()
    {
        var hash = new Hash128();
        
        hash.Append(transform.position.x.ToString("F3"));
        hash.Append(transform.position.y.ToString("F3"));
        hash.Append(transform.position.z.ToString("F3"));
        hash.Append(transform.rotation.x.ToString("F3"));
        hash.Append(transform.rotation.y.ToString("F3"));
        hash.Append(transform.rotation.z.ToString("F3"));
        hash.Append(transform.rotation.w.ToString("F3"));
            
        var hashString = hash.ToString();
            
        return hashString.Substring(hashString.Length - 5);
    }
}
