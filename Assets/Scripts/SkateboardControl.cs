using Unity.Burst.CompilerServices;
using Unity.IO.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal.Internal;
using UnityEngine.Timeline;

public class SkateboardControl : MonoBehaviour
{
    private GameObject skateBoard;
    private GameObject skateBoardBody;
    private Rigidbody rb;
    public PlayerController playerController;
    public SkateboardVisuals visuals;
    private Rigidbody boardRB;
    private Rigidbody dirRB;
    private Transform playerPositionOnBoard;
    [SerializeField] GameObject skateboardPrefab;
    [SerializeField] GameObject skateboardBodyPrefab;
    [SerializeField] float maxLandAngle;
    [SerializeField] float rotationSpeed;
    [SerializeField] float pitchRotationSpeed;
    [SerializeField] float minumumControlSpeed;
    [SerializeField] float speed;
    //contact info (like ground, not phone number)
    private Quaternion setAngleToo;
    private Vector3 BoardUp;
    private Vector3 BoardForward;
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

    public void ResetBoard()
    {
        setUp = false;
        Destroy(skateBoard);
        Destroy(skateBoardBody);
    }
    public void PingDissable()
    {
        playerController.ToggleBoard();
    }
    public void SetUp()//called by player controller
    {
        
        if(setUp == true) return;
        if(skateboardPrefab == null)
        {
            setUp = false;
            return;
        }
        rb = GetComponent<Rigidbody>();
        skateBoard = Instantiate(skateboardPrefab); 
        boardRB = skateBoard.GetComponent<Rigidbody>();

        skateBoardBody = Instantiate(skateboardBodyPrefab);
        dirRB = skateBoardBody.GetComponent<Rigidbody>();
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
            visuals = skateBoardBody.GetComponent<SkateboardVisuals>();
            skateBoard.GetComponent<SkateboardColllision>().skateboardControl = this;
        }
    }

    public void EnableBoard(quaternion initRot, Vector3 initSpeed)
    {

        skateBoard.SetActive(true);
        setAngleToo = initRot;
        BoardUp = Vector3.up;
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
 
        if(ang <= maxLandAngle)
        {
            BoardUp = norm;

            Quaternion quat = Quaternion.FromToRotation(skateBoardBody.transform.up, BoardUp);
            BoardForward = quat * skateBoardBody.transform.forward;
        }
        else
        {
            ResetBoard();
            PingDissable();
            
            transform.position += Vector3.up * 2;
            //fall off skateboard
        }
    }
    private void rotateAdjust()
    {
        Vector3 newFwd = BoardForward;
        Vector3 newUp = BoardUp;

        
        Quaternion yawRot = Quaternion.AngleAxis(
            userTurnReq * rotationSpeed * Time.deltaTime,
            newUp
        );

        newFwd = yawRot * newFwd;
        newUp = yawRot * newUp;

        Debug.Log(groundedBuffer);
        setAngleToo = Quaternion.LookRotation(newFwd, newUp);
        if (groundedBuffer > 0)
        {
            return;
        }
        
        
        Vector3 right = Vector3.Cross(newUp, newFwd).normalized;


        Quaternion pitchRot = Quaternion.AngleAxis(
            userForwardForce * pitchRotationSpeed * Time.deltaTime,
            right
        );

        newFwd = pitchRot * newFwd;
        newUp = pitchRot * newUp;

        BoardForward = newFwd;
        BoardUp = newUp;

        setAngleToo = Quaternion.LookRotation(newFwd, newUp);
        

    }

   

    private void UpdateControlValues()
    {
        if(movement == null || boardRB == null) return;
        Vector2 inputted = movement.ReadValue<Vector2>();

        userForwardForce = 0;
        userLeanForwardReq = 0;

        userForwardForce = Mathf.Clamp(inputted.y, -1f,1f);
        
        userTurnReq *= 0.9f;
        userTurnReq += Mathf.Clamp(inputted.x, -0.2f,0.2f);
        
        allignMulti = Mathf.Clamp(boardRB.linearVelocity.sqrMagnitude / 1000f,0f,1f);
    }
    public void UpdateBoard()
    {
        if(!setUp) return;
        UpdateControlValues();
        
        rotateAdjust();

        Vector3 force = userForwardForce * skateBoardBody.transform.forward * speed;
        Vector3 subtractForce = ((boardRB.linearVelocity.normalized  )* speed) * allignMulti;

        
        if(groundedBuffer > 0)
        {
            boardRB.AddForce( force + subtractForce.magnitude * skateBoardBody.transform.forward, ForceMode.Acceleration);//booster
            boardRB.AddForce(-(subtractForce), ForceMode.Acceleration);
        }
        

        Vector3 vel = boardRB.linearVelocity;
        //skateBoardBody.transform.position = Vector3.SmoothDamp( skateBoardBody.transform.position, skateBoard.transform.position,ref vel, 0.9f * Time.deltaTime);

        Quaternion implied = Quaternion.Lerp(skateBoardBody.transform.rotation, setAngleToo, rotationSpeed * Time.deltaTime);
        //skateBoardBody.transform.rotation = implied;
        dirRB.interpolation = RigidbodyInterpolation.Interpolate;
        dirRB.Move(Vector3.SmoothDamp( skateBoardBody.transform.position, skateBoard.transform.position,ref vel, 0.9f * Time.deltaTime), implied);
        
        

        rb.Move( playerPositionOnBoard.position, skateBoardBody.transform.rotation);
        
        //BoardForward = skateBoardBody.transform.forward;
        //BoardUp = skateBoardBody.transform.up;
        
        visuals.realBoardTransform = skateBoardBody.transform;
        visuals.boardForward = skateBoardBody.transform.forward;
        visuals.boardVelForward = boardRB.linearVelocity.normalized;
        visuals.UpdateVisuals();

        groundedBuffer =- 1;
        groundedBuffer = Mathf.Clamp(groundedBuffer, 0, 5);
    }
}
