using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// Генерирует дорожку, бортики, кегли, шар.  
public class BowlingSetup : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject lanePrefab;
    public GameObject gutterPrefab;
    public GameObject pinPrefab;
    public GameObject ballPrefab;

    [Header("Lane")]
    public float laneWidth = 4f;
    public float laneLength = 25f;
    public float gutterThickness = 0.5f;
    public Vector3 laneOrigin = Vector3.zero;

    [Header("Pins")]
    public float pinSpacing = 0.6f;

    [Header("Ball")]
    public float ballSpawnZ = -2f;

    void Awake()
    {
        BuildLane();
        BuildGutters();
        List<Pin> pinList = BuildPins();
        (BallLauncher ball, Transform spawn) = BuildBall();

        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.pins = pinList;
            gm.ball = ball;
            gm.ballSpawnPoint = spawn;
        }
    }

    // ---------- построение объектов ----------
    void BuildLane()
    {
        GameObject laneGO = Instantiate(lanePrefab, laneOrigin, Quaternion.identity);
    }


    void BuildGutters()
    {
        Vector3 left = laneOrigin + new Vector3(-(laneWidth / 2f + gutterThickness / 2f), 0, 0);
        Vector3 right = laneOrigin + new Vector3((laneWidth / 2f + gutterThickness / 2f), 0, 0);

        GameObject l = Instantiate(gutterPrefab, left, Quaternion.identity);
        GameObject r = Instantiate(gutterPrefab, right, Quaternion.identity);

        l.transform.localScale = new Vector3(gutterThickness, 1, laneLength);
        r.transform.localScale = new Vector3(gutterThickness, 1, laneLength);
    }

    List<Pin> BuildPins()
    {
        var list = new List<Pin>();
        int[] rows = { 4, 3, 2, 1 };          // задний→передний

        float laneEndZ = laneOrigin.z + laneLength * 0.5f;

        // ставим последний ряд вплотную, оставив символические 2 см
        float lastRowZ = laneEndZ - 0.02f;

        for (int row = 0; row < rows.Length; row++)
        {
            int count = rows[row];
            float rowZ = lastRowZ - row * pinSpacing;
            float totalW = (count - 1) * pinSpacing;

            for (int i = 0; i < count; i++)
            {
                float x = -totalW / 2f + i * pinSpacing;      // центрируем ряд
                Vector3 pos = new Vector3(x, 0, rowZ);

                list.Add(Instantiate(pinPrefab, pos, Quaternion.identity)
                         .GetComponent<Pin>());
            }
        }
        return list;
    }

    (BallLauncher, Transform) BuildBall()
    {
        Vector3 spawnPos = laneOrigin + new Vector3(0, 0.15f, ballSpawnZ);
        var spawnGO = new GameObject("BallSpawnPoint");
        spawnGO.transform.position = spawnPos;

        GameObject ballGO = Instantiate(ballPrefab, spawnPos, Quaternion.identity);
        return (ballGO.GetComponent<BallLauncher>(), spawnGO.transform);
    }
}
