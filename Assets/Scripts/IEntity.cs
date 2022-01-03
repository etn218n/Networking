public interface IEntity
{
    void OnDebug();
    void OnUpdate(float deltaTime);
    void OnFixedUpdate(float fixedDeltaTime);
    void OnPostFixedUpdate(float fixedDeltaTime);
}