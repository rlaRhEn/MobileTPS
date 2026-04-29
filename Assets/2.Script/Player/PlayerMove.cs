using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerMove : MonoBehaviourPun
{
    [Header("Aim")]
    [SerializeField]
    GameObject aimImage;


    [SerializeField]
    Transform playerBody;
    [SerializeField]
    Transform cameraArm;

    float speed = 15;
    float jumpPower = 10;
    float fireTime;




    bool isBorder; //�� �浹 
    bool isJump;
    bool isDodge;
    bool isFire;
    bool isReload;
    bool isDead;

    Vector3 moveVec;
    Vector3 dodgeVec;
    Vector3 targetPosition;

    public Weapon weapon;
    Animator ani;
    Rigidbody rigid;

    //��� Panel
    

    private void Awake()
    {
        ani = GetComponentInChildren<Animator>();
        rigid = GetComponentInChildren<Rigidbody>();
        aimImage = GameObject.Find("AimCheck");
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsMatchEnded)
            return;
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
    void FreezeRotation() //�����浹 ���� ����
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

        playerBody.forward = aimDir; //���� ���� �ٶ󺸱�
    }

    public void Move(Vector2 inputDir) 
    {
        if (isDead) 
            return;
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
                
                speedMultiplier = 0.5f;
            }
            if(!isBorder)
                transform.position += moveDir * Time.deltaTime * speed * speedMultiplier;
        
        }
        if (photonView.IsMine)
        {
           
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
        if (!CanControl()) return;
        if (isDead)
            return;
        if (moveVec == Vector3.zero && !isJump)
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
        if (!CanControl()) return;
        if (isDead)
            return;
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
        if (!CanControl()) return;
        if (isDead)
            return;
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
        if (!CanControl()) return;
        if (isDead)
            return;
        if (!isJump  && !isDodge )
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
        if (!CanControl()) return;
        if (photonView.IsMine)
        {
            ani.SetBool("IsJump", false);
        }
      
        isJump = false;
    }
    public void Dead()
    {
        if (!photonView.IsMine) return;

        photonView.RPC(nameof(RpcSetDeadState), RpcTarget.All, true);
    }
    [PunRPC]
    void RpcSetDeadState(bool dead)
    {
        isDead = dead;
        ani.SetBool("IsDie", dead);

        if (dead)
        {
            ani.ResetTrigger("DoDie");
            ani.SetTrigger("DoDie");
        }
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
    [PunRPC]
    public void RpcRespawn(Vector3 pos, Quaternion rot)
    {
        transform.SetPositionAndRotation(pos, rot);
        rigid.linearVelocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;

        ani.ResetTrigger("DoDie");
        ani.SetBool("IsDie", false);
        isDead = false;

        weapon.curAmmo = weapon.maxAmmo;

        var col = GetComponentInChildren<PlayerCollision>();
        if (col != null)
            col.ResetAfterRespawn();
    }
    bool CanControl()
    {
        return !isDead && GameManager.Instance != null && !GameManager.Instance.IsMatchEnded;
    }
    [PunRPC]
    public void RpcDamageFlash()
    {
        var col = GetComponentInChildren<PlayerCollision>();
        if (col != null)
        {
            col.PlayDamageFlash();
        }
    }
}
