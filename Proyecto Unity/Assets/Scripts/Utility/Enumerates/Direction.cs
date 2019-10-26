using UnityEngine;

public enum Direction
{
    NorthEast, East, SouthEast, SouthWest, West, NorthWest
}

public static class DirectionExtensions
{
    static Quaternion[] rotations =
    {
        Quaternion.Euler(0f,30f,0f),
        Quaternion.Euler(0f,90f,0f),
        Quaternion.Euler(0f,150f,0f),
        Quaternion.Euler(0f,210f,0f),
        Quaternion.Euler(0f,270f,0f),
        Quaternion.Euler(0f,330f,0f),
    };

    public static Quaternion GetRotation(this Direction direction)
    {
        return rotations[(int)direction];
    }
}