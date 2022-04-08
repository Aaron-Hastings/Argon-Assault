using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls2 : MonoBehaviour
{

    [SerializeField] [Range(1, 3)] int method = 1;
    [SerializeField] float controlSpeed = 30.0f;
    [SerializeField] float xRange = 10.0f;
    [SerializeField] float yRange = 6.0f;
    [SerializeField] float globalSlopeFactor = -1f;
    [SerializeField] float localPositionPitchFactor = -0.2f;
    [SerializeField] float localPositionYawFactor = -0.2f;
    [SerializeField] float globalYawFactor = 0.3f;
    [SerializeField] float yawOffset = -15f;
    [SerializeField] float throwPitchFactor = -30.0f;
    [SerializeField] float throwRollFactor = 30f;

    float xPositionOld, yPositionOld, zPositionOld;
    float dX, dY, dZ;
    float xThrow, yThrow; // Value of user inputs, range from -1 to 1, can have intermittent values due to "sensitivity setting" 
    float pitch, yaw, roll;
    float pitchOld, yawOld;

    void Update()
    {
        ProcessLocalTranslation();
        ProcessRotation();
    }

    void ProcessLocalTranslation()
    {
        xThrow = Input.GetAxis("Horizontal");
        yThrow = Input.GetAxis("Vertical");

        float rawXPos = transform.localPosition.x + controlSpeed * xThrow * Time.deltaTime;
        float rawYPos = transform.localPosition.y - controlSpeed * yThrow * Time.deltaTime;
        float clampedXPos = Mathf.Clamp(rawXPos, -xRange, xRange);
        float clampedYPos = Mathf.Clamp(rawYPos, -yRange, yRange);

        transform.localPosition = new Vector3(clampedXPos, clampedYPos, transform.localPosition.z);
    }

    void ProcessRotation()
    {
        ComputeDXDYDZ();
        switch (method)
        {
            case 1:
                // This is full effect gamedev uses (i.e. does not use global coords or slope at all)
                PitchFromLocalPosition();
                AddThrowAffectToPitch();
                YawFromLocalPosition();
                RollFromXThrow();
                transform.localRotation = Quaternion.Euler(pitch, yaw, roll);
                break;
            case 2:
                // This is closest to physics, but lags
                PitchFromGlobalSlope();
                YawFromGlobalDHorizontal();
                RollFromXThrow();
                transform.localRotation = Quaternion.Euler(pitch, yaw + yawOffset, roll);
                break;

            case 3:
                // This adds some user interaction to the physics option
                PitchFromGlobalSlope();
                AddThrowAffectToPitch();
                YawFromGlobalDHorizontal();
                RollFromXThrow();
                transform.localRotation = Quaternion.Euler(pitch, yaw + yawOffset, roll);
                break;
            default:
                break;
        }
    }

    void ComputeDXDYDZ()
    {
        dX = transform.position.x - xPositionOld;
        dY = transform.position.y - yPositionOld;
        dZ = transform.position.z - zPositionOld;

        yPositionOld = transform.position.y; // Store this to compute dy
        xPositionOld = transform.position.x; // Store this to compute dx
        zPositionOld = transform.position.z; // Store this to compute dz
    }

    void PitchFromGlobalSlope()
    {
        // Pitch based on Global Coordinates
        float dVertical = dY;
        float dHorizontal = Mathf.Sqrt(dX * dX + dZ * dZ);

        if (dHorizontal == 0) // If the divisor is 0, we need to manually specify pitch
        {
            if (dVertical == 0) // if there is no vertical change, set pitch to 0
            {
                pitch = 0;
            }
            else // Otherwise set the pitch to the old pitch (best guess)
            {
                pitch = pitchOld;
            }
        }
        else // If the divisor is not 0, then we can set pitch equal to slope ( dVertical/dHorizontal ) 
        {
            pitch = globalSlopeFactor * (Mathf.Atan((dVertical) / Mathf.Abs(dHorizontal)) * 180 / Mathf.PI);
        }
        pitchOld = pitch; // Store this in case dx = 0 in later cases
    }

    void PitchFromLocalPosition()
    {
        // Has no relation to physics, but how GameDev did it (with AddThrowAffectToPitch)
        pitch = transform.localPosition.y * localPositionPitchFactor;
        pitchOld = pitch; // Generally should not be needed, included in case used later
    }

    void AddThrowAffectToPitch()
    {
        // Account for pitch based on Local Shift
        pitch = pitch - yThrow * throwPitchFactor; // yThrow adds more to pitch, i.e. the pilot is working the stick harder
        // Note, should not update pitchOld, since that should only reflect geometry
    }

    void YawFromGlobalDHorizontal()
    {
        float dHorizontal = Mathf.Sqrt(dX * dX + dZ * dZ);

        if (dZ == 0) // If the divisor is 0, we need to manually specify yaw
        {
            yaw = yawOld;
        }
        else // If the divisor is not 0, then we can set yaw equal to slope ( dX/dZ ) 
        {
            yaw = globalYawFactor * (Mathf.Atan((dX) / Mathf.Abs(dZ)) * 180 / Mathf.PI);
        }
        yawOld = yaw; // Store this in case dx = 0 in later cases
    }
    
    void YawFromLocalPosition()
    {
        yaw = transform.localPosition.x * localPositionYawFactor;
    }

    void RollFromXThrow()
    {
        roll = xThrow * throwRollFactor;
    }
}
