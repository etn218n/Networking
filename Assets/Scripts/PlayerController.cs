using Mirror;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float moveSpeed;

    private Vector3 desiredVelocity;
    private Rigidbody rigidBody;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (hasAuthority)
            ProcessInput();
    }

    private void FixedUpdate()
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
