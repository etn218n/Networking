using Mirror;
using UnityEngine;

public class PlayerController : NetworkEntity
{
    [Header("Stats")]
    [SerializeField] private float moveSpeed;

    private Vector3 desiredVelocity;
    private Rigidbody rigidBody;

    protected override void Awake()
    {
        base.Awake();
        
        rigidBody = GetComponent<Rigidbody>();
    }

    public override void OnUpdate(float deltaTime)
    {
        if (hasAuthority)
            ProcessInput();
    }

    public override void OnFixedUpdate(float fixedDeltaTime)
    {
        if (isServer)
            Move();
    }

    [Server]
    private void Move()
    {
        rigidBody.MovePosition(rigidBody.position + desiredVelocity * Time.fixedDeltaTime);
    }

    [Client]
    private void ProcessInput()
    {
        var x = Input.GetAxisRaw("Horizontal");
        var y = Input.GetAxisRaw("Vertical");

        CmdSendInputToServer(new Vector3(x, 0f, y));
    }

    [Command]
    private void CmdSendInputToServer(Vector3 inputVector)
    {
        desiredVelocity = inputVector.normalized * moveSpeed;
    }
}
