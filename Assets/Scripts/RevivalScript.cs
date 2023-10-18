using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D.Animation;
using UnityEngine;
using UnityEngine.InputSystem.Android;

public class RevivalScript : MonoBehaviour
{
    public float TimeLeft;
    public bool TimerOn = false;

    public GameObject TimerText;

    private List<Vector2> positionsList = new();
    private Vector2 latestPos;
    [SerializeField] private GameObject mask;
    [SerializeField] private LineRenderer revivalLine;
    [SerializeField] private int minimumDistance = 1;
    [HideInInspector] public int MinimumDistance { get { return minimumDistance; } }
    private static RevivalScript instance;
    public static RevivalScript Instance { get { return instance; } }
    private void Awake()
    {
        if (instance != null && instance != this) Destroy(gameObject);
        else instance = this;
    }
    void Start()
    {
        mask.SetActive(false);
        TimerOn = true;
    }

    void Update()
    {
        if (TimerOn)
        {
            if (TimeLeft > 0)
            {
                TimeLeft -= Time.deltaTime;
            }
            else
            {
                TimeLeft = 0;
                TimerOn = false;
            }
        }


        //remember to remove this later
        if (Input.GetKeyDown(KeyCode.Space)) Rewind();
    }

    public Vector2 FirstPosition()
    {
        return positionsList[0];
    }

    public void AddPosition(Vector2 newPosition)
    {
        positionsList.Add(latestPos = new Vector2((int)newPosition.x, (int)newPosition.y));
    }
    public Vector2 GetLatest()
    {
        return latestPos;
    }
    public void Rewind()
    {
        revivalLine.gameObject.SetActive(true);
        PlayerMovement.Instance.CanMove = false;
        StartCoroutine(nameof(RewindCoroutine));
    }

    private IEnumerator RewindCoroutine()
    {
        PlayerMovement.Instance.PlayerPosition = FirstPosition();


        mask.SetActive(true);
        mask.transform.localScale = Vector2.zero;
        mask.transform.DOScale(Vector2.one * 25, 10f).SetEase(Ease.Linear);

        var ps = mask.transform.GetChild(1).GetComponent<ParticleSystem>();
        var sh = ps.shape;
        var m = ps.main;
        var e = ps.emission;
        var counter = 0f;
        DOTween.To(() => counter, x => counter = x, 25, 10f).SetEase(Ease.Linear)
            .OnUpdate(() =>
            {
                sh.radius = .8f + (counter / 2f);
                //sh.radiusThickness = counter * 0.2f;
                m.startSpeed = -(Mathf.Pow(3f, (counter / 25f)));
                e.rateOverTime = 30 * (int)counter;
            });

        revivalLine.positionCount = positionsList.Count;
        for (int i = 0; i < positionsList.Count; i++)
        {
            revivalLine.SetPosition(i, positionsList[i]);
        }


        yield return new WaitForSeconds(0.1f);
        while (positionsList.Count > 0)
        {
            PlayerMovement.Instance.ForceMovePlayer(positionsList[0]);
            positionsList.RemoveAt(0);
            yield return new WaitForSeconds(0.25f);
        }
        PlayerMovement.Instance.CanMove = true;
        revivalLine.gameObject.SetActive(false);
    }

    void UpdateTimer(float currentTime)
    {
        currentTime += 1;

        float minutes = Mathf.FloorToInt(currentTime / 60);
        float seconds = Mathf.FloorToInt(currentTime % 60);

        string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
