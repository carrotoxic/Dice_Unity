using System.Collections;
using UnityEngine;
using TMPro;

public class DiceManager : MonoBehaviour
{
    [SerializeField] private Dice[] dice;
    [SerializeField] private TextMeshProUGUI resultUI;

    public void RollAll()
    {
        // Roll each die
        foreach (var d in dice) d.Roll();

        // Optionally wait and then show results
        StartCoroutine(ShowResultsWhenDone());
    }

    private IEnumerator ShowResultsWhenDone()
    {
        // wait until all dice stop
        while (true)
        {
            bool anyRolling = false;
            foreach (var d in dice)
                anyRolling |= d.IsRolling;

            if (!anyRolling) break;
            yield return null;
        }

        if (resultUI != null)
        {
            resultUI.text = $"{dice[0].CurrentValue}, {dice[1].CurrentValue}, {dice[2].CurrentValue}";
        }
    }
}