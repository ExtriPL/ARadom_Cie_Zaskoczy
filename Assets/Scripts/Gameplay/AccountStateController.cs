using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AccountStateController : MonoBehaviour
{
    [SerializeField] private int MyAccountState = 1500;
    [SerializeField] private bool TookLoan = false;
    [SerializeField] private Button TakeLoan;
    [SerializeField] private GameObject TakeLoanPanel;
    [SerializeField] private TextMeshProUGUI amount;

    private void Update()
    {
        amount.text = MyAccountState.ToString();
    }

    public void OpenLoanOption()
    {
        TakeLoanPanel.SetActive(true);

        if(TookLoan==true)
        {
            TakeLoan.interactable = false;
        }
    }

    public void TakeLoad()
    {
        TakeLoan.interactable = false;
        TookLoan = true;
        MyAccountState += 1000;
    }

    public void CloseLoanOption()
    {
        TakeLoanPanel.SetActive(false);
    }
}
