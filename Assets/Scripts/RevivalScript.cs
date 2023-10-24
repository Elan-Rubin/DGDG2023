using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.U2D.Animation;
using UnityEngine;
using UnityEngine.InputSystem.Android;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class RevivalScript : MonoBehaviour
{
    private List<Vector2> positionsList = new();
    private Vector2 latestPos;
    [SerializeField] private LineRenderer ghostLine;
    [SerializeField] private GameObject mask;
    [HideInInspector] public GameObject Mask;
    [SerializeField] private LineRenderer revivalLine;
    [SerializeField] private int minimumDistance = 1;
    [HideInInspector] public int MinimumDistance { get { return minimumDistance; } }

    public int GhostCounter;
    public int GhostThreshold;


    [SerializeField] private GameObject ghostCanvas;
    private TextMeshProUGUI ghostText;
    private List<Slider> ghostSliders = new List<Slider>();

    bool dead;
    public bool Dead { get { return dead; } }

    private static RevivalScript instance;
    public static RevivalScript Instance { get { return instance; } }

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(gameObject);
        else instance = this;
    }

    void Start()
    {
        GhostThreshold = 3;
        mask.SetActive(false);
        GameManager.Instance.PlayerDeath += Rewind;
        GameManager.Instance.PlayerReborn += Reborn;
        ghostText = ghostCanvas.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        ghostSliders.Add(ghostCanvas.transform.GetChild(1).GetComponent<Slider>());
    }

    void Update()
    {
        //remember to remove this later
        if (Input.GetKeyDown(KeyCode.Space)) Rewind();

        if (dead)
        {
            var ppos = PlayerMovement.Instance.PlayerPosition;
            var campos = CameraManager.Instance.LaggedMousePos;
            ghostLine.SetPosition(0, Vector2.Lerp(ppos, campos, 0.15f));
            ghostLine.SetPosition(1, Vector2.Lerp(ppos, campos, 0.9f));
        }

        int current = 0;
        foreach (Slider ghostSlider in ghostSliders)
        {
            if (current == ghostSliders.Count - 1)
                ghostSlider.value = (float)(GhostCounter % GhostThreshold) / GhostThreshold;
            else
                ghostSlider.value = 1;
            current++;
        }
    }

    public void UpdateCatches()
    {
        Slider newSlider = null;
        if (GhostCounter > GhostThreshold * ghostSliders.Count)
        {
            var g = Instantiate(ghostSliders[ghostSliders.Count-1], ghostSliders[ghostSliders.Count-1].transform.parent).GetComponent<Slider>();
            g.transform.position = g.transform.position - Vector3.up;
            newSlider = g.GetComponent<Slider>();
            ghostSliders.Add(newSlider);
        }
        if (newSlider != null) ghostSliders[ghostSliders.Count-1] = newSlider;

        ghostText.text = $"{GhostCounter} of {GhostThreshold}";

        var gcNearest = Mathf.Clamp(Mathf.FloorToInt((float)GhostCounter / GhostThreshold), 0, 10);
        var gcNew = GhostCounter - (gcNearest * GhostThreshold);

        ghostSliders[ghostSliders.Count - 1].DOValue((float)gcNew / GhostThreshold, 0.1f).OnComplete(() =>
          {
              if (newSlider != null) ghostSliders[ghostSliders.Count - 1] = newSlider;
          });

        ghostText.transform.DOPunchScale(Vector2.one * 0.3f, 0.2f);
    }

    public Vector2 FirstPosition()
    {
        return positionsList[0];
    }

    public void AddPosition(Vector2 newPosition)
    {
        positionsList.Add(latestPos = new Vector2(Mathf.Floor(newPosition.x) + 0.5f, Mathf.Floor(newPosition.y) + .5f));
    }
    public Vector2 GetLatest()
    {
        return latestPos;
    }
    public void Rewind()
    {
        dead = true;
        ghostCanvas.SetActive(true);
        ghostLine.gameObject.SetActive(true);
        revivalLine.gameObject.SetActive(true);
        StartCoroutine(nameof(RewindCoroutine));
    }

    public void Reborn()
    {
        StartCoroutine(nameof(RebornCoroutine));
    }

    private IEnumerator RewindCoroutine()
    {
        var t1 = GameManager.Instance.Lower;
        var t2 = GameManager.Instance.Upper;

        t1.SetActive(true);

        var t1tm = t1.transform.GetChild(0).GetComponent<TilemapRenderer>();
        var t2tm = t2.transform.GetChild(0).GetComponent<TilemapRenderer>();

        t1tm.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        t2tm.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;

        PlayerMovement.Instance.SnapPosition(FirstPosition());
        positionsList.RemoveAt(0);

        mask.SetActive(true);

        mask.transform.localScale = Vector2.zero;

        mask.transform.DOScale(Vector2.one * 50, 1.25f).SetEase(Ease.InCirc);

        var sr = mask.transform.GetChild(0).GetComponent<SpriteRenderer>();
        sr.color = Color.clear;
        var sequence = DOTween.Sequence();
        sequence.Append(sr.DOColor(Color.white, .5f)).OnComplete(() => sr.DOColor(new Color(97 / 255f, 224 / 255f, 135 / 255f), .5f));
        //sequence.Append(sr.DOColor(new Color(97, 224, 135), 2f));

        var ps = mask.transform.GetChild(1).GetComponent<ParticleSystem>();
        var sh = ps.shape;
        var m = ps.main;
        var e = ps.emission;
        var counter = 0f;
        var alr = false;
        DOTween.To(() => counter, x => counter = x, 50, 1.25f).SetEase(Ease.InCirc)
            .OnUpdate(() =>
            {
                if (!alr && counter > 1)
                {
                    alr = true;
                    ps.Play();
                }

                sh.radius = .8f + (counter / 2f);
                //sh.radiusThickness = counter * 0.2f;
                m.startSpeed = -(Mathf.Pow(3f, (counter / 25f)));
                e.rateOverTime = 30 * (int)counter;
            }).OnComplete(() =>
            {
                t1tm.maskInteraction = SpriteMaskInteraction.None;
                t2tm.maskInteraction = SpriteMaskInteraction.None;
                t2.SetActive(false);
                StartCoroutine(nameof(DeathGameplay));
            });
        yield return null;
    }


    private IEnumerator RebornCoroutine()
    {
        var t1 = GameManager.Instance.Upper;
        var t2 = GameManager.Instance.Lower;

        t1.SetActive(true);

        var t1tm = t1.transform.GetChild(0).GetComponent<TilemapRenderer>();
        var t2tm = t2.transform.GetChild(0).GetComponent<TilemapRenderer>();

        t1tm.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        t2tm.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;

        mask.SetActive(true);

        mask.transform.localScale = Vector2.zero;

        mask.transform.DOScale(Vector2.one * 50, 1.25f).SetEase(Ease.InCirc);

        var sr = mask.transform.GetChild(0).GetComponent<SpriteRenderer>();
        sr.color = Color.clear;
        var sequence = DOTween.Sequence();
        sequence.Append(sr.DOColor(Color.white, .5f)).OnComplete(() => sr.DOColor(new Color(97 / 255f, 224 / 255f, 135 / 255f), .5f));
        //sequence.Append(sr.DOColor(new Color(97, 224, 135), 2f));

        var ps = mask.transform.GetChild(1).GetComponent<ParticleSystem>();
        var sh = ps.shape;
        var m = ps.main;
        var e = ps.emission;
        var counter = 0f;
        var alr = false;
        DOTween.To(() => counter, x => counter = x, 50, 1.25f).SetEase(Ease.InCirc)
            .OnUpdate(() =>
            {
                if (!alr && counter > 1)
                {
                    alr = true;
                    ps.Play();
                }

                sh.radius = .8f + (counter / 2f);
                //sh.radiusThickness = counter * 0.2f;
                m.startSpeed = -(Mathf.Pow(3f, (counter / 25f)));
                e.rateOverTime = 30 * (int)counter;
            }).OnComplete(() =>
            {
                t1tm.maskInteraction = SpriteMaskInteraction.None;
                t2tm.maskInteraction = SpriteMaskInteraction.None;
                t2.SetActive(false);
            });
        yield return null;
    }

    private IEnumerator DeathGameplay()
    {
        revivalLine.positionCount = positionsList.Count;
        for (int i = 0; i < positionsList.Count; i++)
        {
            revivalLine.SetPosition(i, positionsList[i]);
        }


        yield return new WaitForSeconds(0.1f);
        int iterator = 0;
        while (positionsList.Count > 0)
        {
            PlayerMovement.Instance.ForceMovePlayer(positionsList[0]);
            PlayerMovement.Instance.CanMove = false;

            positionsList.RemoveAt(0);
            yield return new WaitForSeconds(0.3f);

            /*for (int i = 0; i < iterator; i++)
            {
                revivalLine.SetPosition(i, positionsList[0]);
            }*/
            iterator++;
        }
        PlayerMovement.Instance.CanMove = true;
        revivalLine.gameObject.SetActive(false);
        GhostSpawner.Instance.RevivalPathEnded();
    }
}
