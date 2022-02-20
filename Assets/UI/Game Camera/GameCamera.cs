using Cinemachine;
using UnityEngine;
using UnityEngine.Events;

public class GameCamera : MonoBehaviour
{
    private CinemachineVirtualCamera m_main;

    private CinemachineImpulseSource shake;

    public static int DEFAULT_CAMERA_ZOOM = 5;

    void Awake()
    {
        m_main = transform.Find("Main").GetComponent<CinemachineVirtualCamera>();

        shake = GetComponent<CinemachineImpulseSource>();
    }

    private void Start()
    {
        Transform player = GameObject.FindGameObjectWithTag("Player").transform;
        m_main.Follow = player;
    }

    public void Shake()
    {
        shake.GenerateImpulse();
    }

    public void Zoom(int orthoSize)
    {
        m_main.m_Lens.OrthographicSize = orthoSize;
    }
}
