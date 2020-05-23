using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public class BasePool
{
    /// <summary>
    /// Obiekt przechowujący wszystkie zinstancjonowane obiekty
    /// </summary>
    protected GameObject parentObject;
    /// <summary>
    /// Obiekt służący za matryce do stworzenia puli
    /// </summary>
    protected GameObject pattern { get; private set; }
    /// <summary>
    /// Pojemność puli (ilość obiektów obsługiwanych przez pulę)
    /// </summary>
    protected int capacity { get; private set; }
    /// <summary>
    /// Lista obiektów przechowywanych przez pule
    /// </summary>
    protected List<GameObject> poolObjects = new List<GameObject>();

    /// <summary>
    /// Inicjalizacja puli
    /// </summary>
    /// <param name="parentObject">Obiekt macierzysty przechowujący wszystkie obiekty w puli</param>
    /// <param name="pattern">Wzór, według którego mają zaostać stworzone wszystkie obiekty puli</param>
    /// <param name="capacity">Pojemność puli</param>
    public BasePool(GameObject parentObject, GameObject pattern, int capacity)
    {
        this.parentObject = parentObject;
        this.pattern = pattern;
        this.capacity = capacity;
    }

    /// <summary>
    /// Inicjalizacja podstawowych właściwości puli i tworzenie obiektów o podanym patternie
    /// </summary>
    public virtual void Init()
    {
        for (int i = 0; i < capacity; i++) AddObject();
    }

    /// <summary>
    /// Inicjalizacja podstawowych właściowści puli i przydzielanie obiektów z istniejącej listy do puli
    /// </summary>
    /// <param name="poolObjects"></param>
    public virtual void Init(List<GameObject> poolObjects)
    {
        this.poolObjects = poolObjects;
        capacity = poolObjects.Count;
        foreach(GameObject poolObject in poolObjects)
        {
            poolObject.GetComponent<Transform>().SetParent(parentObject.GetComponent<Transform>());
            poolObject.SetActive(false);
        }
    }

    /// <summary>
    /// Deinicjalizacja właściwości puli
    /// </summary>
    public virtual void Deinit()
    {
        for(int i = capacity - 1; i > 0; i--)
        {
            UnityEngine.Object.Destroy(poolObjects[i]);
            poolObjects.RemoveAt(i);
        }
    }

    /// <summary>
    /// Dodaje podstawowy obiekt do puli
    /// </summary>
    private void AddObject()
    {
        GameObject poolObject = UnityEngine.Object.Instantiate(pattern, parentObject.GetComponent<Transform>()); //Tworzenie nowego obiektu według patternu
        poolObject.SetActive(false); //Wyłączanie obiektu na start

        poolObjects.Add(poolObject); //Dodawanie nowego obiektu do puli
    }

    /// <summary>
    /// Pobiera obeikt z puli
    /// </summary>
    /// <returns>Włączony obiekt przechowywany przez pule</returns>
    public virtual GameObject TakeObject()
    {
        //Zapewnienie obecności obiektów w puli, gdy wszystkie podstawowe obiekty zostały wykorzystane
        if (poolObjects.Count == 0)
        {
            AddObject();
            capacity++;
        }

        GameObject poolObject = poolObjects[poolObjects.Count - 1]; //Wyciąganie obiektu z puli
        poolObjects.RemoveAt(poolObjects.Count - 1); //Usuwanie pobranego obiektu z listy dostępnych obiektów
        poolObject.SetActive(true);

        return poolObject;
    }

    /// <summary>
    /// Zwraca przekazany obiekt do puli
    /// </summary>
    /// <param name="poolObject">Obiekt, który należy zwrócić do puli</param>
    public virtual void ReturnObject(GameObject poolObject)
    {
        poolObject.SetActive(false);
        poolObjects.Add(poolObject);
    }
}