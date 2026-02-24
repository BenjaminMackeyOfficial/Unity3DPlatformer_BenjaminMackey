using Unity.Burst.CompilerServices;
using Unity.IO.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.InputSystem;

public class SkateboardControl : MonoBehaviour
{
    private GameObject skateBoard;
    private GameObject skateBoardBody;


    private Rigidbody boardRB;
    private Transform playerPositionOnBoard;
    [SerializeField] GameObject skateboardPrefab;
    [SerializeField] GameObject skateboardBodyPrefab;
    [SerializeField] float maxLandAngle;
    [SerializeField] float rotationSpeed;
    [SerializeField] float minumumControlSpeed;
    //contact info (like ground, not phone number)
    private Quaternion setAngleToo;

    //-------------------------------------------

    //board control
    private int groundedBuffer=0;
    public InputAction movement; //needs to be told by player controller
    private float allignMulti=0;
    private float userTurnReq=0;
    private float userLeanForwardReq=0;
    private float userForwardForce=0; //can be negative for backwards... but theres not a backwards booster

    //
    private bool setUp = false;
    public void SetUp()//called by player controller
    {
        if(setUp == true) return;
        if(skateboardPrefab == null)
        {
            setUp = false;
            return;
        }

        skateBoard = Instantiate(skateboardPrefab); 
        boardRB = skateBoard.GetComponent<Rigidbody>();

        skateBoardBody = Instantiate(skateboardBodyPrefab);

        playerPositionOnBoard = skateBoardBody.transform.Find("PlayerHolder");

        

        skateBoard.SetActive(false);
        skateBoardBody.SetActive(false);
        if(
            skateBoard != null &&
            boardRB != null &&
            skateBoardBody != null &&
            playerPositionOnBoard != null
        )
        { 
            setUp = true;
            skateBoard.GetComponent<SkateboardColllision>().skateboardControl = this;
        }
    }

    public void EnableBoard(quaternion initRot, Vector3 initSpeed)
    {
        skateBoard.SetActive(true);
        setAngleToo = initRot;
        boardRB.linearVelocity = Vector3.zero;
        boardRB.AddForce(initSpeed * 20, ForceMode.Impulse);
        skateBoard.transform.position = transform.position;
        
        skateBoardBody.SetActive(true);
    }

    public void DissableBoard()
    {
        skateBoard.SetActive(false);
        skateBoardBody.SetActive(false);
    }

    //ground info update
    public void UpdateGroundData(Collision collision)
    {
        groundedBuffer += 5;

        Vector3 norm = collision.contacts[0].normal;
        
        float up = Vector3.Angle(skateBoardBody.transform.up, norm);
        float down = Vector3.Angle(-skateBoardBody.transform.up, norm);
        float ang = Mathf.Min(up, down);


        Quaternion rot = Quaternion.AngleAxis(userTurnReq * 900f * Time.deltaTime, norm);
        Vector3 newFwd = rot * skateBoardBody.transform.forward;

        Vector3 right = Vector3.Cross(newFwd, norm).normalized;
        newFwd = Vector3.Cross(norm, right).normalized;

        setAngleToo = Quaternion.LookRotation(newFwd, norm);

        if(ang <= maxLandAngle)
        {
            
           
        }
        else
        {
            //fall off skateboard
        }
    }

   

    private void UpdateControlValues()
    {
        if(movement == null) return;
        Vector2 inputted = movement.ReadValue<Vector2>();

        userForwardForce = 0;
        userLeanForwardReq = 0;
        userTurnReq = 0;

        if(groundedBuffer > 0)userForwardForce = Mathf.Clamp(inputted.y, 0f,1f);
        else userLeanForwardReq = Mathf.Clamp(inputted.y, 0f,1f);
        
        userTurnReq = Mathf.Clamp(inputted.x, -1f,1f);
        
        allignMulti = Mathf.Clamp(boardRB.linearVelocity.sqrMagnitude / 1000f,0f,1f);
    }
    public void UpdateBoard()
    {

        UpdateControlValues();
        boardRB.AddForce(userForwardForce * skateBoardBody.transform.forward * 50, ForceMode.Force);

        Vector3 vel = boardRB.linearVelocity;
        skateBoardBody.transform.position = Vector3.SmoothDamp( skateBoardBody.transform.position, skateBoard.transform.position,ref vel, 0.9f * Time.deltaTime);

        Quaternion implied = Quaternion.Lerp(skateBoardBody.transform.rotation, setAngleToo, rotationSpeed * Time.deltaTime);
        skateBoardBody.transform.rotation = implied;

        

        transform.position = playerPositionOnBoard.position;
        transform.rotation = skateBoardBody.transform.rotation;
        int
        groundedBuffer =- 1;
        groundedBuffer = Mathf.Clamp(groundedBuffer, 0, 5);
    }
}
