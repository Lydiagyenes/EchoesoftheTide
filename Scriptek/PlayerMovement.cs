using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float runSpeed = 6f;
    
    [Header("Stamina Beállítás")]
    public float sprintCost = 15f; // Mennyi stamina fogy másodpercenként

    private bool isSprinting = false;
    public float turnSmoothTime = 0.1f;
    
    public Transform cam; 

    private CharacterController controller;
    private Animator animator;
    private float turnSmoothVelocity;
    private Vector3 playerVelocity;
    private float gravityValue = -9.81f;
    public float jumpHeight = 1.5f; 
    
    [HideInInspector] 
    public bool canMove = true; 

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        
        if (cam == null && Camera.main != null)
        {
            cam = Camera.main.transform;
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log($"[Movement] START lefutott. CanMove: {canMove}, Controller aktív: {controller.enabled}");
    }

    void Update()
    {
        // 1. FÖLD ÉRZÉKELÉS
        bool isGrounded = controller.isGrounded;
        
        // Gravitáció alaphelyzetbe állítása, ha földön vagyunk
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f; 
        }

        Vector3 finalMove = Vector3.zero; // Ebben gyűjtjük össze a mozgást

        if (canMove)
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
            bool isMoving = direction.magnitude >= 0.1f;

              if (direction.magnitude > 0.1f)
                {
                    // Debug.Log($"[Movement] Gombnyomás érzékelve! Irány: {direction}");
                }

            // --- STAMINA LOGIKA ---
            float currentSpeed = moveSpeed;
            isSprinting = false;

            if (Input.GetKey(KeyCode.LeftShift) && isMoving)
            {
                float actualSprintCost = sprintCost; // Az alapérték (15)

                if (SkillManager.Instance != null && SkillManager.Instance.HasSkill("Kitartas")) // Ellenőrizd az ID-t!
                {
                    actualSprintCost *= 0.75f; // 25% kedvezmény (csak a 75%-ába kerül)
                    // Debug.Log("Kitartás skill aktív! Költség: " + actualSprintCost); // Teszthez
                }
                if (PlayerStats.Instance != null)
                {
                    if (PlayerStats.Instance.ConsumeStamina(actualSprintCost * Time.deltaTime))
                    {
                        currentSpeed = runSpeed;
                        isSprinting = true;
                    }
                }
                else
                {
                    currentSpeed = runSpeed;
                    isSprinting = true;
                }
            }

            // --- ANIMÁTOR ---
            animator.SetFloat("Speed", direction.magnitude);
            animator.SetBool("isRunning", isSprinting);

            // --- UGRÁS ---
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityValue);
                animator.SetTrigger("Jump");
            }

            // --- MOZGÁS IRÁNY KISZÁMÍTÁSA ---
            if (isMoving)
            {
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                Vector3 moveDir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                
                // HOZZÁADJUK A VÍZSZINTES MOZGÁST
                finalMove += moveDir.normalized * currentSpeed;
            }
        }
        else 
        {
            animator.SetFloat("Speed", 0);
            animator.SetBool("isRunning", false);
        }

        // --- GRAVITÁCIÓ HOZZÁADÁSA ---
        playerVelocity.y += gravityValue * Time.deltaTime;
        finalMove.y = playerVelocity.y; // Y tengelyen a gravitáció érvényesül

        if (finalMove.x != 0 || finalMove.z != 0)
        {
           // Debug.Log($"[Movement] Move parancs kiadva: {finalMove * Time.deltaTime}");
        }

        // --- EGYETLEN KÖZÖS MOZGÁS PARANCS ---
        // Ez a legbiztosabb módja a CharacterController használatának
        controller.Move(finalMove * Time.deltaTime);
        
        animator.SetBool("isGrounded", isGrounded);
    }

    // ==========================================
    // --- MENTÉS ÉS BETÖLTÉS (ISaveable) ---
    // ==========================================

    public void SaveData(ref GameData data)
    {
        // Elmentjük a pontos térbeli pozíciót és forgást
        data.playerPosition = transform.position;
        data.playerRotation = transform.rotation;
        
        Debug.Log($"[PlayerMovement] Pozíció mentve: {transform.position}");
    }

    public void LoadData(GameData data)
    {
        if (data.playerPosition == Vector3.zero) return;

        // FIZIKAI MOTOR KIKAPCSOLÁSA A MOZGATÁS IDEJÉRE!
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        transform.position = data.playerPosition;
        transform.rotation = data.playerRotation;

        if (controller != null)
        {
            controller.enabled = true;
        }

        Debug.Log($"<color=green>[PlayerMovement] Játékos áthelyezve: {transform.position}</color>");
    }
}