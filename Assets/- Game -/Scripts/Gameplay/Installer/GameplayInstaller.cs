using UnityEngine;

namespace Gameplay
{
    public class GameplayInstaller : BaseInstaller
    {
        [SerializeField] private ShotConfig _shotConfig;
        [SerializeField] private PlayerConfig _playerConfig;
        [SerializeField] private ObstacleConfig _obstacleConfig;
        [SerializeField] private Transform _levelRoot;
        [SerializeField] private PathRoute _route;
        [SerializeField] private HoldInputController _holdInputController;
        [SerializeField] private PlayerAdvance _playerAdvance;
        [SerializeField] private DoorObject _doorObject;

        public override void Install(GameContainer container)
        {
            container.BindInstance(_shotConfig);
            container.BindInstance(_playerConfig);
            container.BindInstance(_obstacleConfig);
            container.BindInstance(_shotConfig.Charge);
            container.BindInstance(_levelRoot);
            container.BindInstance(_route);
            container.BindInstance(_holdInputController);
            container.BindInstance(_playerAdvance);
            container.BindInstance(_doorObject);

            container.Bind<ChargeTransfer>();
            container.Bind<ObstacleRegistry>();
            container.Bind<ShotSpawner>();
            container.Bind<LanePassage>();
            container.Bind<ObstacleClearing>();
            container.Bind<ShotSequence>();
            container.Bind<LaneAdvance>();
            container.Bind<GameplaySession>();
        }
    }
}
