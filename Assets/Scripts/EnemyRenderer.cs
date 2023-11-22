using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EnemyRenderer : MonoBehaviour
{
    private bool flashing;
    private SpriteRenderer spriteRenderer;

    [SerializeField] private Sprite deadDeadSprite;
    [SerializeField] private Sprite liveDeadSprite;
    [SerializeField] private Material whiteMaterial;


    private bool flip;
    public bool Flip { get { return flip; } set { flip = value; } }

    void Start()
    {
        spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        int i = 0;
        foreach (Transform child in transform)
        {
            if (i != 0 && i == GetComponent<Enemy>().Health)
            {
                child.gameObject.GetComponent<SpriteRenderer>().flipX = flip;
            }
            i++;
        }
    }

    public void SelectSpriteForHealth()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(i == GetComponent<Enemy>().Health);
        }
    }

    public IEnumerator FlashWhiteCoroutine()
    {
        if (!flashing)
        {
            flashing = true;
            var initialMaterial = spriteRenderer.material;
            foreach (Transform child in transform)
            {
                if (child.gameObject.TryGetComponent(out SpriteRenderer sr))
                    sr.material = whiteMaterial;
            }
            yield return new WaitForSeconds(0.1f);
            foreach (Transform child in transform)
            {
                if (child.gameObject.TryGetComponent(out SpriteRenderer sr))
                    sr.material = initialMaterial;
            }
            flashing = false;
        }
    }

    public void ToggleCorpseSprite(bool val)
    {
        transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = val ? liveDeadSprite : deadDeadSprite;
    }
}
