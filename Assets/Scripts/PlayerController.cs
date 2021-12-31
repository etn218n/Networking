using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : NetworkEntity
{
    public const int MaxBufferSize = 100;
    
    [Header("Stats")]
    [SerializeField] 
    private float moveSpeed;

    private List<InputState>  clientInputStateBuffer;
    private List<EntityState> clientLocalEntityStateBuffer;

    private Queue<EntityState> clientReceivedEntityStateBuffer;
    private Queue<InputState>  serverInputStateBuffer;

    private InputState? nextInputStateFromClient;
    
    private Vector3 inputVector;
    private Rigidbody rigidBody;

    protected override void Awake()
    {
        base.Awake();
        
        rigidBody = GetComponent<Rigidbody>();
        
        clientInputStateBuffer          = new List<InputState>(MaxBufferSize);
        clientLocalEntityStateBuffer    = new List<EntityState>(MaxBufferSize);
        
        clientReceivedEntityStateBuffer = new Queue<EntityState>(MaxBufferSize);
        serverInputStateBuffer          = new Queue<InputState>(MaxBufferSize);
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
            if (clientReceivedEntityStateBuffer.Any())
                RollbackState(clientReceivedEntityStateBuffer.Dequeue());

            var inputState = new InputState { Ticks = GameManager.Instance.Ticks, MoveVector = inputVector };

            clientInputStateBuffer.Add(inputState);

            CmdSendInputStateToServer(inputState);

            Move(inputVector.normalized * moveSpeed, fixedDeltaTime);
        }

        if (isServer && serverInputStateBuffer.Any())
        {
            nextInputStateFromClient = serverInputStateBuffer.Dequeue();

            Move(nextInputStateFromClient.Value.MoveVector.normalized * moveSpeed, fixedDeltaTime);
        }
    }

    public override void OnPostFixedUpdate()
    {
        if (isLocalPlayer)
        {
            var entityState = new EntityState
            {
                Ticks           = GameManager.Instance.Ticks,
                Position        = rigidBody.position,
                LinearVelocity  = rigidBody.velocity,
                AngularVelocity = rigidBody.angularVelocity,
                Orientation     = rigidBody.rotation
            };
        
            clientLocalEntityStateBuffer.Add(entityState);
        }
        
        if (isServer && nextInputStateFromClient.HasValue)
        {
            var entityState = new EntityState
            {
                Ticks           = nextInputStateFromClient.Value.Ticks,
                Position        = rigidBody.position,
                LinearVelocity  = rigidBody.velocity,
                AngularVelocity = rigidBody.angularVelocity,
                Orientation     = rigidBody.rotation
            };
        
            TargetSendEntityStateToClient(entityState);

            nextInputStateFromClient = null;
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
    
    [Client]
    private void RollbackState(EntityState serverEntityState)
    {
        var index = clientInputStateBuffer.FindIndex(input => input.Ticks == serverEntityState.Ticks);

        if (index != -1)
        {
            if (Vector3.Distance(clientLocalEntityStateBuffer[index].Position, serverEntityState.Position) < 0.00001f)
            {
                clientInputStateBuffer.RemoveRange(0, index + 1);
                clientLocalEntityStateBuffer.RemoveRange(0, index + 1);
                return;
            }

            clientInputStateBuffer.RemoveRange(0, index + 1);
            clientLocalEntityStateBuffer.RemoveRange(0, index + 1);
        }
        
        var numberOfCorrection = 1;

        rigidBody.position        = serverEntityState.Position;
        rigidBody.rotation        = serverEntityState.Orientation;
        rigidBody.velocity        = serverEntityState.LinearVelocity;
        rigidBody.angularVelocity = serverEntityState.AngularVelocity;
        
        Physics.SyncTransforms();

        foreach (var inputState in clientInputStateBuffer)
        {
            Move(inputState.MoveVector.normalized * moveSpeed, Time.fixedDeltaTime);
            
            Physics.Simulate(Time.fixedDeltaTime);

            numberOfCorrection++;
        }
        
        Debug.Log($"{gameObject.name} performed {numberOfCorrection} correction steps.");
    }

    [TargetRpc]
    private void TargetSendEntityStateToClient(EntityState entityState)
    {
        if (!clientReceivedEntityStateBuffer.Any())
        {
            clientReceivedEntityStateBuffer.Enqueue(entityState);
            return;
        }
        
        if (entityState.Ticks > clientReceivedEntityStateBuffer.Last().Ticks)
            clientReceivedEntityStateBuffer.Enqueue(entityState);
    }
    
    [Command]
    private void CmdSendInputStateToServer(InputState inputState)
    {
        if (!serverInputStateBuffer.Any())
        {
            serverInputStateBuffer.Enqueue(inputState);
            return;
        }
        
        if (inputState.Ticks > serverInputStateBuffer.Last().Ticks)
            serverInputStateBuffer.Enqueue(inputState);
    }
}
