using System.Collections;
using System.Collections.Generic;
using UnityEngine;



    using System;

    using System.Runtime.InteropServices;
    using GoogleARCore;
    using GoogleARCoreInternal;

    public class Visualizer : MonoBehaviour
    {
        /// <summary>
        /// Obraz na kjtorym ma byc wyswietlony model.
        /// </summary>
        public AugmentedImage Image;

        /// <summary>
        /// Model który ma byc wyswietlany na obrazku.
        /// </summary>
        public GameObject Model;

    public void Start()
    {
        Model = Instantiate(Model, gameObject.GetComponent<Transform>());
    }

    public void Update()
        {
            if (Image == null || Image.TrackingState != TrackingState.Tracking)
            {
                Model.SetActive(false);
                return;
            }

            Model.transform.localPosition = (Image.ExtentX * Vector3.left) + (Image.ExtentZ * Vector3.back);
            Model.transform.localScale.Set(5, 5, 5);

            Debug.LogError(Model.transform.localScale);



            Model.SetActive(true);

        }
    }
