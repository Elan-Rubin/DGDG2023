using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.ParticleSystem;

public class Ghost : MonoBehaviour
{
    private Vector2 currentPosition, targetPosition, prevTargetPosition;
    float timer, timer2, timer3;
    Image circle;
    TextMeshProUGUI text;
    bool caught, killed;
    [SerializeField] private Material whiteMaterial;

    float catchTime = 0.75f;

    [SerializeField] private GameObject particlePrefab; 


    void Start()
    {
        currentPosition = targetPosition = transform.position;
        timer2 = Random.Range(0, 1);
        circle = transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>();
        text = transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (killed) return;

        timer -= Time.deltaTime;
        if(!caught) timer2 += Time.deltaTime / 3f;
        if (timer < 0 && !caught)
        {
            //var dif = PlayerMovement.Instance.PlayerPosition - currentPosition;
            currentPosition = transform.position;
            targetPosition = Vector2.MoveTowards(currentPosition, PlayerMovement.Instance.PlayerPosition, -1);
            targetPosition += new Vector2(Random.Range(-1, 1), Random.Range(-1, 1)) / 2f;
            transform.GetChild(0).GetComponent<SpriteRenderer>().flipX = targetPosition.x < currentPosition.x;
            timer = Random.Range(1f, 3f);
        }

        text.gameObject.SetActive(caught || timer3 > 0);
        if (!caught) transform.position = currentPosition = Vector2.Lerp(currentPosition, targetPosition, Time.deltaTime * 3f);
        if (!caught) transform.GetChild(0).transform.localPosition = Vector2.up * Mathf.Sin(timer2 * 360f * Mathf.Deg2Rad);
        if (!caught && timer3 > 0)
        {
            timer3 -= Time.deltaTime;
            circle.fillAmount = timer3 / catchTime;
            text.text = $"{(int)(timer3 / catchTime * 100)}";
        }
        if (Vector2.Distance(transform.position, CameraManager.Instance.LaggedMousePos) < 1f)
        {
            if (caught)
            {
                if (timer3 < catchTime) timer3 += Time.deltaTime;
                circle.fillAmount = timer3 / catchTime;
                text.text = $"{(int)(timer3 / catchTime * 100)}";
                if (timer3 > catchTime) StartCoroutine(nameof(CatchAnimation));
                //targetPosition = PlayerMovement.Instance.PlayerPosition;
            }

            caught = true;
        }
        else if (caught)
        {
            caught = false;
        }
    }
    private void CatchAnimation() => StartCoroutine(nameof(CatchAnimationCoroutine));
    private IEnumerator CatchAnimationCoroutine()
    {
        killed = true;
        yield return null;
        text.gameObject.SetActive(false);

        transform.GetChild(0).GetComponent<SpriteRenderer>().material = whiteMaterial;

        bool anim = false;

        var s = DOTween.Sequence();
        s.Append(circle.DOColor(Color.white, 0.2f));
        s.Append(circle.transform.DOScale(Vector2.zero, 0.2f));
        s.Append(transform.DOScale(Vector2.zero, 0.2f)).OnComplete(() =>
        {
            anim = true;
        });
        var p = Instantiate(particlePrefab, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
        var timer4 = 0f;
        while (timer4 < 1f)
        {
            timer4 += Time.deltaTime/5f;

            var particles = new ParticleSystem.Particle[p.particleCount];
            var target = PlayerMovement.Instance.PlayerPosition;

            p.GetParticles(particles);

            for (int i = 0; i < particles.GetUpperBound(0); i++)
            {

                //float ForceToAdd = (particles[i].startLifetime - particles[i].remainingLifetime) * (10 * Vector2.Distance(Target.position, particles[i].position));

                //Debug.DrawRay (particles [i].position, (Target.position - particles [i].position).normalized * (ForceToAdd/10));

                //particles[i].velocity = (PlayerMovement.Instance.PlayerPosition - (Vector2)particles[i].position).normalized * ForceToAdd;

                particles [i].position = Vector3.Slerp (particles [i].position, target, timer4);

            }

            p.SetParticles(particles, particles.Length);

            yield return null;
        }
        Destroy(p.gameObject);
        Destroy(gameObject);
    }
}
