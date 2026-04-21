using UnityEngine;
using TMPro;
using UnityEngine.Events;
using System;

public class UI_Timer : MonoBehaviour
{
    public static UI_Timer Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Mode")]
    [SerializeField] private bool useCountdown = false;
    [SerializeField] private float countdownStartTime = 60f;

    [Header("Events")]
    public UnityEvent OnTimeOut;          // Inspector binding
    public event Action OnTimeOutEvent;   // Code binding

    private float currentTime = 0f;
    private bool isRunning = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ResetTimer();
        UpdateDisplay();
    }

    private void Update()
    {
        if (!isRunning) return;

        if (useCountdown)
        {
            currentTime -= Time.deltaTime;

            if (currentTime <= 0f)
            {
                currentTime = 0f;
                isRunning = false;
                TriggerTimeOut();
            }
        }
        else
        {
            currentTime += Time.deltaTime;
        }

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);

        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    private void TriggerTimeOut()
    {
        Debug.Log("TIMEOUT");
        OnTimeOut?.Invoke();     // Inspector listeners
        OnTimeOutEvent?.Invoke(); // Code listeners
    }

    // -------- PUBLIC API --------

    public void ResetTimer()
    {
        currentTime = useCountdown ? countdownStartTime : 0f;
        isRunning = true;
    }

    public void StartCountdown(float startTime)
    {
        useCountdown = true;
        countdownStartTime = startTime;
        ResetTimer();
    }

    public void StartCountUp()
    {
        useCountdown = false;
        ResetTimer();
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public float GetTime()
    {
        return currentTime;
    }

    // -------- EVENT BIND HELPERS --------

    public void AddTimeOutListener(Action listener)
    {
        OnTimeOutEvent += listener;
    }

    public void RemoveTimeOutListener(Action listener)
    {
        OnTimeOutEvent -= listener;
    }
}