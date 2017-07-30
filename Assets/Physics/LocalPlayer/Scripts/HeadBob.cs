using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class CurveControlledBob
{
    public float HorizontalBobRange = 0.33f;
    public float VerticalBobRange = 0.33f;
    public AnimationCurve Bobcurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f),
                                                        new Keyframe(1f, 0f), new Keyframe(1.5f, -1f),
                                                        new Keyframe(2f, 0f)); // sin curve for head bob
    public float VerticaltoHorizontalRatio = 1f;

    private float m_CyclePositionX;
    private float m_CyclePositionY;
    private float m_BobBaseInterval;
    private Vector3 m_OriginalCameraPosition;
    private float m_Time;

    public void Setup(Camera camera, float bobBaseInterval)
    {
        m_BobBaseInterval = bobBaseInterval;
        m_OriginalCameraPosition = camera.transform.localPosition;

        // get the length of the curve in time
        m_Time = Bobcurve[Bobcurve.length - 1].time;
    }

    public Vector3 DoHeadBob(float speed)
    {
        float xPos = m_OriginalCameraPosition.x + (Bobcurve.Evaluate(m_CyclePositionX) * HorizontalBobRange);
        float yPos = m_OriginalCameraPosition.y + (Bobcurve.Evaluate(m_CyclePositionY) * VerticalBobRange);

        m_CyclePositionX += (speed * Time.deltaTime) / m_BobBaseInterval;
        m_CyclePositionY += ((speed * Time.deltaTime) / m_BobBaseInterval) * VerticaltoHorizontalRatio;

        if (m_CyclePositionX > m_Time)
        {
            m_CyclePositionX = m_CyclePositionX - m_Time;
        }
        if (m_CyclePositionY > m_Time)
        {
            m_CyclePositionY = m_CyclePositionY - m_Time;
        }

        return new Vector3(xPos, yPos, 0f);
    }
}

[Serializable]
public class LerpControlledBob
{
    public float bobDurationMin = 0.15f;
    public float bobDurationMax = 0.15f;
    public float bobAmountMin = 0.2f;
    public float bobAmountMax = 0.2f;

    public bool RandomizeSign = false;

    private float offset = 0.0f;

    public float GetOffset()
    {
        return this.offset;
    }

    public IEnumerator DoBobCycle()
    {
        float bobDuration = UnityEngine.Random.Range(bobDurationMin, bobDurationMax);
        float bobAmount = UnityEngine.Random.Range(bobAmountMin, bobAmountMax);
        if (RandomizeSign)
        {
            if (UnityEngine.Random.value >= 0.5f)
                bobAmount *= -1.0f;
        }

        float t = 0f;

        // make the camera move down slightly
        while (t < bobDuration)
        {
            this.offset = Mathf.Lerp(0f, bobAmount, t / bobDuration);
            t += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        // make it move back to neutral
        t = 0f;
        while (t < bobDuration)
        {
            this.offset = Mathf.Lerp(bobAmount, 0f, t / bobDuration);
            t += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        this.offset = 0.0f;
    }
}

public class HeadBob : MonoBehaviour
{
    public LocalPlayer localPlayer;
    public LerpControlledBob landingVerticalBob = new LerpControlledBob();
    public LerpControlledBob landingPitchBob = new LerpControlledBob();
    public LerpControlledBob landingRollBob = new LerpControlledBob();

    private Vector3 cameraPosition;
    private Quaternion cameraRotation;
    private bool oldOnGround;
    private Vector3 startJumpOrigin;

    private void Start()
    {
        this.cameraPosition = this.transform.localPosition;
        this.cameraRotation = this.transform.localRotation;
    }

    private void Update()
    {
        this.AnimateLanding();
    }

    private void AnimateLanding()
    {
        bool onGround = localPlayer.gameMovement.IsOnGround();
        if (onGround && !this.oldOnGround)
        {
            // TODO: add half of the capsule collider as offset or something
            if (this.startJumpOrigin.y - this.transform.position.y > -1.0f)
            {
                StartCoroutine(this.landingVerticalBob.DoBobCycle());
                StartCoroutine(this.landingPitchBob.DoBobCycle());
                StartCoroutine(this.landingRollBob.DoBobCycle());
            }
        }

        if (!onGround && this.oldOnGround)
            this.startJumpOrigin = this.transform.position;

        if (onGround)
        {
            // camer position
            Vector3 newCameraPosition = this.transform.localPosition;
            newCameraPosition.y = cameraPosition.y - this.landingVerticalBob.GetOffset();
            this.transform.localPosition = newCameraPosition;

            // camera rotation
            Quaternion newCameraRotation = this.transform.localRotation;
            newCameraRotation.x = this.cameraRotation.x - this.landingPitchBob.GetOffset();
            newCameraRotation.z = this.cameraRotation.z - this.landingRollBob.GetOffset();
            this.transform.localRotation = newCameraRotation;
        }
        else
        {
            this.transform.localPosition = cameraPosition;
            this.transform.localRotation = this.cameraRotation;

        }

        this.oldOnGround = onGround;
    }
}
