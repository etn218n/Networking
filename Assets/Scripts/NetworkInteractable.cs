using System.Linq;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class NetworkInteractable : NetworkEntity
{
    private Rigidbody rigidBody;
    
    private List<EntityState>  clientLocalEntityStateBuffer;
    private Queue<EntityState> clientReceivedEntityStateBuffer;

    protected override void Awake()
    {
        base.Awake();

        rigidBody = GetComponent<Rigidbody>();

        clientLocalEntityStateBuffer    = new List<EntityState>(100);
        clientReceivedEntityStateBuffer = new Queue<EntityState>(100);
    }

    public override void OnFixedUpdate(float fixedDeltaTime)
    {
        if (!isClient || isServer)
            return;
    
        while (clientReceivedEntityStateBuffer.Any())
            Rollback(clientReceivedEntityStateBuffer.Dequeue());
    }
    
    public override void OnPostFixedUpdate(float fixedDeltaTime)
    {
        if (isClient)
        {
            var entityState = new EntityState
            {
                Ticks           = GameManager.Instance.ServerTicks + (uint)(NetworkTime.rtt * 0.5f / Time.fixedDeltaTime) + 2,
                Position        = rigidBody.position,
                LinearVelocity  = rigidBody.velocity,
                AngularVelocity = rigidBody.angularVelocity,
                Orientation     = rigidBody.rotation
            };

            clientLocalEntityStateBuffer.Add(entityState);
        }
        
        if (isServer)
        {
            var entityState = new EntityState
            {
                Ticks           = GameManager.Instance.ServerTicks,
                Position        = rigidBody.position,
                LinearVelocity  = rigidBody.velocity,
                AngularVelocity = rigidBody.angularVelocity,
                Orientation     = rigidBody.rotation
            };
    
            RpcSendEntityStateToClient(entityState);
        }
    }
    
    private void Rollback(EntityState serverEntityState)
    {
        var index = clientLocalEntityStateBuffer.FindIndex(state => state.Ticks == serverEntityState.Ticks);
        
        if (index == -1)
            return;

        var localEntityState = clientLocalEntityStateBuffer[index];
        
        clientLocalEntityStateBuffer.RemoveRange(0, index + 1);

        if (localEntityState.Position == serverEntityState.Position && localEntityState.Orientation == serverEntityState.Orientation)
            return;

        var numberOfCorrections = GameManager.Instance.ServerTicks - serverEntityState.Ticks + 1;

        rigidBody.position        = serverEntityState.Position;
        rigidBody.rotation        = serverEntityState.Orientation;
        rigidBody.velocity        = serverEntityState.LinearVelocity;
        rigidBody.angularVelocity = serverEntityState.AngularVelocity;

        for (int i = 0; i < numberOfCorrections; i++)
            Physics.Simulate(Time.fixedDeltaTime);

        Debug.Log($"{gameObject.name} performed {numberOfCorrections} correction steps.");
    }

    [ClientRpc]
    private void RpcSendEntityStateToClient(EntityState serverEntityState)
    {
        if (clientReceivedEntityStateBuffer.Any() && serverEntityState.Ticks <= clientReceivedEntityStateBuffer.Last().Ticks)
            return;
        
        clientReceivedEntityStateBuffer.Enqueue(serverEntityState);
    }
}
