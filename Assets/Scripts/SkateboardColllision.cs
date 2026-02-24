using UnityEngine;

public class SkateboardColllision : MonoBehaviour
{
    public SkateboardControl skateboardControl;
    public void OnCollisionEnter(Collision collision)
    {
        if(skateboardControl != null)
        {
            skateboardControl.UpdateGroundData(collision);
        }
    }
}
