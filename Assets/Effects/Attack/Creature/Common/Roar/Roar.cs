using CreatureSystems;
using UnityEngine;

public class Roar : MonoBehaviour
{
    private Creature creature;

    private const float ROAR_RADIUS = 15f;

    private void Awake()
    {
        creature = GetComponentInParent<Creature>();
    }

    void Start()
    {
        
    }
}
