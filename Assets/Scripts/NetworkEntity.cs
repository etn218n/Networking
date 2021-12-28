using Mirror;
using System.Collections;
using UnityEngine;

public abstract class NetworkEntity : NetworkBehaviour, IEntity
{
    [Header("Debug Settings")]
    [SerializeField] protected Color correctSyncColor;
    [SerializeField] protected Color falseSyncColor;
    [SerializeField] protected Renderer mainRenderer;
    
    protected Color originalColor;
    
    protected virtual void Awake()
    {
        originalColor = mainRenderer.material.color;
    }
    
    protected virtual void OnEnable()
    {
        GameManager.Instance.Subscribe(this);
    }

    protected virtual void OnDisable()
    {
        GameManager.Instance.Unsubscribe(this);
    }

    public virtual void OnDebug()
    {
        if (Input.GetKeyDown(KeyCode.C))
            CmdCheckState();
    }

    [Command(requiresAuthority = false)]
    public void CmdCheckState()
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
    
    
    public virtual void OnUpdate(float deltaTime) { }
    public virtual void OnPreFixedUpdate() { }
    public virtual void OnFixedUpdate(float fixedDeltaTime) { }
    public virtual void OnPostFixedUpdate() { }
}