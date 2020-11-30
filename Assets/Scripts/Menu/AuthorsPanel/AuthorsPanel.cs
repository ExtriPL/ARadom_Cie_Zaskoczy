﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class AuthorsPanel : MonoBehaviour, IInitiable<MainMenuController>
{
    public Authors authors;
    public List<GameObject> professionContainers;
    private List<List<Authors.Author>> randomAuthors = new List<List<Authors.Author>>();
    private MainMenuController mainMenuController;

    public void Init()
    {
        RandomizeLists();

        for (int i = 0; i< randomAuthors.Count; i++) 
        {
            professionContainers[i].GetComponent<TextMeshProUGUI>().text = "";
            for (int j = 0; j < randomAuthors[i].Count; j++) 
            {
                professionContainers[i].GetComponent<TextMeshProUGUI>().text += randomAuthors[i][j].name + " ";
                professionContainers[i].GetComponent<TextMeshProUGUI>().text += randomAuthors[i][j].surname + "<br>";
                professionContainers[i].GetComponent<TextMeshProUGUI>().text += randomAuthors[i][j].website;
            }
        }

        mainMenuController.loadingScreen.EndLoading();
    }

    public void PreInit(MainMenuController mainMenuController) 
    {
        this.mainMenuController = mainMenuController;
    }

    public void DeInit() { }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            mainMenuController.OpenPanel(Panel.MenuPanel);
    }

    private void RandomizeLists() 
    {
        randomAuthors.Clear();
        for (int i = 0; i <= (int)Authors.Profession.teacher; i++)
        { 
            randomAuthors.Add(new List<Authors.Author>());
            for (int j = 0; j < authors.authors.Count; j++)
            {
                if (authors.authors[j].proffesion == (Authors.Profession)i)
                {
                    randomAuthors[i].Add(authors.authors[j]);
                }
            }
            randomAuthors[i] = randomAuthors[i].OrderBy(a => Guid.NewGuid()).ToList();
        }
    }
}
