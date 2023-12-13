using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText;
    public TextMeshProUGUI HealthText { get { return healthText; } }
    [SerializeField] private Slider healthSlider;
    public Slider HealthSlider { get { return healthSlider; } }
    [SerializeField] private TextMeshProUGUI gunNameText;
    public TextMeshProUGUI GunNameText { get { return gunNameText; } }
    [SerializeField] private Slider gunSlider;
    public Slider GunSlider { get { return gunSlider; } }
    [SerializeField] private Image gunImage;
    public Image GunImage { get { return gunImage; } }
    private static UIManager instance;
    public static UIManager Instance { get { return instance; } }
    private void Awake()
    {
        if (instance != null && instance != this) Destroy(gameObject);
        else instance = this;
    }
    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
