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

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // Déplacement (Vector 2)
    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    // Saut (Bouton) - Appelé automatiquement par le Player Input lors de l'appui sur Espace
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
    }
}
