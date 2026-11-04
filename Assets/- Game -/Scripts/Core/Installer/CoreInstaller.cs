using UnityEngine;

public class CoreInstaller : BaseInstaller
{
    [Header("Services")]
    [SerializeField] private AudioService _audioService;
    [SerializeField] private VibrationService _vibrationService;

    public override void Install(GameContainer container)
    {
        container.BindInstance(_audioService);
        container.BindInstance(_vibrationService);

        container.Bind<GameSettings>();
        container.Bind<TimeService>();
        container.Bind<ScheduleService>();
        container.Bind<UIQueuePopup>();
        container.Bind<GameplayTransitionPoint>();
    }
}
