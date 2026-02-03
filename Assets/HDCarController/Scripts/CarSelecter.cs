using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarSelecter : MonoBehaviour
{
    public GameObject[] Cars = new GameObject[4];
    public Dropdown SelectCarDdl;
    public void SelectCar()
    {
        int index = SelectCarDdl.value;
        for (int i = 0; i < Cars.Length; i++)
        {
            if (i == index)
            {
                Cars[i].SetActive(true);
            }
            else
            {
                Cars[i].SetActive(false);
            }
        }
    }
}
