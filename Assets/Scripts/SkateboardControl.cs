using Unity.Mathematics;
using UnityEngine;

public class SkateboardControl : MonoBehaviour
{
    private GameObject skateBoard;
    private Rigidbody boardRB;
    private Transform boardBody;
    private Transform playerPositionOnBoard;
    [SerializeField] GameObject skateboardPrefab;

    //contact info (like ground, not phone number)
    private quaternion setAngleToo = quaternion.identity;
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
        boardBody = skateBoard.transform.Find("SkateboardBody");
        playerPositionOnBoard = boardBody.Find("PlayerHolder");
        
        skateBoard.SetActive(false);
        if(
            skateBoard != null &&
            boardRB != null &&
            boardBody != null &&
            playerPositionOnBoard != null
        )
        { 
            setUp = true;
            skateBoard.GetComponent<SkateboardColllision>().skateboardControl = this;
        }

    }

    public void EnableBoard()
    {
        skateBoard.SetActive(true);
        skateBoard.transform.position = transform.position;
    }

    public void DissableBoard()
    {
        skateBoard.SetActive(false);
    }

    //ground info update
    public void UpdateGroundData(Collision collision)
    {
        
    }
    //

    // Update is called once per frame
    public void UpdateBoard()
    {
        boardBody.rotation = setAngleToo;
        transform.position = playerPositionOnBoard.position;
    }
}
