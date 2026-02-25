using System;

[System.Flags]
public enum ActivatorType
{
    None = 0,
    TypeGrab = 1 << 0,
    TypeDrag = 1 << 1,
    Player = 1 << 2
}