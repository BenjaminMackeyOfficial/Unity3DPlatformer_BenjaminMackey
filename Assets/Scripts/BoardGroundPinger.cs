using UnityEngine;

public class BoardGroundPinger : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public SkateboardVisuals visuals;
    private void OnTriggerStay(Collider collision)
    {
        if(visuals == null) return;
        visuals.PingGround(collision);
    }
}
