using UnityEngine;

public enum EMashSide { Customer, Seller }

public class MashInput : MonoBehaviour
{
    [SerializeField] private EMashSide side;
    private int sellerSfxCount;
    private int nextMaleVoiceCount = 6;

    public void TriggerMash()
    {
        if (MinigameManager.Instance == null || !MinigameManager.Instance.IsActive)
            return;

        if (side == EMashSide.Seller)
            PlaySellerMashSfx();

        if (side == EMashSide.Customer)
            MinigameManager.Instance.CustomerMash();
        else
            MinigameManager.Instance.SellerMash();
    }

    private void PlaySellerMashSfx()
    {
        sellerSfxCount++;

        if (sellerSfxCount >= nextMaleVoiceCount)
        {
            GameSound.PlaySfx(ESfxSoundId.MaleVoice);
            sellerSfxCount = 0;
            nextMaleVoiceCount = Random.Range(6, 8);
            return;
        }

        GameSound.PlaySfx(ESfxSoundId.MiniGameButton);
    }
}
