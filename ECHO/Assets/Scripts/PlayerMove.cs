using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerMove : MonoBehaviour
{
    public GameManager gameManager;
    public Transform headTransform;
    public AudioClip audioJump;
    public AudioClip audioAttack;
    public AudioClip audioDamaged;
    public AudioClip audioItem;
    public AudioClip audioDie;
    public AudioClip audioFinish;
    public float maxSpeed;
    public float jumpPower;
    public float ladderXThreshold = 0.5f; // 사다리 중앙 근처 체크
    public float climbSpeed = 1f; // 사다리 오르기 속도
    public float jumpOffLadderForce = 5f; // 사다리에서 뛰어내릴 때의 힘

    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator anim;
    CapsuleCollider2D capsulecollider;
    AudioSource audioSource;

    // Ladder 관련 변수
    private bool isClimbing = false;
    private Collider2D currentLadder;
    private float verticalInput;
    private int originalLayer;
    private int climbingLayer; // Climbing 레이어 인덱스
    private float defaultGravityScale = 4f; // RigidBody2D의 초기 Gravity Scale 값이라고 가정

    private bool isDead = false;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        capsulecollider = GetComponent<CapsuleCollider2D>();
        audioSource = GetComponent<AudioSource>();

        // 기본 레이어와 중력 스케일 저장
        originalLayer = gameObject.layer;
        climbingLayer = LayerMask.NameToLayer("ClimbingPlayer");

        // RigidBody2D의 기본 Gravity Scale 값 저장 (Inspector 값으로 초기화)
        defaultGravityScale = rigid.gravityScale;
    }

    void PlaySound(string action)
    {
        switch (action)
        {
            case "JUMP":
                audioSource.clip = audioJump;
                break;
            case "ATTACK":
                audioSource.clip = audioAttack;
                break;
            case "DAMAGED":
                audioSource.clip = audioDamaged;
                break;
            case "ITEM":
                audioSource.clip = audioItem;
                break;
            case "DIE":
                audioSource.clip = audioDie;
                break;
            case "FINISH":
                audioSource.clip = audioFinish;
                break;
        }
        audioSource.Play();
    }

    void StartClimbing()
    {
        isClimbing = true;
        rigid.gravityScale = 0; // 중력 제거
        gameObject.layer = climbingLayer; // Climbing 레이어로 변경하여 platform 충돌 무시

        rigid.velocity = new Vector2(0, 0); // 현재 속도 초기화
        anim.SetBool("isClimbing", true);
        anim.SetBool("isHanging", false);
        anim.SetBool("isJumping", false);
    }

    void StopClimbing()
    {
        isClimbing = false;
        rigid.gravityScale = defaultGravityScale;
        gameObject.layer = originalLayer;   // 원래 레이어로 복구하여 Platform 충돌 가능

        currentLadder = null; // 현재 사다리 참조 해제
        anim.SetBool("isClimbing", false);
        anim.SetBool("isHanging", false);
    }


    void Update()
    {
        verticalInput = Input.GetAxisRaw("Vertical");
        float h = Input.GetAxisRaw("Horizontal");

        // 플레이어가 죽으면 Update 로직 실행 X
        if (isDead) return;

        // 1. 사다리 진입 로직
        // 사다리 중앙 근처에 있어야 진입 가능
        if (currentLadder != null && !isClimbing)
        {
            float ladderX = currentLadder.bounds.center.x;
            float xDifference = Mathf.Abs(transform.position.x - ladderX);

            // 아래로 내려갈 때와 위로 올라갈 때의 조건을 분리
            if (xDifference < ladderXThreshold)
            {
                // 1. 아래로 내려갈 때 (S키): 언제나 가능해야 함
                if (verticalInput < 0)
                {
                    gameObject.layer = climbingLayer;
                    StartClimbing();
                }
                // 2. 위로 올라갈 때 (W키): 땅에 서있지 않을 때만 가능해야 함
                else if (verticalInput > 0 && !IsGrounded()) 
                {
                    StartClimbing();
                }
            }
        }

        // 2. 사다리 이동 및 상태 관리
        if (isClimbing)
        {
            float xVelocity = h * maxSpeed;
            float yVelocity = verticalInput * climbSpeed;

            // W/S를 누르지 않고 A/D만 눌렀을 때도 매달림 상태 유지
            if (verticalInput != 0)
            {
                // Y축 이동 중
                rigid.velocity = new Vector2(xVelocity, yVelocity);
                anim.SetBool("isClimbing", true);
                anim.SetBool("isHanging", false);
            }
            else if (h != 0)
            {
                // W/S를 떼고 A/D만 누를 때 (좌우 이동 매달림 상태)
                rigid.velocity = new Vector2(xVelocity, 0);
                anim.SetBool("isClimbing", false);
                anim.SetBool("isHanging", true);
            }
            else
            {
                // 모든 입력이 없을 때: 매달림 (Hanging) 상태로 속도 0 고정
                rigid.velocity = Vector2.zero;
                anim.SetBool("isClimbing", false);
                anim.SetBool("isHanging", true);
            }

            // A/D 키를 눌렀을 때 좌우 반전 (유지)
            if (h != 0)
            {
                spriteRenderer.flipX = h > 0;
            }

            if (Input.GetButtonDown("Jump"))
            {
                // 사다리에서 점프하여 벗어남
                StopClimbing();
                // 수평 이동 중이면 그 방향으로 힘을 주고, 아니면 제자리에 점프
                Vector2 jumpDirection = new Vector2(h, 1f).normalized;
                rigid.AddForce(jumpDirection * jumpOffLadderForce, ForceMode2D.Impulse);
                anim.SetBool("isJumping", true);
                PlaySound("JUMP");
            }
        }

        // 3. 점프 처리 (Climbing 상태에서는 일반 점프 방지)
        if (Input.GetButtonDown("Jump") && !anim.GetBool("isJumping") && !isClimbing)
        {
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            anim.SetBool("isJumping", true);
            PlaySound("JUMP");
        }

        // 4. Horizontal 이동 처리 (Climbing 상태가 아닐 때만)
        if (!isClimbing)
        {
            rigid.velocity = new Vector2(h * maxSpeed, rigid.velocity.y);
            anim.SetBool("isWalking", Mathf.Abs(h) > 0.3f);

            if (h != 0)
            {
                spriteRenderer.flipX = h > 0;
            }

            // 플레이어가 땅에 서 있거나 걷고 있을 때 (isClimbing이 아닐 때), 
            // 사다리 관련 애니메이션은 확실히 꺼줌
            anim.SetBool("isClimbing", false);
            anim.SetBool("isHanging", false); 
        }
    }

    void FixedUpdate()
    {
        // climbing 아닐 때 horizontal 속도 제한
        if (!isClimbing)
        {
            // 기존 Horizontal 속도 제한 로직
            float newX = Mathf.Clamp(rigid.velocity.x, -maxSpeed, maxSpeed);
            rigid.velocity = new Vector2(newX, rigid.velocity.y);
        }

        // Landing 체크
        if (rigid.velocity.y < 0)
        {
            RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector2.down, 1f, LayerMask.GetMask("Platform"));
            if (rayHit.collider != null && rayHit.distance < 0.5f)
                anim.SetBool("isJumping", false);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            // Attack
            // if (rigid.velocity.y < 0 && transform.position.y > collision.transform.position.y)
            // {
            //     OnAttack(collision.transform);
            // }
            //else    // Damaged
            {
                OnDamaged(collision.transform.position);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            currentLadder = collision;
        }

        // 기존에 있던 코인 관련
        if (collision.gameObject.tag == "Item")
        {
            // Point
            bool isBronze = collision.gameObject.name.Contains("Bronze");
            bool isSilver = collision.gameObject.name.Contains("Silver");
            bool isGold = collision.gameObject.name.Contains("Gold");

            if (isBronze)
                gameManager.stagePoint += 50;
            else if (isSilver)
                gameManager.stagePoint += 100;
            else if (isGold)
                gameManager.stagePoint += 300;

            gameManager.stagePoint += 100;

            // Deactive Item
            collision.gameObject.SetActive(false);

            // Sound
            PlaySound("ITEM");
        }
        // 나중에 문이랑 상호작용하여 다음 스테이지로 이동하는 걸로 수정해야 함
        else if (collision.gameObject.tag == "Finish")
        {
            // Next Stage
            gameManager.NextStage();

            // Sound
            PlaySound("FINISH");
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        // 사다리 Trigger를 벗어나는 순간 isClimbing 상태라면 무조건 StopClimbing 호출
        if (collision.CompareTag("Ladder"))
        {
            if (isClimbing)
            {
                // 사다리 영역을 벗어나는 순간 중력 복구 및 Climbing 상태 종료
                StopClimbing();
            }
            else
            {
                // Climbing 중이 아닐 때, 단순히 사다리 영역을 지나칠 경우 currentLadder만 해제
                currentLadder = null;
            }
        }
    }

    void OnDamaged(Vector2 targetPos)
    {
        // Health Down
        gameManager.HealthDown();

        // Change Layer (Immortal Active)
        gameObject.layer = 11;

        // View Alpha
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);

        // Reaction Force
        int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1;
        rigid.AddForce(new Vector2(dirc, 1) * 7, ForceMode2D.Impulse);

        // Animation
        anim.SetTrigger("doDamaged");

        // Sound
        PlaySound("DAMAGED");

        Invoke("OffDamaged", 3);
    }

    // 나중에 공격 삭제
    // void OnAttack(Transform enemy)
    // {
    //     // Point
    //     gameManager.stagePoint += 100;

    //     // Reaction Force
    //     rigid.AddForce(Vector2.up * 10, ForceMode2D.Impulse);

    //     // Enemy Die
    //     EnemyMove enemyMove = enemy.GetComponent<EnemyMove>();
    //     enemyMove.OnDamaged();

    //     // Sound
    //     PlaySound("ATTACK");

    // }

    void OffDamaged()
    {
        gameObject.layer = 10;
        spriteRenderer.color = new Color(1, 1, 1, 1);
    }

    public void OnDie()
    {
        // 1. 사망 상태로 변경 (Update() 조작을 막음)
        isDead = true;

        // 2. 물리 효과 정지
        rigid.velocity = Vector2.zero; // 현재 속도를 0으로
        rigid.simulated = false;       // 중력 및 모든 물리 시뮬레이션을 끔 (바닥으로 안 떨어짐)

        // 3. 충돌체 비활성화
        capsulecollider.enabled = false;

        // 4. Sprite Alpha
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);

        // 5. 사망 애니메이션 재생
        // (Animator에 "Die"라는 이름의 Trigger를 만들었다고 가정)
        anim.SetTrigger("isDead");

        // 6. Sound (기존 코드)
        PlaySound("DIE");
    }

    public void Respawn()
    {
        // 1. 생존 상태로 변경 (Update() 조작을 다시 받음)
        isDead = false;

        // 2. 물리/충돌 복구
        rigid.simulated = true;
        capsulecollider.enabled = true;

        // 3. 시각적/상태 복구
        spriteRenderer.color = new Color(1, 1, 1, 1f);
        rigid.gravityScale = defaultGravityScale;
        gameObject.layer = originalLayer;

        // 4. 사다리 상태 강제 초기화 (기존 코드)
        if (isClimbing)
        {
            isClimbing = false;
            currentLadder = null;
        }

        // 5. [가장 중요!] 애니메이터 리셋
        // 'Die' Trigger가 남아있는 것을 방지
        anim.ResetTrigger("isDead");
        // 'Idle' 상태로 돌아가도록 'Respawn' Trigger 발동 (Animator에서 설정한 것)
        anim.SetTrigger("Respawn");

        // 6. (안전을 위한) bool들도 초기화
        anim.SetBool("isClimbing", false);
        anim.SetBool("isHanging", false);
        anim.SetBool("isJumping", false);
        anim.SetBool("isWalking", false);
    }

    public void VelocityZero()
    {
        rigid.velocity = Vector2.zero;
    }

    bool IsGrounded()
    {
        // 플레이어의 현재 위치에서 아래 방향으로 0.1f 길이의 선을 쏴서
        // 'Platform' 레이어를 가진 콜라이더가 감지되는지 확인
        // new Vector2(0, -0.5f)는 레이 시작 위치를 발밑 근처로 조정하는 오프셋
        RaycastHit2D rayHit = Physics2D.Raycast((Vector2)transform.position + new Vector2(0, -0.5f),
                                                Vector2.down, 0.1f, LayerMask.GetMask("Platform"));

        // 감지된 콜라이더가 있다면(null이 아니라면) 땅에 서 있는 것!
        return rayHit.collider != null;
    }
    

}
