using System.Drawing;
using UnityEngine;

public class CharacterCreator : MonoBehaviour
{

    enum Roles
    {
        Lawyer,
        Judge,
        Client
    }

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

    [SerializeField]
    private Sprite[] _femaleBehindHairs;

    [SerializeField]
    private Sprite _maleLawyerClothes;

    [SerializeField]
    private Sprite _femaleLawyerClothes;

    [SerializeField]
    private Sprite _judgeClothes;

    [SerializeField]
    private Sprite _normalClothes;

    #endregion

    #region Colors

    [SerializeField]
    private Color32[] _skinColor;

    [SerializeField]
    private Color32[] _hairColor;

    [SerializeField]
    private Color32[] _shirtColor;

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

    [SerializeField]
    private SpriteRenderer _hairBehindRef;

    [SerializeField]
    private SpriteRenderer _clothesRef;

    #endregion

    [SerializeField, Range(0, 1)]
    private float _maleProbability;

    [SerializeField]
    private Roles _role;

    private bool _aspectoAsignado = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!_aspectoAsignado)
        {
            createRandomCharacter();
            _aspectoAsignado = true;
        }
        
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
            _hairBehindRef.sprite = _femaleBehindHairs[hairValue];
        }
        else
        {
            //Es hombre
            int hairValue = Random.Range(0, _maleHairs.Length);
            _hairRef.sprite = _maleHairs[hairValue];
            
        }

        int hairColorValue = Random.Range(0, _hairColor.Length);
        _hairRef.color = _hairColor[hairColorValue];
        _hairBehindRef.color = _hairColor[hairColorValue];

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

        //Ropa

        if(_role == Roles.Client)
        {
            int clothesColor = Random.Range(0, _shirtColor.Length);
            _clothesRef.sprite = _normalClothes;
            _clothesRef.color = _shirtColor[clothesColor];
        }
        else if(_role == Roles.Judge)
        {
            _clothesRef.sprite = _judgeClothes;
        }
        else
        {
            if (genderValue > _maleProbability)
            {
                _clothesRef.sprite = _femaleLawyerClothes;
            }
            else
            {
                _clothesRef.sprite = _maleLawyerClothes;
            }
        }
    }
}
