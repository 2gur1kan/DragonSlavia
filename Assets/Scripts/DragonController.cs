using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Cinemachine;
using Mirror;
using TMPro;

public class DragonController : NetworkBehaviour
{
    [SerializeField] private float speed = 1f;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private float speedMull = 1f;

    private Joystick joystick; // Joystick referansý
    private Area area;
    private TextMeshProUGUI scoreBoard;
    private Pointer pointer = null;

    public float SpeedMull { set => speedMull = value; }

    public override void OnStartLocalPlayer()
    {
        // Cinemachine kamerasýný bul
        CinemachineVirtualCamera vcam = FindObjectOfType<CinemachineVirtualCamera>();

        if (vcam != null)
        {
            vcam.Follow = transform;
            vcam.LookAt = transform;
        }

        // FireBTN tagli butonu bul
        GameObject fireBtnObj = GameObject.FindGameObjectWithTag("FireBTN");

        if (fireBtnObj != null)
        {
            CustomBTN button = fireBtnObj.GetComponent<CustomBTN>();
            if (button != null)
            {
                button.onDown += TryFire;
                button.onHold += TryFire;
            }
            else Debug.LogError("FireBTN objesinde Button component yok!");
        }
        else Debug.LogError("FireBTN tagli obje bulunamadý!");

        // speed bost ayarlarý
        GameObject speedBoostBtn = GameObject.FindGameObjectWithTag("SpeedBoostBTN");

        if (speedBoostBtn != null)
        {
            CustomBTN button = speedBoostBtn.GetComponent<CustomBTN>();
            if (button != null)
            {
                button.onDown += SpeedBoost;
                button.onUp += SpeedBoostRelease;
            }
            else Debug.LogError("SpeedBoostBTN objesinde Button component yok!");
        }
        else Debug.LogError("SpeedBoostBTN tagli obje bulunamadý!");
    }

    private void Start()
    {
        if (!isLocalPlayer)
        {
            Invoke("CreatePointer", 2f);
            return;
        }
            
        joystick = FindObjectOfType<Joystick>();

        area = FindObjectOfType<Area>();
        if (area == null) Debug.LogError("Area bulunamadý!");
        else InvokeRepeating("CheckPlayerInAreaInvoke", 1f, 1f);

        scoreBoard = GameObject.FindGameObjectWithTag("score").GetComponent<TextMeshProUGUI>();

        scoreBoard.text = score.ToString();

        DragonName = UIManager.Instance.getName;
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        float h = joystick.Horizontal;
        float v = joystick.Vertical;

        Vector3 rotationDelta = new Vector3(-v, h, 0) * rotationSpeed * Time.deltaTime;
        transform.Rotate(rotationDelta, Space.Self);

        transform.position += transform.forward * speed * speedMull * Time.deltaTime;
    }

    [Command]
    public void CmdTeleportMe(Vector3 pos)
    {
        transform.position = pos;
    }

    private void CheckPlayerInAreaInvoke()
    {
        Vector3 newPos = area.GetWrappedPosition(transform.position);

        if (newPos != transform.position)
        {
            transform.position = newPos;
        }
    }

    private void CreatePointer() => pointer = PointersPanelController.Instance.CreateEnemyPointer(transform);

    public void SpeedBoost()
    {
        if (!isLocalPlayer || isDead) return;

        speedMull = 2.5f;
    }
    public void SpeedBoostRelease()
    {
        if (!isLocalPlayer || isDead) return;

        speedMull = 1f;
    }

    ////////////////////////////////////////////////////////////////////////////////////////// fire

    [Header("Fight systems:")]

    [SyncVar(hook = nameof(OnHPChanged))] [SerializeField] private int hp = 10;
    [SyncVar(hook = nameof(OnScoreChanged))] [SerializeField] private int score = 0;
    [SerializeField] private float FireCooldown = .8f;
    [SerializeField] private float DeadCooldown = 5f;

    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform firePoint;
    private bool attackFlag = false;
    [SyncVar(hook = nameof(OnDeathStateChanged))] private bool isDead;

    private void AttackFlagSetter() => attackFlag = false;
    private void OnDeathStateChanged(bool oldValue, bool newValue) => transform.GetChild(0).gameObject.SetActive(!newValue);

    public void TryFire()
    {
        if (!isLocalPlayer || attackFlag || isDead) return;
        CmdFire();
        attackFlag = true;
        Invoke("AttackFlagSetter", FireCooldown);
    }

    [Command]
    void CmdFire()
    {
        GameObject fireball = Instantiate(fireballPrefab, firePoint.position, firePoint.rotation);
        NetworkServer.Spawn(fireball);

        fireball.GetComponent<FireBall>().Launch(firePoint.position, firePoint.rotation, this);
    }

    public bool TakeDamage()
    {
        if (!isServer) return false;

        hp--;

        if (hp < 1) return Dead();

        return false;
    }

    [Server]
    public void addScore(int add = 1)
    {
        if (!isServer) return;

        score += add;
    }

    private void OnHPChanged(int oldHp, int newHp)
    {
        if (!isLocalPlayer) return;

        UIManager.Instance.HitOrDeadPanel(newHp < 1);
    } 
    
    private void OnScoreChanged(int oldScore, int newScore)
    {
        if (!isLocalPlayer) return;

        scoreBoard.text = newScore.ToString();
    }

    private bool Dead()
    {
        isDead = true;

        score -= 5;

        Invoke("RespwanInvoke", DeadCooldown);
        return true;
    }

    private void RespwanInvoke()
    {
        transform.position = area.GetRandomPositionInside();

        hp = 10;

        isDead = false;
    }

    private void OnDestroy()
    {
        if(pointer != null) pointer.Destroy();
    }

    /////////////////////////////////////////////////////////////////////// others

    [SyncVar(hook = nameof(OnDragonNameChanged))]
    public string DragonName;

    private void OnDragonNameChanged(string oldName, string newName)
    {
        Debug.Log($"Ejderha adý deðiþti: {oldName} => {newName}");
    }

    [Command]
    public void CmdChangeDragonName(string newName)
    {
        DragonName = newName;

        if (pointer != null) pointer.setText(DragonName);
    }
}
