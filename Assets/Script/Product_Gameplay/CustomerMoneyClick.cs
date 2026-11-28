using UnityEngine;

public class CustomerMoneyClick : MonoBehaviour
{
    void OnMouseDown()
    {
        if (CheckoutManager.Instance != null)
        {
            CheckoutManager.Instance.TakeCustomerMoney();
        }
    }
}