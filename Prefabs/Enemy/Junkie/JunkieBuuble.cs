using UnityEngine;
using System.Collections.Generic;

public class JunkieBubble : MonoBehaviour
{
    private Dictionary<SkinnedMeshRenderer, bool> skinnedMeshRendererStates = new Dictionary<SkinnedMeshRenderer, bool>();

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy"))
        {
            return;
        }

        EnemyHealth enemyHealth = other.GetComponentInParent<EnemyHealth>();
        if (enemyHealth == null)
        {
            enemyHealth = other.GetComponentInChildren<EnemyHealth>(true);
        }

        if (enemyHealth == null)
        {
            return;
        }

        // Populate and enable SkinnedMeshRenderers
        SkinnedMeshRenderer[] smrs = other.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (SkinnedMeshRenderer smr in smrs)
        {
            if (!skinnedMeshRendererStates.ContainsKey(smr))
            {
                skinnedMeshRendererStates[smr] = true;
            }
            smr.enabled = skinnedMeshRendererStates[smr];
        }

        enemyHealth.Resurrect();
    }

    public void ToggleSkinnedMeshRenderers(bool enable)
    {
        foreach (var kvp in skinnedMeshRendererStates)
        {
            kvp.Key.enabled = enable;
            skinnedMeshRendererStates[kvp.Key] = enable;
        }
    }
}
