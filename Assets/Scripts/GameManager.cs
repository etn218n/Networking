using UnityEngine;
using System.Collections.Generic;
using Mirror;

public class GameManager : NetworkBehaviour
{
    private static GameManager instance;
    public  static GameManager Instance => instance;
    
    private uint ticks;
    public  uint Ticks => ticks;

    [SyncVar] 
    private uint serverTicks;
    public  uint ServerTicks => serverTicks;

    private List<IEntity> entities = new List<IEntity>();
    
    private void Awake()
    {
        instance = this;
        syncInterval = Time.fixedDeltaTime;

        Physics.autoSimulation = false;
    }

    private void Update()
    {
        foreach (var entity in entities)
        {
            entity.OnUpdate(Time.deltaTime);
            entity.OnDebug();
        }
    }

    private void FixedUpdate()
    {
        ticks++;

        if (isServer)
            serverTicks = ticks;

        foreach (var entity in entities)
            entity.OnFixedUpdate(Time.fixedDeltaTime);

        Physics.Simulate(Time.fixedDeltaTime);

        foreach (var entity in entities)
            entity.OnPostFixedUpdate(Time.fixedDeltaTime);
    }

    public void Subscribe(IEntity entity) => entities.Add(entity);
    public void Unsubscribe(IEntity entity) => entities.Remove(entity);
}
