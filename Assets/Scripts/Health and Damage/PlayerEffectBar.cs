public class PlayerEffectBar : EffectBar {
    private void Awake() {
        PlayerHUDEvents.OnRegisterStatboard += OnRegisterStatboard;
    }

    private void OnRegisterStatboard(Statboard board) {
        Init(board);
        PlayerHUDEvents.OnRegisterStatboard -= OnRegisterStatboard;
    }
}