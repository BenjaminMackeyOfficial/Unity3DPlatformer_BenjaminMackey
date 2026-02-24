using UnityEngine;

public class SkateboardColllision : MonoBehaviour
{
    public SkateboardControl skateboardControl;
    public void OnCollisionStay(Collision collision)
    {
        if(skateboardControl != null)
        {
            skateboardControl.UpdateGroundData(collision);
        }
    }

}
