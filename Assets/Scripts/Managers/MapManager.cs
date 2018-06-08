﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class MapManager : MonoBehaviour
{
    [SerializeField]
    AreaStatusColorDictionary statusColors = new AreaStatusColorDictionary();

    [Space]
    [SerializeField]
    StringImageDictionary areaImages = new StringImageDictionary();

    GameData gameData;
    GeoLocManager geoLocManager;

    [Space]
    public Transform currentPosImg;

    // Esto funciona solo porque en el primer frame (cuando se cogen todas las referencias), el mapa estará desactivado
    private void OnEnable()
    {
        gameData = GameManager.Instance.GetGameData();
        geoLocManager = GeoLocManager.Instance;
        geoLocManager.WhenSceneAvailable += SetAreaColors;
        geoLocManager.WhenSceneAvailable += ShowCurrentPosition;

        SetAreaColors();
        ShowCurrentPosition();
    }

    public void DisableMap()
    {
        gameObject.SetActive(false);
    }

    private void SetAreaColors() // Se pueden poner en amarillo si están visitadas pero no completadas
    {
        foreach (KeyValuePair<string, AreaStatus> area in gameData.areasStatus)
        {
            try
            {
                areaImages[area.Key].color = statusColors[area.Value]; // Devuelve el color que corresponde a ese estado
            }
            catch (KeyNotFoundException e)
            {
                Debug.LogError("No has referenciado el área " + area.Key + " con su imagen en el MapManager");
            }
            catch (NullReferenceException e)
            {
                Debug.LogError("Hay un área que no está en el GeoLocData");
            }
        }
    }

    private void ShowCurrentPosition()
    {        
        string currentArea = geoLocManager.GetCurrentArea();
        Debug.Log("Show current position with current area: " + currentArea);
        if (currentArea != GameManager.defaultAreaName) currentPosImg.position = areaImages[geoLocManager.GetCurrentArea()].transform.position;
    }
}