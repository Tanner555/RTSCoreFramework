using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTSCoreFramework
{
    public interface IAllyMovable
    {
        void MoveAlly(Vector3 Direction, bool isFreeMoving);
    }
}