using UnityEngine;

public enum EMashSide { Customer, Seller }

public class MashInput : MonoBehaviour
{
    [SerializeField] private EMashSide side;

    public void TriggerMash()
    {
        if (MinigameManager.Instance == null || !MinigameManager.Instance.IsActive)
            return;

        if (side == EMashSide.Seller)
            GameSound.PlaySfx(ESfxSoundId.MiniGameButton);

        if (side == EMashSide.Customer)
            MinigameManager.Instance.CustomerMash();
        else
            MinigameManager.Instance.SellerMash();
    }
}
