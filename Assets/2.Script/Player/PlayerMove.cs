using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerMove : MonoBehaviourPun
{
    [Header("Aim")]
    //[SerializeField]
    //Camera AimCam;
    [SerializeField]
    GameObject aimImage;


    [SerializeField]
    Transform playerBody;
    [SerializeField]
    Transform cameraArm;

    float speed = 15;
    float jumpPower = 10;
    float fireTime;




    bool isBorder; //벽 충돌 
    bool isJump;
    bool isDodge;
    bool isFire;
    bool isReload;

    Vector3 moveVec;
    Vector3 dodgeVec;
    Vector3 targetPosition;

    public Weapon weapon;
    Animator ani;
    Rigidbody rigid;


    

    private void Awake()
    {
        ani = GetComponentInChildren<Animator>();
        rigid = GetComponentInChildren<Rigidbody>();
        aimImage = GameObject.Find("AimCheck");
    }

    private void Update()
    {
        fireTime += Time.deltaTime;
        if (photonView.IsMine)
        {
            AimCheck();
        }
        if(weapon.curAmmo == 0)
        {
            Reload();
        }
    }
    private void FixedUpdate()
    {
        FreezeRotation();
        StopToWall();
    }
    void StopToWall()
    { 
        isBorder = Physics.Raycast(playerBody.transform.position, playerBody.transform.forward, 5, LayerMask.GetMask("Wall"));
    }
    void FreezeRotation() //물리충돌 버그 방지
    {
        rigid.angularVelocity = Vector3.zero;
    }
    void AimCheck()
    {

        targetPosition = Vector3.zero;
        Transform camTransform = Camera.main.transform;
        RaycastHit hit;

        if (Physics.Raycast(camTransform.position, camTransform.forward, out hit, Mathf.Infinity))
        {
            targetPosition = hit.point;
            aimImage.SetActive(true);
        }

        Vector3 targetAim = targetPosition;
        targetAim.y = transform.position.y;
        Vector3 aimDir = (targetAim - transform.position).normalized;

        playerBody.forward = aimDir; //에임 방향 바라보기
    }

    public void Move(Vector2 inputDir) 
    {
        moveVec = new Vector3 (inputDir.x,0,inputDir.y);
        bool isMove = moveVec.magnitude != 0;
        if (isDodge)
        {
            moveVec = dodgeVec;
        }
        if(isReload)
        {
            moveVec = Vector3.zero;
        }

        if (isMove)
        {
            Vector3 lookForward = new Vector3(cameraArm.forward.x, 0f, cameraArm.forward.z).normalized;
            Vector3 lookRight = new Vector3(cameraArm.right.x, 0f, cameraArm.right.z).normalized;
            Vector3 moveDir = (lookForward * moveVec.z) + (lookRight * moveVec.x);

            //playerBody.forward = moveDir; 
            float speedMultiplier = 1f;
            if (Mathf.Abs(inputDir.x) > Mathf.Abs(inputDir.y))
            {
                // 옆 이동이 더 큰 경우 → 걷는 속도
                speedMultiplier = 0.5f;
            }
            if(!isBorder)
                transform.position += moveDir * Time.deltaTime * speed * speedMultiplier;
        
        }
        if (photonView.IsMine)
        {
            // 걷기, 달리기 판별
            if (inputDir.magnitude == 0)
            {
                ani.SetBool("IsWalk", false);
                ani.SetBool("IsRun", false);
            }
            else if (Mathf.Abs(inputDir.x) > Mathf.Abs(inputDir.y))
            {
                ani.SetBool("IsWalk", true);
                ani.SetBool("IsRun", true);
            }
            else
            {
                ani.SetBool("IsWalk", false);
                ani.SetBool("IsRun", true);
            }
        }

    }

    public void Jump()
    {
        if(moveVec == Vector3.zero && !isJump)
        {
            rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            if (photonView.IsMine)
            {
                ani.SetBool("IsJump", true);
                ani.SetTrigger("DoJump");
            }
          
            isJump = true;
        }
    }
    public void Fire()
    {
        isFire = fireTime > 0.3f;
        if(!isDodge && isFire)
        {
            if (photonView.IsMine)
            {
                ani.SetTrigger("DoShot");
            }
           
            weapon.Use(targetPosition);
            fireTime = 0;
        }
        
    }

    public void Reload()
    {
        if (weapon.curAmmo == weapon.maxAmmo) return;

        if(!isJump && !isDodge )
        {
            if (photonView.IsMine)
            {
                ani.SetTrigger("DoReload");
            }
            isReload = true;

            Invoke("ReloadOut", 2f);
        }
    }
 
    public void Dodge()
    {
        if(!isJump  && !isDodge /*&& moveVec != Vector3.zero*/)
        {
            dodgeVec = moveVec;
            speed *= 2;
            if (photonView.IsMine)
            {
                ani.SetTrigger("DoDodge");
            }
            
            isDodge = true;

            Invoke("DoDodgeOut", 0.6f);
        }
    }


    public void Land()
    {
        if (photonView.IsMine)
        {
            ani.SetBool("IsJump", false);
        }
      
        isJump = false;
    }
    public void Dead()
    {
        if (photonView.IsMine)
        {
            photonView.RPC("PlayDeadAnim", RpcTarget.All);
        }
    }
    [PunRPC]
    void PlayDeadAnim()
    {
        ani.SetTrigger("DoDie");
    }
    void DoDodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
    }
    void ReloadOut()
    {
        weapon.curAmmo = weapon.maxAmmo;
        isReload = false;
    }

}
