using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : NetworkEntity
{
    [Header("Stats")]
    [SerializeField] private float moveSpeed;

    public const int MaxInputBufferSize = 100;

    private List<InputState> inputBuffer = new List<InputState>(MaxInputBufferSize);
    private InputState latestInputState;
    private InputState previousProcessedInputState;

    private Vector3 inputVector;
    private Rigidbody rigidBody;

    protected override void Awake()
    {
        base.Awake();
        
        rigidBody = GetComponent<Rigidbody>();
    }

    public override void OnUpdate(float deltaTime)
    {
        if (isLocalPlayer)
            ProcessLocalInput();
    }

    public override void OnFixedUpdate(float fixedDeltaTime)
    {
        if (isLocalPlayer)
        {
            var inputState = new InputState { Ticks = GameManager.Instance.Ticks, MoveVector = inputVector };

            inputBuffer.Add(inputState);

            CmdSendInputStateToServer(inputState);
            
            var desiredVelocity = inputVector.normalized * moveSpeed;
            
            Move(desiredVelocity, fixedDeltaTime);
        }

        if (isServer)
        {
            var desiredVelocity = latestInputState.MoveVector.normalized * moveSpeed;
            
            Move(desiredVelocity, fixedDeltaTime);
        }
    }

    public override void OnPostFixedUpdate()
    {
        if (isServer && latestInputState.Ticks > previousProcessedInputState.Ticks)
        {
            var entityState = new EntityState
            {
                Ticks = latestInputState.Ticks,
                Position = rigidBody.position,
                LinearVelocity = rigidBody.velocity,
                AngularVelocity = rigidBody.angularVelocity,
                Orientation = rigidBody.rotation
            };
        
            RpcSendEntityStateToClient(entityState);

            previousProcessedInputState = latestInputState;
        }
    }

    private void Move(Vector3 desiredVelocity, float deltaTime)
    {
        rigidBody.MovePosition(rigidBody.position + desiredVelocity * deltaTime);
    }

    [Client]
    private void ProcessLocalInput()
    {
        var x = Input.GetAxisRaw("Horizontal");
        var y = Input.GetAxisRaw("Vertical");

        inputVector = new Vector3(x, 0f, y);
    }

    [ClientRpc]
    private void RpcSendEntityStateToClient(EntityState entityState)
    {
        var index = inputBuffer.FindIndex(input => input.Ticks == entityState.Ticks);
        
        if (index != -1)
            inputBuffer.RemoveRange(0, index + 1);

        rigidBody.position        = entityState.Position;
        rigidBody.rotation        = entityState.Orientation;
        rigidBody.velocity        = entityState.LinearVelocity;
        rigidBody.angularVelocity = entityState.AngularVelocity;
        
        Physics.SyncTransforms();

        foreach (var inputState in inputBuffer)
        {
            var desiredVelocity = inputState.MoveVector.normalized * moveSpeed;
            
            Move(desiredVelocity, Time.fixedDeltaTime);
            
            Physics.Simulate(Time.fixedDeltaTime);
        }
    }

    [Command]
    private void CmdSendInputStateToServer(InputState inputState)
    {
        if (inputState.Ticks < latestInputState.Ticks)
            return;

        latestInputState = inputState;
    }
}
