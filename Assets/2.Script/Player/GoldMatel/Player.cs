using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    public float jumpPower = 15;

    float hAxis;
    float vAxis;

    public Vector3 moveVec { get; set; }
    public Vector3 dodgeVec;

    Animator ani;
    Rigidbody rigid;

    bool jDown;

    bool isJump;
    bool isDodge;

    private void Awake()
    {
        ani = GetComponentInChildren<Animator>(); //자식 컴포넌트 가져오기
        rigid = GetComponent<Rigidbody>();
    }
    private void Update()
    {
        Move();
        GetInput();
        Turn();
        Jump();
        Dodge();
    }

 
    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        jDown = Input.GetButtonDown("Jump");
    }

    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        if (isDodge)
        {
            moveVec = dodgeVec;
        }
        transform.position += moveVec * speed * Time.deltaTime;
        ani.SetBool("IsRun", moveVec != Vector3.zero);
    }

    void Turn()
    {
        transform.LookAt(transform.position + moveVec);
    }

    void Jump()
    {
        if(jDown && moveVec == Vector3.zero&& !isJump)
        {
            rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            ani.SetBool("IsJump", true);
            ani.SetTrigger("DoJump");
            isJump = true;

        }
    }
    void Dodge()
    {
        if (jDown && !isJump && moveVec != Vector3.zero && !isDodge)
        {
            dodgeVec = moveVec;
            speed *= 2;
            ani.SetTrigger("DoDodge");
            isDodge = true;

            Invoke("DodgeOut", 0.6f);
        }
    }
    void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Ground")
        {
            ani.SetBool("IsJump", false);
            isJump = false;
        }
    }
}
