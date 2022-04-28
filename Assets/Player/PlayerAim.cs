using UnityEngine;
public enum AimDirection { UP, UP_DIAG, STRAIGHT, DOWN_DIAG, DOWN }
public class PlayerAim : MonoBehaviour
{
    private readonly float[] CLAMPED_ANGLES = new float[9] { 360, 315, 270, 225, 180, 135, 90, 45, 0 };
    public float AimAngle;

    private Transform player;
    private Camera cameraMain;

    private const float AIM_RADIUS = 0.75f;

    //TODO implement analog aim
    public bool IsAnalogAim = true;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        cameraMain = Camera.main;
    }

    void Update()
    {
        AlignCursorWithAim();
    }

    private void AlignCursorWithAim()
    {
        // Construct Aim Vector
        Vector3 aimVector = Input.mousePosition;
        aimVector.z = player.position.z - cameraMain.transform.position.z;
        aimVector = cameraMain.ScreenToWorldPoint(aimVector);
        aimVector = aimVector - player.position;
        // Get angle from Aim Vector
        AimAngle = Mathf.Atan2(aimVector.y, aimVector.x) * Mathf.Rad2Deg;
        if (AimAngle < 0.0f) AimAngle += 360.0f;
        AimAngle = clampAngle(AimAngle);
        // Apply rotation to aim sprite
        transform.localEulerAngles = new Vector3(0, 0, AimAngle);
        // Apply positioning off of radius
        float xPos = Mathf.Cos(Mathf.Deg2Rad * AimAngle) * AIM_RADIUS;
        float yPos = Mathf.Sin(Mathf.Deg2Rad * AimAngle) * AIM_RADIUS;
        transform.position = new Vector2(player.position.x + xPos, player.position.y + yPos);
    }

    private float clampAngle(float angle)
    {
        float clamp = angle;
        foreach (float clampedAngle in CLAMPED_ANGLES)
        {
            float min = Mathf.Clamp(clampedAngle - 45, 0, 360);
            float max = Mathf.Clamp(clampedAngle + 45, 0, 360);
            if (min <= angle && angle <= max)
            {
                clamp = clampedAngle;
            }
        }

        return clamp;
    }

    public Vector2 ToVector
    {
        get
        {
            Vector2 aimToVector = Vector2.zero;
            if (AimAngle == 90)
            {
                aimToVector = Vector2.up;
            }
            else if (AimAngle == 45 || AimAngle == 135)
            {
                aimToVector = new Vector2(1, 1);
            }
            else if (AimAngle == 0 || AimAngle == 180)
            {
                aimToVector = Vector2.right;
            }
            else if (AimAngle == 225 || AimAngle == 315)
            {
                aimToVector = new Vector2(1, -1);
            }
            else if (AimAngle == 270)
            {
                aimToVector = Vector2.down;
            }

            // Flip x axis of aim if player is not facing right
            if (!player.GetComponent<Player>().Controller.FacingRight)
            {
                aimToVector = new Vector2(aimToVector.x * -1, aimToVector.y);
            }

            return aimToVector;
        }
    }

    public AimDirection ToEnum
    {
        get
        {
            if (AimAngle == 90)
            {
                return AimDirection.UP;
            }
            else if (AimAngle == 45 || AimAngle == 135)
            {
                return AimDirection.UP_DIAG;
            }
            else if (AimAngle == 0 || AimAngle == 180)
            {
                return AimDirection.STRAIGHT;
            }
            else if (AimAngle == 225 || AimAngle == 315)
            {
                return AimDirection.DOWN_DIAG;
            }
            else if (AimAngle == 270)
            {
                return AimDirection.DOWN;
            }
            return AimDirection.STRAIGHT;
        }
    }
}