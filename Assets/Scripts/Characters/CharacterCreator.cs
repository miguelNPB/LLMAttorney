using System.Drawing;
using UnityEngine;

public class CharacterCreator : MonoBehaviour
{

    #region BodyParts
    [SerializeField]
    private Sprite[] _eyes;

    [SerializeField]
    private Sprite[] _noses;

    [SerializeField]
    private Sprite[] _mouths;

    [SerializeField]
    private Sprite[] _maleHairs;

    [SerializeField]
    private Sprite[] _femaleHairs;
    #endregion

    #region Colors

    [SerializeField]
    private Color32[] _skinColor;

    [SerializeField]
    private Color32[] _hairColor;

    #endregion

    #region BodyPartsReferences

    [SerializeField]
    private SpriteRenderer _bodyRef;

    [SerializeField]
    private SpriteRenderer _eyesRef;

    [SerializeField]
    private SpriteRenderer _noseRef;

    [SerializeField]
    private SpriteRenderer _mouthRef;

    [SerializeField]
    private SpriteRenderer _hairRef;

    #endregion

    [SerializeField, Range(0, 1)]
    private float _maleProbability;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        createRandomCharacter();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void createRandomCharacter()
    {

        float genderValue = Random.Range(0.0f, 1.0f);

        //Escoger Pelo
        if(genderValue > _maleProbability)
        {
            //Es mujer
            int hairValue = Random.Range(0, _femaleHairs.Length);
            _hairRef.sprite = _femaleHairs[hairValue];
        }
        else
        {
            //Es hombre
            int hairValue = Random.Range(0, _maleHairs.Length);
            _hairRef.sprite = _maleHairs[hairValue];
        }

        int hairColorValue = Random.Range(0, _hairColor.Length);
        _hairRef.color = _hairColor[hairColorValue];

        //Cuerpo
        int principalColor = Random.Range(0, _skinColor.Length);

        _bodyRef.color = _skinColor[principalColor];

        //Ojos
        int eyesValue = Random.Range(0, _eyes.Length);
        _eyesRef.sprite = _eyes[eyesValue];
        _eyesRef.color = _skinColor[principalColor];

        //Boca
        int mouthValue = Random.Range(0, _mouths.Length);
        _mouthRef.sprite = _mouths[mouthValue];
        _mouthRef.color = _skinColor[principalColor];

        //Ojos
        int noseValue = Random.Range(0, _noses.Length);
        _noseRef.sprite = _noses[noseValue];
        _noseRef.color = _skinColor[principalColor];

    }
}
