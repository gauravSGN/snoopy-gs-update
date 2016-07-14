using Service;
using UnityEngine;

namespace UI.Callbacks
{
    public class PurchaseHearts : MonoBehaviour
    {
        [SerializeField]
        private int numberOfHearts;

        [SerializeField]
        private int coinCost;

        public void Buy()
        {
            var user = GlobalState.Instance.Services.Get<UserStateService>();

            if ((user != null) &&
                (user.purchasables.hearts.quantity < GlobalState.Instance.Config.purchasables.maxHearts) &&
                (user.purchasables.coins.quantity >= coinCost))
            {
                user.purchasables.hearts.quantity += numberOfHearts;
                user.purchasables.coins.quantity -= coinCost;
            }
        }
    }
}