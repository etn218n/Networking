public interface IEntity
{
    void OnDebug();
    void OnUpdate(float deltaTime);
    void OnPreFixedUpdate();
    void OnFixedUpdate(float fixedDeltaTime);
    void OnPostFixedUpdate();
}