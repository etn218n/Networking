using UnityEngine;

public struct EntityState
{
    public uint Ticks;
    public Vector3 Position;
    public Vector3 LinearVelocity;
    public Vector3 AngularVelocity;
    public Quaternion Orientation;
}