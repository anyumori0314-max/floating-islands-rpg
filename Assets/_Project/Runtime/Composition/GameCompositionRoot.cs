using UnityEngine;

namespace FloatingIslandsRpg.Composition
{
    public sealed class GameCompositionRoot : MonoBehaviour
    {
        public GameServices Services { get; private set; }

        private void Awake()
        {
            var existing = FindObjectsByType<GameCompositionRoot>(FindObjectsSortMode.None);
            if (existing.Length > 1)
            {
                Destroy(gameObject);
                return;
            }

            EnsureServices();
            DontDestroyOnLoad(gameObject);
        }

        // Defensive: guarantees Services is never left null for any surviving instance,
        // regardless of how it was constructed (e.g. found already alive via the locator).
        public void EnsureServices()
        {
            if (Services == null)
            {
                Services = new GameServices(UnityEngine.Application.persistentDataPath);
            }
        }
    }
}
