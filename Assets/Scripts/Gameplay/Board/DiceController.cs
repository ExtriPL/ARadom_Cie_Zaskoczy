using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

using Action = System.Action;

public class DiceController : MonoBehaviour
{
    [SerializeField]
    private Transform dice1Holder, dice2Holder;

    [SerializeField, Tooltip("Czas trwania rzutu kostką")]
    private float timeDuration;
    [SerializeField, Range(0f, 1f), Tooltip("Określa, w jakim stopniu wykonywanie animacji może być krótsze od timeDuration (w %)")]
    private float durationVariation;
    [SerializeField, Tooltip("Wysokość, na jaką zostaną rzucone kostki")]
    private float throwHeight;
    [SerializeField, Range(0f, 1f), Tooltip("Określa, w jakim stopniu wysokość podskoku być niższa od throwHeight (w %)")]
    private float heightVariation;
    [SerializeField, Range(0f, 360f), Tooltip("Maksymalny kąt, o jaki może kosta podczas animacji obrócić się na osi Y")]
    private int maxYAngle;
    [SerializeField, Range(0f, 10f), Tooltip("Zasięg, na jaki kostka może polecieć podczas animacji")]
    private float throwRadius;
    [SerializeField, Range(0f, 1f), Tooltip("Określa w jakim stopniu ostateczna pozycja może różnić się od wyznaczonej")]
    private float positionVariation;
    [SerializeField, Tooltip("Lista zawierająca wszystkie wyniki rzutów, razem z ich obrotami")]
    private List<RollRotation> rollRotations = new List<RollRotation>();

    private float startTime;
    private bool isRolling = false;

    /// <summary>
    /// Obecne odchylenia czasy animacji rzutu kostką
    /// </summary>
    private float currentTimeVariation1, currentTimeVariation2;
    /// <summary>
    /// Obecne odchylenie od maksymalnej wysokości rzutu kostką
    /// </summary>
    private float currentHeightVariation1, currentHeightVariation2;
    /// <summary>
    /// Kierunki obrotu na poszczególnych osiach kostki
    /// </summary>
    private Vector3 rotationDirection1, rotationDirection2;
    /// <summary>
    /// Obrót kostki, jaki miała na początku rzutu
    /// </summary>
    private Vector3 startRotation1, startRotation2;
    /// <summary>
    /// Pozycja kostki pzy rozpoczęciu rzutu
    /// </summary>
    private Vector3 startPosition1, startPosition2;
    /// <summary>
    /// Końcowa pozycja kostki po wykonaniu animacji
    /// </summary>
    private Vector3 targetPosition1, targetPosition2;
    /// <summary>
    /// Obecny kąt obrotu na osi Y kostki
    /// </summary>
    private float currentYAngel1, currentYAngel2;
    private RollRotation dice1End, dice2End;

    private PhotonView photonView;
    private Vector3 startDice1Position, startDice2Position;

    private Action onRollEnd;

    private void Start()
    {
        startDice1Position = dice1Holder.localPosition;
        startDice2Position = dice2Holder.localPosition;
        photonView = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            Roll(Random.Range(1, 6), Random.Range(1, 6));

        if(isRolling)
        {
            float timeProgress = (Time.time - startTime) / timeDuration;

            PositionInterpolation(timeProgress, currentTimeVariation1, currentHeightVariation1, startPosition1, targetPosition1, dice1Holder);
            PositionInterpolation(timeProgress, currentTimeVariation2, currentHeightVariation2, startPosition2, targetPosition2, dice2Holder);

            RotationInterpolation(timeProgress, currentTimeVariation1, dice1End, startRotation1, rotationDirection1, currentYAngel1, dice1Holder);
            RotationInterpolation(timeProgress, currentTimeVariation2, dice2End, startRotation2, rotationDirection2, currentYAngel2, dice2Holder);

            if (timeProgress >= 1f)
            {
                GameplayController.instance.flow.Resume();
                onRollEnd?.Invoke();
                onRollEnd = null;
                isRolling = false;
            }
        }
    }

    /// <summary>
    /// Animuje kostki, by te wylosowały podane wyniki
    /// </summary>
    /// <param name="result1">Wynik na pierwszej kostce</param>
    /// <param name="result2">Wynik na drugiej kostce</param>
    public void Roll(int result1, int result2, Action onRollEnd = null)
    {
        if (isRolling)
            return;

        this.onRollEnd = onRollEnd;

        dice1End = rollRotations[result1 - 1];
        dice2End = rollRotations[result2 - 1];

        currentTimeVariation1 = Random.Range(0f, durationVariation);
        currentTimeVariation2 = Random.Range(0f, durationVariation);

        currentHeightVariation1 = Random.Range(0f, heightVariation);
        currentHeightVariation2 = Random.Range(0f, heightVariation);

        currentYAngel1 = Random.Range(-maxYAngle, maxYAngle);
        rotationDirection1 = new Vector3(MathExtension.RandomSign(), MathExtension.RandomSign(), MathExtension.RandomSign());
        currentYAngel2 = Random.Range(-maxYAngle, maxYAngle);
        rotationDirection2 = new Vector3(MathExtension.RandomSign(), MathExtension.RandomSign(), MathExtension.RandomSign());

        startRotation1 = dice1Holder.localRotation.eulerAngles;
        startRotation2 = dice2Holder.localRotation.eulerAngles;

        startPosition1 = dice1Holder.localPosition;
        startPosition2 = dice2Holder.localPosition;

        Vector3 displacement = startDice2Position - startDice1Position;
        targetPosition1 = new Vector3(Random.Range(-throwRadius, throwRadius), 0f, Random.Range(-throwRadius, throwRadius));
        targetPosition2 = targetPosition1 + displacement;

        float placeDisplacement = displacement.magnitude * positionVariation;
        targetPosition1 += new Vector3(Random.Range(-placeDisplacement, placeDisplacement), 0f, Random.Range(-placeDisplacement, placeDisplacement));
        targetPosition2 += new Vector3(Random.Range(-placeDisplacement, placeDisplacement), 0f, Random.Range(-placeDisplacement, placeDisplacement));

        isRolling = true;
        startTime = Time.time;

        object[] variables = new object[] { 
            result1, result2,
            currentTimeVariation1, currentTimeVariation2,
            currentHeightVariation1, currentHeightVariation2,
            currentYAngel1, rotationDirection1, currentYAngel2, rotationDirection2,
            startRotation1, startRotation2,
            startPosition1, startPosition2,
            targetPosition1, targetPosition2
        };
        photonView.RPC("Roll", RpcTarget.Others, variables);
    }

    /// <summary>
    /// Funkcja służąca do synchronizacji kostek między graczami
    /// </summary>
    /// <param name="variables"></param>
    [PunRPC]
    public void Roll(object[] variables)
    {
        dice1End = rollRotations[(int)variables[0] - 1];
        dice2End = rollRotations[(int)variables[1] - 1];

        currentTimeVariation1 = (float)variables[2];
        currentTimeVariation2 = (float)variables[3];

        currentHeightVariation1 = (float)variables[4];
        currentHeightVariation2 = (float)variables[5];

        currentYAngel1 = (float)variables[6];
        rotationDirection1 = (Vector3)variables[7];
        currentYAngel2 = (float)variables[8];
        rotationDirection2 = (Vector3)variables[9];

        startRotation1 = (Vector3)variables[10];
        startRotation2 = (Vector3)variables[11];

        startPosition1 = (Vector3)variables[12];
        startPosition2 = (Vector3)variables[13];

        targetPosition1 = (Vector3)variables[14];
        targetPosition2 = (Vector3)variables[15];

        isRolling = true;
        startTime = Time.time;
    }

    /// <summary>
    /// Funckja animująca wysokość kostek
    /// </summary>
    /// <param name="timeProgress">Postęp animacji</param>
    private void PositionInterpolation(float timeProgress, float currentTimeVariation, float currentHeightVariation, Vector3 startPosition, Vector3 targetPosition, Transform diceHolder)
    {
        float diceProgress = GetDiceProgress(timeProgress, currentTimeVariation);
        float diceHeight = GetCurrentHeight(diceProgress, throwHeight * (1f - currentHeightVariation));

        Vector3 position = Vector3.Lerp(startPosition, targetPosition, diceProgress);

        diceHolder.localPosition = new Vector3(position.x, diceHeight, position.z);
    }

    /// <summary>
    /// Funkcja animująca obrót kostek
    /// </summary>
    /// <param name="timeProgress">Postęp animacji</param>
    private void RotationInterpolation(float timeProgress, float currentTimeVariation, RollRotation diceEnd, Vector3 startRotation, Vector3 rotationDirection, float currentYAngel, Transform diceHolder)
    {
        float diceProgress = GetDiceProgress(timeProgress, currentTimeVariation);

        //Dodatkowy obrót występuje tylko na osi, na której już nie ma obrotu
        float rotationX = rotationDirection.x == 1 ? diceEnd.rotation.x : MathExtension.InverseRotation(diceEnd.rotation.x);
        float rotationY = rotationDirection.y == 1 ? diceEnd.rotation.y : MathExtension.InverseRotation(diceEnd.rotation.y);
        rotationY += currentYAngel;
        float rotationZ = rotationDirection.z == 1 ? diceEnd.rotation.z : MathExtension.InverseRotation(diceEnd.rotation.z);
        Vector3 targetRotation = new Vector3(rotationX, rotationY, rotationZ);

        Vector3 diceRotation = Vector3.Lerp(startRotation, targetRotation, diceProgress);

        diceHolder.localRotation = Quaternion.Euler(diceRotation);
    }

    /// <summary>
    /// Zwraca postęp animacji kostki, uwzględniajac wariację w jej czasie trwania
    /// </summary>
    /// <param name="timeProgress">Obecny postęp animacji</param>
    /// <param name="variation">Warjacja trwania animacji wyrażona w procentach (od 0 do 1)</param>
    private float GetDiceProgress(float timeProgress, float variation)
    {
        float duration = timeDuration * (1f - variation);
        float time = timeProgress * timeDuration;

        return Mathf.Clamp01(time / duration);
    }

    /// <summary>
    /// Zwraca obecną wysokość, na jakiej znajduje się kostka
    /// </summary>
    /// <param name="progress">Postęp animacji kostki</param>
    /// <param name="height">Maksymalna wysokość, na jaką może unieść się kostka</param>
    /// <returns></returns>
    private float GetCurrentHeight(float progress, float height)
    {
        //Połowę animcji lecimy w górę, a połowę w dół
        if (progress <= 0.5f)
        {
            float currentProgress = MathExtension.Map(0f, 0.5f, 0f, 1f, progress);
            return Mathfx.Sinerp(0f, height, currentProgress);
        }
        else
        {
            float currentProgress = MathExtension.Map(0f, 0.5f, 0f, 1f, progress - 0.5f);
            return Mathfx.Coserp(height, 0f, currentProgress);
        }
    }

    public void ToggleVisibility(bool visible)
    {
        dice1Holder.gameObject.SetActive(visible);
        dice2Holder.gameObject.SetActive(visible);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(dice1Holder.position + transform.up * throwHeight, 0.1f);
        Gizmos.DrawWireSphere(dice2Holder.position + transform.up * throwHeight, 0.1f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, throwRadius);
    }

    [System.Serializable]
    private struct RollRotation
    {
        public int result;
        public Vector3 rotation;
    }
}
