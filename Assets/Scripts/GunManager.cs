using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class GunManager : MonoBehaviour
{
    private static GunManager instance;
    public static GunManager Instance { get { return instance; } }
    private void Awake()
    {
        if (instance != null && instance != this) Destroy(gameObject);
        else instance = this;
    }
    [SerializeField] private GunData selectedGun;
    private Vector2 gunPosition;
    [HideInInspector] public Vector2 GunPosition { get { return gunPosition; } }
    [SerializeField] private GameObject crosshair;
    [SerializeField] private LineRenderer laser;
    [SerializeField] private LayerMask laserIgnore;
    [SerializeField] private SpriteRenderer gunRenderer;
    [SerializeField] private Transform gunRotator;
    [SerializeField] private Transform gunTip;
    [HideInInspector] public Transform GunTip { get { return gunTip; } }

    [HideInInspector] public SpriteRenderer GunRenderer { get { return gunRenderer; } }

    private float waitTime;
    private bool queuedShoot;
    void Start()
    {
        SwitchGun(selectedGun);
    }

    void Update()
    {
        
        TryShootGun();
        gunRenderer.flipY = PlayerRenderer.Instance.PlayerPosition.x > gunPosition.x;
        gunRenderer.sortingOrder = PlayerRenderer.Instance.PlayerPosition.y > gunPosition.y ? 2 : -2;
        gunPosition = gunRenderer.transform.position;
        if (Vector2.Distance(CameraManager.Instance.LaggedMousePos, gunPosition) < 1) return;
        //Vector3 relativePos = CameraManager.Instance.LaggedMousePos - PlayerRenderer.Instance.PlayerPosition;
        //Quaternion rotation = Quaternion.LookRotation(relativePos);
        //Debug.Log(Quaternion.ToEulerAngles(rotation));
        //Debug.Log($"{gunPosition},{CameraManager.Instance.LaggedMousePos}");
        var rotation = Mathf.Atan2(gunPosition.y - CameraManager.Instance.LaggedMousePos.y, gunPosition.x - CameraManager.Instance.LaggedMousePos.x);
        //Debug.Log($"Degree: #{Mathf.Rad2Deg*test}");
        gunRotator.rotation = Quaternion.Euler(new Vector3(0, 0, 180f + Mathf.Rad2Deg * rotation));
        //rotation.x = 0; rotation.y = 0;
        //gunRotator.rotation = rotation;
        //var angle = Vector3.SignedAngle(gunRotator.position, CameraManager.Instance.LaggedMousePos, transform.forward);
        //gunRotator.Rotate(0.0f, 0.0f, angle);
        //gunRotator.rotation = Quaternion.Euler(new Vector3(0, 0, rotation));

        /*Vector3 targetDirection = CameraManager.Instance.LaggedMousePos - gunPosition;
        Vector3 newDirection = Vector3.RotateTowards(gunRotator.transform.forward, targetDirection, Time.deltaTime, 0.0f);
        gunRotator.rotation = Quaternion.LookRotation(newDirection);*/

        //gunRotator.right = Vector3.RotateTowards(gunRotator.position, CameraManager.Instance.LaggedMousePos, 100f, Mathf.Infinity);

        laser.gameObject.SetActive(selectedGun.Laser);
        if (selectedGun.Laser)
        {
            var pointer = (CameraManager.Instance.LaggedMousePos - (Vector2)gunTip.position).normalized;
            var hit = Physics2D.Raycast(gunTip.position, pointer, Mathf.Infinity, ~laserIgnore);
            laser.SetPosition(0, gunTip.position);
            laser.SetPosition(1, hit.point);
        }
    }

    public void TryShootGun()
    {
        if (waitTime >= 0) waitTime -= Time.deltaTime;
        if (Input.GetMouseButtonDown(0) || queuedShoot)
        {
            if (waitTime <= 0) StartCoroutine(nameof(ShootGun));
            else queuedShoot = true;
        }
    }

    private IEnumerator ShootGun()
    {
        //yield return null;
        waitTime = selectedGun.ReloadTime;
        queuedShoot = false;
        for (int i = 0; i < selectedGun.Repetitions +1; i++)
        {
            for (int j = 0; j < selectedGun.BulletsPerShot; j++)
            {
                var b = Instantiate(selectedGun.Bullet, gunTip.position, Quaternion.identity).GetComponent<Rigidbody2D>();
                var dif = (CameraManager.Instance.LaggedMousePos - (Vector2)GunTip.position).normalized;
                //bodyRigid.AddForce(dif * multiplier * Time.deltatime);
                dif = HelperClass.RotateVector(dif, Random.Range(-selectedGun.Inaccuracy, selectedGun.Inaccuracy));
                b.AddForce(dif * 800);
                CameraManager.Instance.ShakeCamera();
            }
            yield return new WaitForSeconds(selectedGun.DelayBetween);
           
        }
    }

    private void FixedUpdate()
    {
        crosshair.transform.position = CameraManager.Instance.LaggedMousePos;
    }

    private void SwitchGun(GunData newGun)
    {
        selectedGun = newGun;
        gunRenderer.sprite = selectedGun.GunSprite;
        gunRenderer.transform.localPosition = selectedGun.GunOffset;
        gunRenderer.transform.GetChild(0).localPosition = selectedGun.TipOffset;
        waitTime = selectedGun.ReloadTime;
    }
}
