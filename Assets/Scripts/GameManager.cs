using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// Счёт, переходы Roll/Frame, ресеты мяча и кеглей.
public class GameManager : MonoBehaviour
{
    public static GameManager I;

    [Header("Ссылки (BowlingSetup поставит сам)")]
    public List<Pin> pins;
    public BallLauncher ball;
    public Transform ballSpawnPoint;
    public TextMeshProUGUI scoreText;

    int[] rolls = new int[21];   // максимум 21 бросок
    int rollPtr;

    int frame = 1;  
    int rollInFrame = 1;  
    int pinsStandingAtStart = 10;
    int lastRollPins;       // кегли за предыдущий бросок
    string lastRollLabel = ""; // "", "Strike!", "Spare!"
    int pinsKnockedThisFrame;

    void Awake() => I = this;
    void Start() => UpdateUI();

    public void OnBallStopped()
    {
        int standingNow = CountStandingPins();
        int knocked = pinsStandingAtStart - standingNow;

        pinsKnockedThisFrame += knocked;
        rolls[rollPtr++] = knocked;
        lastRollPins = knocked;          
        lastRollLabel = "";
        pinsStandingAtStart = standingNow;

        bool strike = rollInFrame == 1 && knocked == 10;
        bool secondRoll = rollInFrame == 2;

        bool endFrame = strike || secondRoll;

        if (rollInFrame == 1 && knocked == 10)          
        {
            lastRollLabel = "Strike";
        }
        else if (rollInFrame == 2 && pinsKnockedThisFrame == 10)
        {
            lastRollLabel = "Spare";
        }

        if (strike || secondRoll)          // конец фрейма
        {
            pinsKnockedThisFrame = 0;
            frame++;
            rollInFrame = 1;
            pinsStandingAtStart = 10;
            StartCoroutine(WaitPinsThenReset(endFrame));
        }
        else                               // Roll 2 того же фрейма
        {
            rollInFrame = 2;
        }

        StartCoroutine(WaitPinsThenReset(endFrame));
        UpdateUI();
    }

    IEnumerator WaitPinsThenReset(bool endOfFrame)
    {
        // ждём, пока скорость всех кеглей < 0.05 или прошло 3 с.
        float t = 0;
        while (t < 3f)
        {
            bool moving = false;
            foreach (var p in pins)
                if (p.GetComponent<Rigidbody>().velocity.sqrMagnitude > 0.0025f)
                { moving = true; break; }

            if (!moving) break;
            t += Time.deltaTime;
            yield return null;
        }

        if (endOfFrame) ResetPins();   // поднять все кегли
        ResetBall();                   // телепорт шара
    }

    int CountStandingPins()
    {
        int standing = 0;
        foreach (var p in pins) if (!p.IsKnocked()) standing++;
        return standing;
    }

    void ResetBall() =>
        ball.HardReset(ballSpawnPoint.position, ballSpawnPoint.rotation);

    void ResetPins()
    {
        foreach (var p in pins) p.ResetPose();
    }

    void UpdateUI()
    {
        if (scoreText == null) return;

        scoreText.text =
            $"Игра: {frame}\n" +
            $"Ход: {rollInFrame}/2\n" +
            $"Общий счет: {CalculateScore()}\n" +
            $"Посл. удар: {lastRollPins}\n" +
            $"{lastRollLabel}";
    }

    int CalculateScore()
    {
        int score = 0, r = 0;
        for (int f = 0; f < 10; f++)
        {
            if (rolls[r] == 10)                     // strike
            {
                score += 10 + rolls[r + 1] + rolls[r + 2];
                r += 1;
            }
            else if (rolls[r] + rolls[r + 1] == 10) // spare
            {
                score += 10 + rolls[r + 2];
                r += 2;
            }
            else                                    // обычный 
            {
                score += rolls[r] + rolls[r + 1];
                r += 2;
            }
        }
        return score;
    }
}
