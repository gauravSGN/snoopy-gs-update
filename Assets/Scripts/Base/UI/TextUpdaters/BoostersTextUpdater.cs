using UnityEngine;

public class BoostersTextUpdater : UITextUpdater
{
    [SerializeField]
    private GameObject buyButton;

    [SerializeField]
    private GameObject boosterCountDisplay;

    override protected void Start()
    {
        base.Start();

        if (GlobalState.User != null)
        {
            Target = GlobalState.User.purchasables.boosters;
        }
    }

    override protected void UpdateText(Observable target)
    {
        if ((text != null) && (target != null))
        {
            var updatedText = BuildString();

            text.text = updatedText;

            bool boosterCountDisplayActive = (int.Parse(updatedText) > 0);

            boosterCountDisplay.SetActive(boosterCountDisplayActive);
            buyButton.SetActive(!boosterCountDisplayActive);
        }
    }
}
