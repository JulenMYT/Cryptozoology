using UnityEngine;
using UnityEngine.UI;

public class UIPause : MonoBehaviour
{
    [SerializeField] private Image border;
    [SerializeField] private Image pauseIcon;

    private void Start()
    {
        Resume();

        GameManager.Instance.Pause.OnPause += Pause;
        GameManager.Instance.Pause.OnResume += Resume;
    }

    private void Pause()
    {
        border.gameObject.SetActive(true);
        pauseIcon.gameObject.SetActive(true);
    }

    private void Resume()
    {
        border.gameObject.SetActive(false);
        pauseIcon.gameObject.SetActive(false);
    }
}
