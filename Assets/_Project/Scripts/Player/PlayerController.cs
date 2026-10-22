using UnityEngine;
using UnityEngine.InputSystem; // Dùng để test bằng bàn phím trong Editor

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private VirtualJoystick joystick;
    [SerializeField] private float moveSpeed = 5f; // Sau này sẽ lấy từ chỉ số SPD

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Awake()
    {
        // Lấy Rigidbody2D một lần duy nhất, không gọi GetComponent trong Update
        rb = GetComponent<Rigidbody2D>();
    }

    // Đọc input mỗi frame
    private void Update()
    {
        moveInput = joystick.Direction;

#if UNITY_EDITOR
        // Chỉ dùng khi test trên máy tính: nếu joystick không chạm, đọc phím WASD/mũi tên
        if (moveInput == Vector2.zero && Keyboard.current != null)
        {
            var k = Keyboard.current;
            float x = (k.dKey.isPressed || k.rightArrowKey.isPressed ? 1 : 0)
                    - (k.aKey.isPressed || k.leftArrowKey.isPressed ? 1 : 0);
            float y = (k.wKey.isPressed || k.upArrowKey.isPressed ? 1 : 0)
                    - (k.sKey.isPressed || k.downArrowKey.isPressed ? 1 : 0);
            moveInput = new Vector2(x, y).normalized;
        }
#endif
    }

    // Di chuyển vật lý đặt trong FixedUpdate để mượt và ổn định
    private void FixedUpdate()
    {
        // Unity 6 dùng linearVelocity (tên cũ là velocity)
        rb.linearVelocity = moveInput * moveSpeed;
    }
}