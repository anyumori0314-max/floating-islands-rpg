using System.Runtime.CompilerServices;
using FloatingIslandsRpg.Application.Battle;
using FloatingIslandsRpg.Application.Save;
using FloatingIslandsRpg.Application.Scenes;
using FloatingIslandsRpg.Application.Session;
using FloatingIslandsRpg.Infrastructure.Save;
using FloatingIslandsRpg.Infrastructure.Scenes;

[assembly: InternalsVisibleTo("FloatingIslandsRpg.Tests.PlayMode")]

namespace FloatingIslandsRpg.Composition
{
    public sealed class GameServices
    {
        // Internal setters exist solely so PlayMode tests can substitute fakes (ISaveRepository-backed
        // use cases, an ISceneLoader-backed SceneTransitionUseCase), avoiding real disk I/O against
        // Application.persistentDataPath and real SceneManager.LoadSceneAsync calls (and their
        // cross-test bleed risk) while still exercising the production installer wiring.
        public ISaveRepository SaveRepository { get; internal set; }
        public SaveGameUseCase SaveGameUseCase { get; internal set; }
        public LoadGameUseCase LoadGameUseCase { get; internal set; }
        public ISceneLoader SceneLoader { get; internal set; }
        public SceneTransitionUseCase SceneTransitionUseCase { get; internal set; }

        public PlayerSessionState CurrentSession { get; set; }
        public BattleOutcome? LastBattleOutcome { get; set; }

        // A defensive copy of the player's session state taken immediately before a Battle
        // begins (see BattleSceneInstaller). Retry restores CurrentSession from this snapshot
        // instead of reusing the post-defeat state (which would have CurrentHp at 0), and
        // instead of the save file's CurrentSceneId (Retry always re-enters Battle).
        public PlayerSessionState RematchSnapshot { get; set; }

        // Set by a Field/Dungeon scene installer immediately before an Additive Battle load;
        // consumed and cleared by BattleSceneInstaller once the battle resolves. Null means
        // "no additive field/dungeon encounter in flight" (e.g. a Single-mode Retry from
        // GameClear, or before this feature existed).
        public PendingBattleContext PendingBattle { get; set; }

        // A defensive copy of the PendingBattle that was in flight when this battle started,
        // captured alongside RematchSnapshot (see BattleSceneInstaller.Start()). PendingBattle
        // itself is cleared as soon as the battle resolves (see OnBattleEnded), so without this,
        // Retry from GameClear had no way to know whether the original encounter was a boss fight
        // or which field/dungeon scene to return to (Codex review Major 2). Null means either no
        // battle has started yet, or it started with no PendingBattle context to preserve.
        public PendingBattleContext RematchPendingBattle { get; set; }

        public GameServices(string saveDirectoryPath)
        {
            var storage = new FileSystemSaveStorage(saveDirectoryPath);
            SaveRepository = new JsonSaveRepository(storage);
            SaveGameUseCase = new SaveGameUseCase(SaveRepository);
            LoadGameUseCase = new LoadGameUseCase(SaveRepository);
            SceneLoader = new UnitySceneLoader();
            SceneTransitionUseCase = new SceneTransitionUseCase(SceneLoader);
        }
    }
}
