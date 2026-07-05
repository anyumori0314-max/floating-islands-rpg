using UnityEngine;

namespace FloatingIslandsRpg.Composition
{
    public static class GameCompositionRootLocator
    {
        public static GameCompositionRoot EnsureRoot()
        {
            var existing = Object.FindFirstObjectByType<GameCompositionRoot>();
            if (existing != null)
            {
                existing.EnsureServices();
                return existing;
            }

            var go = new GameObject(nameof(GameCompositionRoot));
            var root = go.AddComponent<GameCompositionRoot>();
            return root;
        }
    }
}
