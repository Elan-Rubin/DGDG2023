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
    [HideInInspector] public GunData SelectedGun { get { return selectedGun; } }
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

        gunRenderer.flipY = PlayerMovement.Instance.PlayerPosition.x > gunPosition.x;
        gunRenderer.sortingOrder = PlayerMovement.Instance.PlayerPosition.y > gunPosition.y ? 3 : 1;
        gunPosition = gunRenderer.transform.position;
        laser.gameObject.SetActive(selectedGun.Laser);
        if (selectedGun.Laser)
        {
            var pointer = (CameraManager.Instance.LaggedMousePos - (Vector2)gunTip.position).normalized;
            var hit = Physics2D.Raycast(gunTip.position, pointer, Mathf.Infinity, ~laserIgnore);
            laser.SetPosition(0, gunTip.position);
            laser.SetPosition(1, hit.point);
        }
        if (Vector2.Distance(CameraManager.Instance.LaggedMousePos, gunPosition) < 1)
        {
            laser.gameObject.SetActive(false);
        }
        else
        {
            var rotation = Mathf.Atan2(gunPosition.y - CameraManager.Instance.LaggedMousePos.y, gunPosition.x - CameraManager.Instance.LaggedMousePos.x);
            gunRotator.rotation = Quaternion.Euler(new Vector3(0, 0, 180f + Mathf.Rad2Deg * rotation));

            TryShootGun();
        }
    }

    public void TryShootGun()
    {
        if (waitTime >= 0) waitTime -= Time.deltaTime;
        if (Input.GetMouseButtonDown(0) || queuedShoot || (selectedGun.Machinegun && Input.GetMouseButton(0)))
        {
            if (waitTime <= 0) StartCoroutine(nameof(ShootGun));
            else queuedShoot = true;
        }
    }

    private IEnumerator ShootGun()
    {
        waitTime = selectedGun.ReloadTime;
        queuedShoot = false;
        for (int i = 0; i < selectedGun.Repetitions + 1; i++)
        {
            for (int j = 0; j < selectedGun.BulletsPerShot; j++)
            {
                var b = Instantiate(selectedGun.Bullet, gunTip.position, Quaternion.identity).GetComponent<Rigidbody2D>();
                var bullet = b.GetComponent<Bullet>();
                bullet.StartLifetime(selectedGun.LifetimeVariation ? selectedGun.BulletLifeTime : (selectedGun.BulletLifeTime * Random.Range(0.5f, 2f)));
                var dif = (CameraManager.Instance.LaggedMousePos - (Vector2)GunTip.position).normalized;
                //bodyRigid.AddForce(dif * multiplier * Time.deltatime);
                dif = HelperClass.RotateVector(dif, Random.Range(-selectedGun.Inaccuracy, selectedGun.Inaccuracy));
                bullet.Velocity = dif.normalized;
                b.AddForce(100 * selectedGun.ShootForce * dif);
                CameraManager.Instance.ShakeCamera();
            }
            yield return new WaitForSeconds(selectedGun.DelayBetween);

        }
    }

    private void FixedUpdate()
    {
        crosshair.transform.position = CameraManager.Instance.LaggedMousePos;
    }

    public void SwitchGun(GunData newGun)
    {
        selectedGun = newGun;
        gunRenderer.sprite = selectedGun.GunSprite;
        gunRenderer.transform.localPosition = selectedGun.GunOffset;
        gunRenderer.transform.GetChild(0).localPosition = selectedGun.TipOffset;
        waitTime = selectedGun.ReloadTime;
    }
}
