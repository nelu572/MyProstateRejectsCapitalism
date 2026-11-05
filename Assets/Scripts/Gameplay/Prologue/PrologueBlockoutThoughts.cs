using MyProstateRejectsCapitalism.Characters;
using TMPro;
using UnityEngine;

namespace MyProstateRejectsCapitalism.Gameplay
{
    // Only the independent first-route blockout uses this timing experiment.
    public sealed class PrologueBlockoutThoughts : MonoBehaviour
    {
        [SerializeField] private FirstPersonPlayer player;
        [SerializeField] private TMP_Text subtitle;
        [SerializeField] private BoxCollider firstTurn;

        private float observationSeconds;
        private float remaining;
        private int nextThought;

        private void Awake()
        {
            subtitle.text = string.Empty;
        }

        private void Update()
        {
            if (!player.CanControl)
                return;

            observationSeconds += Time.deltaTime;
            if (remaining > 0f)
            {
                remaining -= Time.deltaTime;
                if (remaining <= 0f)
                    subtitle.text = string.Empty;
                return;
            }

            if (nextThought == 0 && observationSeconds >= 8f)
                Show("눈뜨니까 여기였는데… 어떻게 온 거지.", 5f);
            else if (nextThought == 1 && observationSeconds >= 26f)
                Show("화장실 찾은 지 삼십 분은 된 것 같은데.", 5f);
            else if (nextThought == 2 && observationSeconds >= 44f
                     && firstTurn.bounds.Contains(player.transform.position + Vector3.up))
                Show("아… 더는 못 참겠는데.", 4f);
        }

        private void Show(string text, float seconds)
        {
            subtitle.text = text;
            remaining = seconds;
            nextThought++;
        }
    }
}
