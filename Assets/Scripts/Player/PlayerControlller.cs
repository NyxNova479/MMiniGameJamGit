using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Déplacement")]
    public float speed = 5f;
    private CharacterController controller;
    private Vector2 moveInput;

    [Header("Physique & Saut")]
    private float jumpHeight = 2f;
    private float gravity = -9.81f;
    private Vector3 playerVelocity;
    private bool isGrounded;

    // --- Coyote Time ---
    private float coyoteTime = 0.5f;
    private float coyoteTimeCounter = 0f;


    private float originalHeight;
    private Vector3 originalCenter;
    private float originalSpeed;
    private bool isCrouching = false;

    [Header("Crouch Renderer")]
    // Référence à l'enfant qui contient le Renderer (sera scale pour l'effet de crouch)
    public Transform rendererChild;
    // Multiplicateur d'échelle sur l'axe Y pour le renderer (ex: 0.5 pour moitié)
    public float rendererCrouchYMultiplier = 0.5f;
    // Vitesse de lissage de l'échelle
    public float rendererScaleSmooth = 10f;
    // Vitesse de déplacement pendant le crouch
    public float crouchSpeedMultiplier = 0.5f;
    // Vérification avant de se relever
    public float standUpCheckRadius = 0.15f;
    public LayerMask ceilingMask = ~0;
    private Vector3 originalRendererScale;
    private Vector3 targetRendererScale;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        // Sauvegarder les valeurs originales pour le crouch
        originalHeight = controller.height;
        originalCenter = controller.center;
        originalSpeed = speed;
        if (rendererChild != null)
        {
            originalRendererScale = rendererChild.localScale;
            targetRendererScale = originalRendererScale;
        }

    }
    // Action crouch (toggle)
    void OnCrouch()
    {
        if (isCrouching)
            TryStopCrouch();
        else
            StartCrouch();
    }

    void StartCrouch()
    {
        if (isCrouching) return;
        // visuel: réduire l'échelle de l'enfant renderer
        if (rendererChild != null)
        {
            targetRendererScale = new Vector3(originalRendererScale.x, originalRendererScale.y * rendererCrouchYMultiplier, originalRendererScale.z);
        }
        // mouvement: réduire la vitesse
        speed = originalSpeed * crouchSpeedMultiplier;
        isCrouching = true;
    }

    void TryStopCrouch()
    {
        // Vérifier qu'il n'y a pas d'obstacle au-dessus avant de se relever
        Vector3 headPosition = transform.position + controller.center + Vector3.up * (originalHeight / 2f);
        bool blocked = Physics.CheckSphere(headPosition, standUpCheckRadius, ceilingMask, QueryTriggerInteraction.Ignore);
        if (!blocked)
        {
            StopCrouch();
        }
        // sinon rester accroupi
    }

    void StopCrouch()
    {
        if (!isCrouching) return;
        if (rendererChild != null)
        {
            targetRendererScale = originalRendererScale;
        }
        speed = originalSpeed;
        isCrouching = false;
    }
    

    // Déplacement (Vector 2)
    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void OnJump()
    {
        // Autoriser le saut si on est au sol ou si le compteur de coyote est encore positif
        if (isGrounded || coyoteTimeCounter > 0f)
        {
            // Formule physique pour atteindre une hauteur précise : v = sqrt(h * -2 * g)
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            coyoteTimeCounter = 0f; // consommer le coyote time
        }
    }





    void Update()
    {
        // Mettre à jour l'état au sol d'abord
        isGrounded = controller.isGrounded;

        // Gérer le compteur de coyote
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
        if (isGrounded && playerVelocity.y < 0)
        {
            // On maintient une légère force négative pour coller au sol
            playerVelocity.y = -2f;
        }

        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        controller.Move(move * speed * Time.deltaTime);


        playerVelocity.y += gravity * Time.deltaTime;


        controller.Move(playerVelocity * Time.deltaTime);

        // Lissage de l'échelle du renderer child si nécessaire
        if (rendererChild != null)
        {
            rendererChild.localScale = Vector3.Lerp(rendererChild.localScale, targetRendererScale, Mathf.Clamp01(rendererScaleSmooth * Time.deltaTime));
        }
    }
}
