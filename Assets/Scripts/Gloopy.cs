using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gloopy : MonoBehaviour
{
    [SerializeField] private List<Sprite> sprites = new();
    void Start()
    {
        transform.Rotate(new Vector3(0, 0, Random.Range(0, 3) * 90));
        GetComponent<SpriteRenderer>().sprite = sprites[Random.Range(0, sprites.Count)];
        //transform.position = new Vector2((int)transform.position.x, (int)transform.position.y);
        var sequence = DOTween.Sequence();
        sequence.AppendInterval(Random.Range(4,6));
        sequence.Append(GetComponent<SpriteRenderer>().DOColor(Color.clear, 0.5f)).OnComplete(() => Destroy(gameObject));
    }

    void Update()
    {
        
    }
}
