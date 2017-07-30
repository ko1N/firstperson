using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserCommand
{
    public static int IN_JUMP = (1 << 1);
    public static int IN_DUCK = (1 << 2);

    public Quaternion viewAngles;
    public Vector3 movement;
    public int buttons;
}
