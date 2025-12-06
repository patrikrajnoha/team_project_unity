using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Logika final portálu – čo sa stane, keď do neho hráč „vojde“.
/// </summary>
public class FinalPortal : MonoBehaviour
{
    [Header("Destination")]
    [Tooltip("Meno scény, ktorá sa načíta po vstupe do portálu. Ak je prázdne, iba vypíše log.")]
    public string targetSceneName;

    public void Enter()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.Log("[FinalPortal] Enter() – targetSceneName nie je nastavený. Tu si môžeš dopísať vlastnú logiku (napr. výhra).");
            return;
        }

        Debug.Log($"[FinalPortal] Loading scene: {targetSceneName}");
        SceneManager.LoadScene(targetSceneName);
    }
}
