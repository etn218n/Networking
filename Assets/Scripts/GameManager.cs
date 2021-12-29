using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public  static GameManager Instance => instance;
    
    private uint ticks;
    public  uint Ticks => ticks;
    
    private List<IEntity> entities = new List<IEntity>();
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        
        instance = this;

        Physics.autoSimulation = false;

        DontDestroyOnLoad(this.gameObject);
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

        foreach (var entity in entities)
        {
            entity.OnPreFixedUpdate();
            entity.OnFixedUpdate(Time.fixedDeltaTime);
        }
        
        Physics.Simulate(Time.fixedDeltaTime);

        foreach (var entity in entities)
        {
            entity.OnPostFixedUpdate();
        }
    }

    public void Subscribe(IEntity entity) => entities.Add(entity);
    public void Unsubscribe(IEntity entity) => entities.Remove(entity);
}
