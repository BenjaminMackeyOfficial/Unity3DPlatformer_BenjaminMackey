using System;
using NUnit.Framework;
using UnityEngine;

public class SkateboardVisuals : MonoBehaviour
{
    [SerializeField] GameObject boardPrefab;
    private GameObject board;
    private Rigidbody boardRB;

    [SerializeField] GameObject sparksPrefab;
    private GameObject spark;
    private Vector3 sparkPos;
    private bool sparking;


    public Transform realBoardTransform;
    public Vector3 boardVelForward;
    public Vector3 boardForward;
    public float turnAmmount;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
        if(board == null) board = Instantiate(boardPrefab);
        board.SetActive(true);

        if(spark == null) spark = Instantiate(sparksPrefab); 
        spark.SetActive(false);

        boardRB = board.GetComponent<Rigidbody>();
    }
    void OnEnable()
    {
        if(board == null) board = Instantiate(board);
        board.SetActive(true);

        if(spark == null) spark = Instantiate(sparksPrefab); 
        spark.SetActive(false);
    }
    void OnDisable()
    {
        
    }

    public void PingGround(Collider col)
    { 
        sparking = true;

        Vector3 tmp = col.transform.position - board.transform.position;
        sparkPos = board.transform.position + tmp;

    }

    public void GenerateRot()
    {
        
    }

    // Update is called once per frame
    public void UpdateVisuals()
    {
        if(sparking) spark.SetActive(true);
        else spark.SetActive(false);
        spark.transform.position = sparkPos;


        
        boardRB.Move(realBoardTransform.position, realBoardTransform.rotation);




        sparking = false;
    }
}
