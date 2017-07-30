using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputSystem
{
    public class Settings
    {
        public float sensitivityX = 3.5f;
        public float sensitivityY = 3.5f;

        public float movementScaleRun = 450.0f;
        public float movementScaleWalk = 130.0f;
    }

    public Settings settings = new Settings();

    public Quaternion GetViewAngleDelta(Transform character, Transform camera)
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Mouse X") * this.settings.sensitivityX,
            Input.GetAxisRaw("Mouse Y") * this.settings.sensitivityY);

        // TODO: additional processing (filtering, accel, etc)

        return new Quaternion(-input.y, input.x, 0.0f, 0.0f);
    }

    public Vector2 GetMovement()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Vertical"),
            Input.GetAxisRaw("Horizontal"));

        // TODO: additional processing
        if (Input.GetKey(KeyCode.LeftShift))
        {
            input.x *= this.settings.movementScaleWalk;
            input.y *= this.settings.movementScaleWalk;
        }
        else
        {
            input.x *= this.settings.movementScaleRun;
            input.y *= this.settings.movementScaleRun;
        }

        return input;
    }

    public int GetButtons()
    {
        int buttons = 0;

        if (Input.GetButton("Jump"))
            buttons |= UserCommand.IN_JUMP;

        //if (Input.GetButton("Duck"))
        //    buttons |= UserCommand.IN_DUCK;

        return buttons;
    }
}
