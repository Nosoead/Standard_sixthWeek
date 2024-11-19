using UnityEngine;
using UnityEngine.InputSystem;

//플레이어의 경우 Input이 있어서 Input 관련해서 처리함.
public class PlayerInputController : TopDownController
{
    private Camera mainCam;
    protected override void Awake()
    {
        base.Awake(); //부모꺼 베이스로 들고오기
        mainCam = Camera.main; //MainCamera 태그 붙어있는 카메라를 가져온다.
    }

    public void OnMove(InputValue value)//Event 실행전 전처리과정 (노말라이즈)
    {
        Vector2 moveInput = value.Get<Vector2>().normalized;
        CallMoveEvent(moveInput);
        //실제 움직이는 처리는 여기서하는게 아니라 PlayerMovement에서 함
    }

    public void OnLook(InputValue value)//Event 실행전 전처리과정
    {
        Vector2 newAim = value.Get<Vector2>();
        Vector2 worldPos = mainCam.ScreenToWorldPoint(newAim);
        newAim = (worldPos - (Vector2)transform.position).normalized;
        Debug.Log(newAim);
        CallLookEvent(newAim);
    }

    public void OnFire(InputValue value)
    {
        IsAttacking = value.isPressed;
    }
}
