using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Splines;


public class CameraSplineController : MonoBehaviour
{
    [Tooltip("Virtual Camera contenant un composant TrackedDolly")]
    [SerializeField] private CinemachineCamera  vcam;


    [Tooltip("Transform du joueur; utilisé pour déterminer la position le long du spline (axe X)")]
    public Transform player;

    [Header("Mapping joueur -> spline")]
    [SerializeField] private float playerMinX = -10f;
    [SerializeField] private float playerMaxX = 10f;
    [SerializeField] private float pathMin = 0f;
    [SerializeField] private float pathMax = 10f;

    [Header("Lissage")]
    [SerializeField] private float smoothTime = 0.2f;

    private CinemachineSplineDolly dolly;
    private float velocity = 0f;

    void Start()
    {
        if (vcam == null)
        {
            Debug.LogWarning("CameraSplineController: vcam non assignée.");
            return;
        }


        dolly = vcam.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachineSplineDolly;
        if (dolly == null)
        {
            Debug.LogWarning("CameraSplineController: la Virtual Camera n'a pas de composant TrackedDolly. Ajoutez un TrackedDolly via l'inspecteur Cinemachine.");
        }
    }

    void Update()
    {
        if (vcam == null || player == null || dolly == null) return;

        float t = Mathf.InverseLerp(playerMinX, playerMaxX, player.position.x);
        float target = Mathf.Lerp(pathMin, pathMax, t);
        dolly.CameraPosition = Mathf.SmoothDamp(dolly.CameraPosition, target, ref velocity, smoothTime);
    }
}
