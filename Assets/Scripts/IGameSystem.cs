using UnityEngine;
using System;
using Unity;

public interface IGameSystem
{
    public void Initialize(IGameEvents gameEvents);
}

