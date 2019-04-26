using UnityEngine;

//this is not the original file, but a replica. 

[RequireComponent(typeof(PlatformerMotor2D))]
public class PlatformerInput : MonoBehaviour
{
    [SerializeField]
    string XMovementAxis = "Horizontal";
    [SerializeField]
    string JumpAxis = "Jump";
    [SerializeField]
    string DashAxis = "Fire1";

    private PlatformerMotor2D motor;

    void Start()
    {
        motor = GetComponent<PlatformerMotor2D>();
    }

    void Update()
    {
        motor.normalizedXMovement = Input.GetAxis(XMovementAxis);
        if(Input.GetAxis(JumpAxis) > 0.0f)
        {
            motor.Jump();
        }
        if (Input.GetAxis(DashAxis) > 0.0f)
        {
            motor.Dash();
        }
    }
}
