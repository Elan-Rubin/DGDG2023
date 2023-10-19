using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GunChest : MonoBehaviour
{
    [SerializeField] private GunData storedGun;
    [HideInInspector] public GunData StoredGun { get { return storedGun; } }
    private SpriteRenderer sr, sr2;
    [SerializeField] private Sprite ghostSprite;
    [SerializeField] private TextMeshProUGUI nameText, descriptionText;
    private GunData previousStoredGun;
    private Animator animator;
    private bool open;

    private bool dead;

    void Start()
    {
        animator = GetComponent<Animator>();
        sr = transform.GetChild(0).GetComponent<SpriteRenderer>();
        sr2 = transform.GetChild(1).GetComponent<SpriteRenderer>();
        SwitchGun(storedGun, false);
        GameManager.Instance.PlayerDeath += Switch;
    }

    void Update()
    {
        var previousOpen = open;
        open = Vector2.Distance(transform.position, PlayerMovement.Instance.PlayerPosition) < 4;

        if (open != previousOpen)
        {
            animator.SetTrigger("change");
            animator.SetBool("powered", open);
        }

        //chestRenderer.sprite = open ? openSprite : closedSprite;
        nameText.gameObject.SetActive(open);
        descriptionText.gameObject.SetActive(open);
    }
    public void SwitchGun(GunData newGun) => SwitchGun(newGun, true);
    void SwitchGun(GunData newGun, bool makeSwitch)
    {
        if (makeSwitch) GunManager.Instance.SwitchGun(storedGun);
        previousStoredGun = storedGun;
        storedGun = newGun;
        nameText.text = storedGun.GunName;
        descriptionText.text = storedGun.GunDescription;
    }
    private void Switch()//ten seconds
    {
        StartCoroutine(nameof(SwitchCoroutine));
    }
    private IEnumerator SwitchCoroutine()
    {
        sr.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
        sr2.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

        yield return new WaitForSeconds(5f);
        sr.gameObject.SetActive(false);
        sr2.maskInteraction = SpriteMaskInteraction.None;
    }
}
