using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GunChest : MonoBehaviour
{
    [SerializeField] private GunData storedGun;
    [HideInInspector] public GunData StoredGun { get { return storedGun; } }
    private SpriteRenderer chestRenderer;
    [SerializeField] private Sprite closedSprite, openSprite;
    [SerializeField] private TextMeshProUGUI nameText, descriptionText;
    private GunData previousStoredGun;
    private Animator animator;
    private bool open;
    void Start()
    {
        animator = GetComponent<Animator>();
        chestRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        SwitchGun(storedGun, false);
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
}
