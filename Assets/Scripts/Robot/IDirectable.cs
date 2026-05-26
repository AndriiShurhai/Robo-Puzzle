using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public interface IDirectable
{
    void SetDirection(Vector3 direction, Transform platform);
}