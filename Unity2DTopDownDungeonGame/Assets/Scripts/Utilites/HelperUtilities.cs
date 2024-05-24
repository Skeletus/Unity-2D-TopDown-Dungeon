using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class HelperUtilities
{
    /// <summary>
    /// verificacion de un string vacio
    /// </summary>
    /// <param name="thisObject"></param>
    /// <param name="fieldName"></param>
    /// <param name="stringToCheck"></param>
    /// <returns></returns>
    public static bool ValidateCheckEmptyStrings(Object thisObject, string fieldName, string stringToCheck)
    {
        if (stringToCheck == "")
        {
            Debug.Log(fieldName + " esta vacio y debe contener un valor en el objeto " + thisObject.name.ToString());
            return true;
        }
        return false;
    }

    /// <summary>
    /// verificacion de lista vacia o que contiene valores nulos - devuelve true si hay un error
    /// </summary>
    /// <param name="thisObject"></param>
    /// <param name="fieldName"></param>
    /// <param name="enumerableObjectsToCheck"></param>
    /// <returns></returns>
    public static bool ValidateCheckEnumerableValues(Object thisObject, string fieldName, IEnumerable enumerableObjectsToCheck)
    {
        bool error = false;
        int count = 0;

        if (enumerableObjectsToCheck == null)
        {
            Debug.Log(fieldName + " es nulo en el objeto " + thisObject.name.ToString());
            return true;
        }

        foreach (var item in enumerableObjectsToCheck)
        {
            if (item == null)
            {
                Debug.Log(fieldName + " tiene valores nulos en el objeto " + thisObject.name.ToString());
                error = true;
            }
            else
            {
                count++;
            }
        }

        if (count == 0)
        {
            Debug.Log(fieldName + " no tiene valores en el objeto " + thisObject.name.ToString());
            error = true;
        }

        return error;
    }
}
