using FloatingIslandsRpg.Presentation.Dialogue;
using UnityEngine;

namespace FloatingIslandsRpg.Composition.Scenes
{
    public sealed class VillageSceneInstaller : MonoBehaviour
    {
        private void Start()
        {
            var services = GameCompositionRootLocator.EnsureRoot().Services;
            if (services.CurrentSession == null)
            {
                return;
            }

            var npcs = FindObjectsByType<NpcInteractable>(FindObjectsSortMode.None);
            foreach (var npc in npcs)
            {
                npc.LinkedQuest = services.CurrentSession.MainQuest;
            }
        }
    }
}
