using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

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
    private quaternion GroundAngle = quaternion.identity;
    private Vector3 boardIntendedForward = Vector3.forward;
    private quaternion setAngleToo;
    private float speed;
    //-------------------------------------------
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

        //skateBoard.GetComponent<SkateboardColllision>().skateboardControl = this;
        
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
        GroundAngle = initRot;
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
        Vector3 norm = collision.contacts[0].normal;

        float up = Vector3.Angle(skateBoardBody.transform.up, norm);
        float down = Vector3.Angle(-skateBoardBody.transform.up, norm);

        float ang = Mathf.Min(up, down);
        if(ang <= maxLandAngle)
        {
  
            Vector3 projForward = Vector3.ProjectOnPlane(skateBoardBody.transform.forward, norm);

            if (projForward.sqrMagnitude < 0.001f) projForward = Vector3.Cross(norm, skateBoardBody.transform.right); //edge case

            GroundAngle = Quaternion.LookRotation(projForward, norm);
        }
        else
        {
            //fall off skateboard
        }
    }
    //

    // Update is called once per frame

    private quaternion boardAngles()
    {
        Vector3 boardForward = skateBoardBody.transform.forward;
        Vector3 ballForwardDir = boardRB.linearVelocity.normalized;
        Vector3 up = skateBoardBody.transform.up;

        if(ballForwardDir.sqrMagnitude  < minumumControlSpeed) ballForwardDir = skateBoardBody.transform.forward; //edge case


        Vector3 fromProj = Vector3.ProjectOnPlane(boardForward, up).normalized;
        Vector3 toProj = Vector3.ProjectOnPlane(ballForwardDir, up).normalized;

        float angle = Vector3.SignedAngle(fromProj, toProj, up);
        
        return Quaternion.AngleAxis(angle, up);
    }
    public void UpdateBoard()
    {
        boardAngles();
        Vector3 vel = boardRB.linearVelocity;
        skateBoardBody.transform.position = Vector3.SmoothDamp( skateBoardBody.transform.position, skateBoard.transform.position,ref vel, 0.9f * Time.deltaTime);

        Quaternion implied = Quaternion.Lerp(skateBoardBody.transform.rotation, GroundAngle, rotationSpeed * Time.deltaTime);
        skateBoardBody.transform.rotation = implied * boardAngles();

        
        transform.position = playerPositionOnBoard.position;


        transform.rotation = skateBoardBody.transform.rotation;
    }
}
