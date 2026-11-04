using UnityEngine;

namespace Gameplay
{
    public class GameplayInstaller : BaseInstaller
    {
        [SerializeField] private ShotConfig _shotConfig;
        [SerializeField] private PlayerConfig _playerConfig;
        [SerializeField] private DoorConfig _doorConfig;
        [SerializeField] private ObstacleConfig _obstacleConfig;
        [SerializeField] private Transform _levelRoot;
        [SerializeField] private HoldInputController _holdInputController;
        [SerializeField] private PlayerAdvance _playerAdvance;
        [SerializeField] private DoorObject _doorObject;
        [SerializeField] private ScaleBinding _scaleBinding;

        public override void Install(GameContainer container)
        {
            container.BindInstance(_shotConfig);
            container.BindInstance(_playerConfig);
            container.BindInstance(_doorConfig);
            container.BindInstance(_obstacleConfig);
            container.BindInstance(_shotConfig.Charge);
            container.Bind<ChargeTransfer>();
            container.Bind<ObstacleRegistry>();
            container.BindInstance(_levelRoot);
            container.Bind<ShotSpawner>();
            container.BindInstance(_holdInputController);
            container.BindInstance(_playerAdvance);
            container.BindInstance(_doorObject);
            container.BindInstance(_scaleBinding);
            container.Bind<GameplaySession>();
        }
    }
}
