using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterSeconds : MonoBehaviour {

    // Tiempo de vida del objeto
    [SerializeField] int timeToDestroy = 5;
// ------------------------------------------------------
// Corrutina que duerme el objeto despues de N segundos
// ------------------------------------------------------
IEnumerator DisableAfterNSeconds(int secondsToDisable)
    {
        // Esperamos los segundos especificados
        yield return new WaitForSeconds(secondsToDisable);
        // Dormimos el objeto
        this.gameObject.SetActive(false);
    }
    // ------------------------------------------------------
    // Método que se ejecuta cuando el objeto despierta
    // ------------------------------------------------------
    private void OnEnable()
    {
        // En vez de destruir... dormirmos el objeto del pool
        StartCoroutine(DisableAfterNSeconds(timeToDestroy));
    }


}
