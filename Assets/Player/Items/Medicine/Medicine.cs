using System.Collections;
using UnityEngine;

public class Medicine : MonoBehaviour
{
    private Player player;
    // Range of 0 - 1
    private const float PERCENTAGE_HEAL = 0.75f;

    private float amountToHeal = 0;
    private float amountHealed = 0;

    void Start()
    {
        player = GetComponentInParent<Player>();
        amountToHeal = player.MAX_HEALTH * PERCENTAGE_HEAL;
        StartCoroutine(ApplyMedicine());
    }

    private IEnumerator ApplyMedicine()
    {
        while (amountHealed < amountToHeal) {
            amountHealed++;
            // Apply healing if under player max health
            if(player.Health < player.MAX_HEALTH) { player.Health++; }
            yield return new WaitForFixedUpdate();
        }
        Destroy(this.gameObject);
    }
}
