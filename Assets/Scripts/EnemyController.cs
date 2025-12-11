using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

/// <summary>
/// 通用敌人控制：
/// - 普通地面场景（使用 NavMesh）
/// - 敌人有 Idle / Chase 两种动画（bool 参数名：IsChase）
/// - 敌人会在一定视野 & 距离内发现玩家：
///     alarmLevel 0: 未发现，UI 不显示
///     alarmLevel 1: 问号阶段，黄色条从下往上填充
///     alarmLevel 2: 叹号阶段，追击玩家
/// - 玩家跑远后敌人停止追击并回到初始位置
/// - 头顶问号/叹号 UI 始终面向相机
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("玩家 Transform（如果留空，会在 Awake 中按 Tag=Player 自动查找）")]
    public Transform player;

    [Header("Components")]
    public NavMeshAgent agent;   // NavMeshAgent
    public Animator animator;    // 敌人 Animator（含 bool 参数 IsChase）

    [Header("Sign UI")]
    [Tooltip("头顶挂 UI 的节点，例如 SignRoot")]
    public Transform signRoot;
    
    [Tooltip("底层白/灰问号图（Simple Image）")]
    public Image questionBaseImage;
    
    [Tooltip("上层填充条 + 叹号图（Filled Vertical Image）")]
    public Image signImage;
    
    [Tooltip("问号 Sprite")]
    public Sprite questionSprite;
    
    [Tooltip("叹号 Sprite")]
    public Sprite exclamationSprite;

    [Header("Sign Colors")]
    public Color baseQuestionColor = Color.white;   // 底层问号颜色
    public Color fillQuestionColor = Color.yellow;  // 黄色填充颜色
    public Color exclamationColor  = Color.red;     // 叹号颜色

    [Header("Detection")]
    [Tooltip("初次发现玩家的距离")]
    public float detectDistance = 10f;
    
    [Tooltip("玩家跑到多远算完全丢失")]
    public float loseDistance = 12f;
    
    [Tooltip("视野角度（度），例如 120 表示前方 120° 锥形")]
    [Range(0f, 180f)]
    public float fieldOfView = 120f;

    [Header("Alarm Settings")]
    [Tooltip("看到玩家时，问号填充速度（每秒）")]
    public float chargeSpeed = 0.5f;
    
    [Tooltip("看不到玩家时，问号回退速度（每秒）")]
    public float decaySpeed  = 1.0f;

    [SerializeField, Range(0f, 1f)]
    private float alarmValue = 0f;   // 问号填充值（0~1）

    [SerializeField, Range(0, 2)]
    private int alarmLevel = 0;      // 0=未发现, 1=问号, 2=叹号

    // 内部状态
    private bool isChasing = false;
    private Vector3 initialPosition;

    // 为减少字符串比较开销，预先缓存 Animator 参数哈希
    private static readonly int IsChaseHash = Animator.StringToHash("IsChase");

    void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        initialPosition = transform.position;

        // 自动找 Player（Tag = "Player"）
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                player = p.transform;
            }
            else
            {
                Debug.LogWarning("EnemyController: 场景中找不到 Tag 为 Player 的对象，请手动在 Inspector 中指定 player 引用。");
            }
        }

        // 初始化 Sign UI：一开始全部隐藏
        if (questionBaseImage != null)
        {
            questionBaseImage.enabled = false;
            if (questionSprite != null)
                questionBaseImage.sprite = questionSprite;
            questionBaseImage.color = baseQuestionColor;
        }

        if (signImage != null)
        {
            signImage.enabled = false;
            if (questionSprite != null)
                signImage.sprite = questionSprite;
            signImage.color = fillQuestionColor;
            signImage.fillAmount = 0f;
        }
    }

    void Update()
    {
        if (player == null) return;

        UpdateAlarmAndSign();
        UpdateMovement();
        UpdateBillboard();
    }

    /// <summary>
    /// 更新警戒状态（alarmLevel / alarmValue）以及问号/叹号 UI。
    /// </summary>
    private void UpdateAlarmAndSign()
    {
        Vector3 toPlayer = player.position - transform.position;
        float distance   = toPlayer.magnitude;
        Vector3 dirToPlayer = distance > 0.001f ? toPlayer / distance : Vector3.zero;

        // 1. 距离判定
        bool inDistance = distance <= detectDistance;

        // 2. 视野角判定（简单 FOV）
        float halfFovRad = fieldOfView * 0.5f * Mathf.Deg2Rad;
        float cosHalfFov = Mathf.Cos(halfFovRad);
        bool inFov = Vector3.Dot(transform.forward, dirToPlayer) >= cosHalfFov;

        bool canSeePlayer = inDistance && inFov;

        switch (alarmLevel)
        {
            // 0：完全未发现玩家，UI 不显示
            case 0:
                isChasing = false;

                if (canSeePlayer)
                {
                    alarmLevel = 1;
                    alarmValue = 0f;
                    ShowQuestionSign();
                }
                break;

            // 1：问号阶段，填充或回退
            case 1:
                isChasing = false;

                if (canSeePlayer)
                {
                    alarmValue += chargeSpeed * Time.deltaTime;
                    if (alarmValue >= 1f)
                    {
                        alarmValue = 1f;
                        alarmLevel = 2;
                        ShowExclamationSign();
                        isChasing = true;
                    }
                }
                else
                {
                    alarmValue -= decaySpeed * Time.deltaTime;
                    if (alarmValue <= 0f)
                    {
                        alarmValue = 0f;
                        alarmLevel = 0;
                        HideSign();
                    }
                }

                alarmValue = Mathf.Clamp01(alarmValue);
                UpdateQuestionSignVisual();
                break;

            // 2：叹号阶段，追击中
            case 2:
                isChasing = true;

                // 玩家远离则降级回问号阶段（但仍记得他存在一段时间）
                if (distance > loseDistance)
                {
                    alarmLevel = 1;
                    isChasing = false;
                    ShowQuestionSign();
                    UpdateQuestionSignVisual();
                }
                break;
        }
    }

    /// <summary>
    /// NavMeshAgent 移动逻辑：追击玩家或返回初始位置，同时驱动 IsChase 动画。
    /// </summary>
    private void UpdateMovement()
    {
        Vector3 targetPos = isChasing && player != null ? player.position : initialPosition;

        if (!agent.isOnNavMesh) return;

        if (agent.destination != targetPos)
        {
            agent.SetDestination(targetPos);
        }

        bool reached = false;

        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance + 0.05f)
            {
                reached = true;
            }
        }

        if (reached)
        {
            agent.isStopped = true;
            if (animator != null)
            {
                animator.SetBool(IsChaseHash, false);
            }
        }
        else
        {
            agent.isStopped = false;
            if (animator != null)
            {
                animator.SetBool(IsChaseHash, true);
            }
        }
    }

    /// <summary>
    /// 头顶 SignRoot 始终朝向相机。
    /// </summary>
    private void UpdateBillboard()
    {
        if (signRoot == null || Camera.main == null) return;
        signRoot.rotation = Camera.main.transform.rotation;
    }

    #region Sign 辅助方法

    /// <summary>
    /// 进入问号阶段时调用：显示白问号 + 黄色填充条。
    /// </summary>
    private void ShowQuestionSign()
    {
        if (questionBaseImage != null)
        {
            questionBaseImage.enabled = true;
            questionBaseImage.sprite  = questionSprite;
            questionBaseImage.color   = baseQuestionColor;
        }

        if (signImage != null)
        {
            signImage.enabled = true;
            signImage.sprite  = questionSprite;
            signImage.color   = fillQuestionColor;
            signImage.fillAmount = alarmValue;
        }
    }

    /// <summary>
    /// 问号填满 -> 叹号阶段。
    /// </summary>
    private void ShowExclamationSign()
    {
        if (questionBaseImage != null)
        {
            questionBaseImage.enabled = false;
        }

        if (signImage != null)
        {
            signImage.enabled = true;
            signImage.sprite  = exclamationSprite;
            signImage.color   = exclamationColor;
            signImage.fillAmount = 1f;
        }
    }

    /// <summary>
    /// 完全丢失玩家 -> 隐藏 UI。
    /// </summary>
    private void HideSign()
    {
        if (questionBaseImage != null)
            questionBaseImage.enabled = false;
        if (signImage != null)
            signImage.enabled = false;
    }

    /// <summary>
    /// 问号阶段：更新黄色填充条（覆盖底层白问号）。
    /// </summary>
    private void UpdateQuestionSignVisual()
    {
        if (signImage == null || !signImage.enabled) return;
        if (alarmLevel != 1) return;

        signImage.fillAmount = alarmValue;
        signImage.color      = fillQuestionColor;
    }

    #endregion

#if UNITY_EDITOR
    // 在 Scene 视图中画出视野 & 侦测范围，仅编辑器可见
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectDistance);

        if (!Application.isPlaying)
        {
            // 使用 Handles 画 FOV 只在编辑器可用
            UnityEditor.Handles.color = new Color(1f, 0.8f, 0f, 0.25f);
            Vector3 forward = transform.forward;
            float halfFov = fieldOfView * 0.5f;
            UnityEditor.Handles.DrawSolidArc(
                transform.position,
                Vector3.up,
                Quaternion.Euler(0f, -halfFov, 0f) * forward,
                fieldOfView,
                detectDistance
            );
        }
    }
#endif
}
