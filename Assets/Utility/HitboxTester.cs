using HitboxSystem;
using UnityEngine;

public class HitboxTester : MonoBehaviour
{
    private Hitbox[] hitBoxes;
    // Start is called before the first frame update
    void Awake()
    {
        hitBoxes = GetComponentsInChildren<Hitbox>();
        // Set up hit box event Listeners
        foreach (Hitbox hitbox in hitBoxes)
        {
            hitbox.Handler += new Hitbox.HitboxEventHandler(Test);
        }
    }

    private void Test(object sender, HitboxEventArgs e)
    {
        Debug.Log("Hitbox Tester registered hit with from: " + sender);
    }
}
