using TMPro;
using UnityEngine;

public class UI_Timer : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI timerText;
    private float timeElapsed = 0f;


    void Start()
    {
        timerText.text = "00:00";
    }


    void Update()
    {
        timeElapsed += Time.deltaTime;

        int minutes = Mathf.FloorToInt(timeElapsed / 60f);
        int seconds = Mathf.FloorToInt(timeElapsed % 60f);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
