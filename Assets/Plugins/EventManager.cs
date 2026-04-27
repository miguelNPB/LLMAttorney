using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Telemetry
{
    public class EventManager : MonoBehaviour
    {

        public const int SerializationJson = 0;
        public const int PersistenceFile = 0;
        public const int QueueCircularArray = 0;

        private int _sessionID = -1;
        private int _userID = -1;
        private IntPtr _trackerHandle = IntPtr.Zero;

        private EventManager _instance = null;

        [SerializeField]
        private bool _persistPeriodically = false;

        [SerializeField]
        private int _persistSeconds = 60;

        private double _elapsedTime = 0;

        public EventManager Instance
        {
            get { return _instance; }
        }

        enum AtributesNameId
        {
            eventType = 0,
            sessionID = 1,
            userID = 2,
            timeStamp = 3,
            messageID = 4,
            phaseID = 5,
            price = 6,
            documentType = 7,
            isValid = 8,

        }

        private void SubmitEvent(IntPtr eventPtr)
        {
            if (eventPtr == IntPtr.Zero)
            {
                return;
            }

            // El ownership pasa siempre a C++ al invocar TrackEvent,
            // incluso si internamente falla la insercion.
            TelemetryNative.TrackEvent(_trackerHandle, eventPtr);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageID"></param>
        /// <param name="phaseID"></param>
        public void SendNotConsistentAnswerEvent(int messageID, int phaseID)
        {

            Debug.Log("Envio de evento de fakta de coherencia");

            //Establecimiento del numero de atributos
            const int attributeCount = 4;

            //Creacion del evento
            IntPtr eventPtr;

            try
            {
                eventPtr = TelemetryNative.CreateEvent(attributeCount);
            }
            catch (System.EntryPointNotFoundException)
            {
                Debug.LogWarning("Telemetry DLL no disponible o funci�n no encontrada");
                return;
            }
            catch (System.DllNotFoundException)
            {
                Debug.LogWarning("Telemetry DLL no encontrada");
                return;
            }

            if (eventPtr == IntPtr.Zero)
            {
                return;
            }

            //obtencion del puntero de los atributos
            IntPtr attributesBase = TelemetryUtils.GetEventAttributesPtr(eventPtr);

            if (attributesBase == IntPtr.Zero)
            {
                TelemetryNative.DestroyEvent(eventPtr);
                return;
            }

            //Comprobacion de valores de los atributos
            if(messageID < 0 || _sessionID == -1 || _userID == -1 || phaseID < 1)
            {
                TelemetryNative.DestroyEvent(eventPtr);
                return;
            }

            //Escritura de los atributos
            TelemetryUtils.WriteAttributeInt64(attributesBase, 0, (int)AtributesNameId.sessionID, _sessionID);
            TelemetryUtils.WriteAttributeInt64(attributesBase, 1, (int)AtributesNameId.userID, _userID);
            TelemetryUtils.WriteAttributeInt32(attributesBase, 2, (int)AtributesNameId.messageID, messageID);
            TelemetryUtils.WriteAttributeInt32(attributesBase, 3, (int)AtributesNameId.phaseID, phaseID);

            //Escritura de directivas externas a atributos
            TelemetryUtils.WriteEventHeader(eventPtr, 0, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

            Debug.Log("Se envio del todo");

            //TODO Envio del evento
            SubmitEvent(eventPtr);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageID"></param>
        /// <param name="price"></param>
        public void SendDeniedBudgetEvent(int messageID, int price)
        {
            //Establecimiento del numero de atributos
            const int attributeCount = 4;

            //Creacion del evento
            IntPtr eventPtr;

            try
            {
                eventPtr = TelemetryNative.CreateEvent(attributeCount);
            }
            catch (System.EntryPointNotFoundException)
            {
                Debug.LogWarning("Telemetry DLL no disponible o funci�n no encontrada");
                return;
            }
            catch (System.DllNotFoundException)
            {
                Debug.LogWarning("Telemetry DLL no encontrada");
                return;
            }

            if (eventPtr == IntPtr.Zero)
            {
                return;
            }

            //obtencion del puntero de los atributos
            IntPtr attributesBase = TelemetryUtils.GetEventAttributesPtr(eventPtr);

            if (attributesBase == IntPtr.Zero)
            {
                TelemetryNative.DestroyEvent(eventPtr);
                return;
            }

            //Comprobacion de valores de los atributos
            if (messageID < 0 || _sessionID == -1 || _userID == -1 || price < 0)
            {
                TelemetryNative.DestroyEvent(eventPtr);
                return;
            }

            //Escritura de los atributos


            TelemetryUtils.WriteAttributeInt64(attributesBase, 0, (int)AtributesNameId.sessionID, _sessionID);
            TelemetryUtils.WriteAttributeInt64(attributesBase, 1, (int)AtributesNameId.userID, _userID);
            TelemetryUtils.WriteAttributeInt32(attributesBase, 2, (int)AtributesNameId.messageID, messageID);
            TelemetryUtils.WriteAttributeInt32(attributesBase, 4, (int)AtributesNameId.price, price);


            //Escritura de directivas externas a atributos
            TelemetryUtils.WriteEventHeader(eventPtr, 1, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

            //TODO Envio del evento
            SubmitEvent(eventPtr);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="documentType"></param>
        public void SendPostDocumentEvent(int documentType)
        {
            //Establecimiento del numero de atributos
            const int attributeCount = 3;

            //Creacion del evento
            IntPtr eventPtr;

            try
            {
                eventPtr = TelemetryNative.CreateEvent(attributeCount);
            }
            catch (System.EntryPointNotFoundException)
            {
                Debug.LogWarning("Telemetry DLL no disponible o funci�n no encontrada");
                return;
            }
            catch (System.DllNotFoundException)
            {
                Debug.LogWarning("Telemetry DLL no encontrada");
                return;
            }

            if (eventPtr == IntPtr.Zero)
            {
                return;
            }

            //obtencion del puntero de los atributos
            IntPtr attributesBase = TelemetryUtils.GetEventAttributesPtr(eventPtr);

            if (attributesBase == IntPtr.Zero)
            {
                TelemetryNative.DestroyEvent(eventPtr);
                return;
            }

            //Comprobacion de valores de los atributos
            if (documentType < 0 || _sessionID == -1 || _userID == -1)
            {
                TelemetryNative.DestroyEvent(eventPtr);
                return;
            }

            //Escritura de los atributos
            TelemetryUtils.WriteAttributeInt64(attributesBase, 0, (int)AtributesNameId.sessionID, _sessionID);
            TelemetryUtils.WriteAttributeInt64(attributesBase, 1, (int)AtributesNameId.userID, _userID);
            TelemetryUtils.WriteAttributeInt32(attributesBase, 2, (int)AtributesNameId.documentType, documentType);

            //Escritura de directivas externas a atributos
            TelemetryUtils.WriteEventHeader(eventPtr, 2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

            //TODO Envio del evento
            SubmitEvent(eventPtr);
        }

    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="documentType"></param>
        /// <param name="isValid"></param>
        public void SendReceivedDocumentEvent(int documentType, bool isValid)
        {
            //Establecimiento del numero de atributos
            const int attributeCount = 4;

            //Creacion del evento
            IntPtr eventPtr;

            try
            {
                eventPtr = TelemetryNative.CreateEvent(attributeCount);
            }
            catch (System.EntryPointNotFoundException)
            {
                Debug.LogWarning("Telemetry DLL no disponible o funci�n no encontrada");
                return;
            }
            catch (System.DllNotFoundException)
            {
                Debug.LogWarning("Telemetry DLL no encontrada");
                return;
            }

            if (eventPtr == IntPtr.Zero)
            {
                return;
            }

            //obtencion del puntero de los atributos
            IntPtr attributesBase = TelemetryUtils.GetEventAttributesPtr(eventPtr);

            if (attributesBase == IntPtr.Zero)
            {
                TelemetryNative.DestroyEvent(eventPtr);
                return;
            }

            //Comprobacion de valores de los atributos
            if (documentType < 0 || _sessionID == -1 || _userID == -1)
            {
                TelemetryNative.DestroyEvent(eventPtr);
                return;
            }

            TelemetryUtils.WriteAttributeInt64(attributesBase, 0, (int)AtributesNameId.sessionID, _sessionID);
            TelemetryUtils.WriteAttributeInt64(attributesBase, 1, (int)AtributesNameId.userID, _userID);
            TelemetryUtils.WriteAttributeInt32(attributesBase, 2, (int)AtributesNameId.documentType, documentType);
            TelemetryUtils.WriteAttributeBool(attributesBase, 3, (int)AtributesNameId.isValid, isValid);


            //Escritura de directivas externas a atributos
            TelemetryUtils.WriteEventHeader(eventPtr, 3, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

            //TODO Envio del evento
            SubmitEvent(eventPtr);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="price"></param>
        /// <param name="documentType"></param>
        public void SendAskedDocumentEvent(int price, int documentType)
        {
            //Establecimiento del numero de atributos
            const int attributeCount = 4;

            //Creacion del evento
            IntPtr eventPtr;

            try
            {
                eventPtr = TelemetryNative.CreateEvent(attributeCount);
            }
            catch (System.EntryPointNotFoundException)
            {
                Debug.LogWarning("Telemetry DLL no disponible o funci�n no encontrada");
                return;
            }
            catch (System.DllNotFoundException)
            {
                Debug.LogWarning("Telemetry DLL no encontrada");
                return;
            }

            if (eventPtr == IntPtr.Zero)
            {
                return;
            }

            //obtencion del puntero de los atributos
            IntPtr attributesBase = TelemetryUtils.GetEventAttributesPtr(eventPtr);

            if (attributesBase == IntPtr.Zero)
            {
                TelemetryNative.DestroyEvent(eventPtr);
                return;
            }

            //Comprobacion de valores de los atributos
            if (price < 0 || _sessionID == -1 || _userID == -1 || documentType < 0)
            {
                TelemetryNative.DestroyEvent(eventPtr);
                return;
            }

            //Escritura de los atributos
            TelemetryUtils.WriteAttributeInt64(attributesBase, 0, (int)AtributesNameId.sessionID, _sessionID);
            TelemetryUtils.WriteAttributeInt64(attributesBase, 1, (int)AtributesNameId.userID, _userID);
            TelemetryUtils.WriteAttributeInt32(attributesBase, 2, (int)AtributesNameId.price, price);
            TelemetryUtils.WriteAttributeInt32(attributesBase, 3, (int)AtributesNameId.documentType, documentType);

            //Escritura de directivas externas a atributos
            TelemetryUtils.WriteEventHeader(eventPtr, 4, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

            //TODO Envio del evento
            SubmitEvent(eventPtr);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageID"></param>
        /// <param name="phaseID"></param>
        public void sendQueryPostEvent(int messageID, int phaseID)
        {
            //Establecimiento del numero de atributos
            const int attributeCount = 4;

            //Creacion del evento
            IntPtr eventPtr;

            try
            {
                eventPtr = TelemetryNative.CreateEvent(attributeCount);
            }
            catch (System.EntryPointNotFoundException)
            {
                Debug.LogWarning("Telemetry DLL no disponible o funci�n no encontrada");
                return;
            }
            catch (System.DllNotFoundException)
            {
                Debug.LogWarning("Telemetry DLL no encontrada");
                return;
            }

            if (eventPtr == IntPtr.Zero)
            {
                return;
            }

            //obtencion del puntero de los atributos
            IntPtr attributesBase = TelemetryUtils.GetEventAttributesPtr(eventPtr);

            if (attributesBase == IntPtr.Zero)
            {
                TelemetryNative.DestroyEvent(eventPtr);
                return;
            }

            //Comprobacion de valores de los atributos
            if (messageID < 0 || _sessionID == -1 || _userID == -1 || phaseID < 1)
            {
                TelemetryNative.DestroyEvent(eventPtr);
                return;
            }

            //Escritura de los atributos
            TelemetryUtils.WriteAttributeInt64(attributesBase, 0, (int)AtributesNameId.sessionID, _sessionID);
            TelemetryUtils.WriteAttributeInt64(attributesBase, 1, (int)AtributesNameId.userID, _userID);
            TelemetryUtils.WriteAttributeInt32(attributesBase, 2, (int)AtributesNameId.messageID, messageID);
            TelemetryUtils.WriteAttributeInt32(attributesBase, 3, (int)AtributesNameId.phaseID, phaseID);

            //Escritura de directivas externas a atributos
            TelemetryUtils.WriteEventHeader(eventPtr, 5, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

            //TODO Envio del evento
            SubmitEvent(eventPtr);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageID"></param>
        /// <param name="phaseID"></param>
        public void sendQueryReceivedEvent(int messageID, int phaseID)
        {
            //Establecimiento del numero de atributos
            const int attributeCount = 4;

            //Creacion del evento
            IntPtr eventPtr;

            try
            {
                eventPtr = TelemetryNative.CreateEvent(attributeCount);
            }
            catch (System.EntryPointNotFoundException)
            {
                Debug.LogWarning("Telemetry DLL no disponible o funci�n no encontrada");
                return;
            }
            catch (System.DllNotFoundException)
            {
                Debug.LogWarning("Telemetry DLL no encontrada");
                return;
            }

            if (eventPtr == IntPtr.Zero)
            {
                return;
            }

            //obtencion del puntero de los atributos
            IntPtr attributesBase = TelemetryUtils.GetEventAttributesPtr(eventPtr);

            if (attributesBase == IntPtr.Zero)
            {
                TelemetryNative.DestroyEvent(eventPtr);
                return;
            }

            //Comprobacion de valores de los atributos
            if (messageID < 0 || _sessionID == -1 || _userID == -1 || phaseID < 1)
            {
                TelemetryNative.DestroyEvent(eventPtr);
                return;
            }

            //Escritura de los atributos
            TelemetryUtils.WriteAttributeInt64(attributesBase, 0, (int)AtributesNameId.sessionID, _sessionID);
            TelemetryUtils.WriteAttributeInt64(attributesBase, 1, (int)AtributesNameId.userID, _userID);
            TelemetryUtils.WriteAttributeInt32(attributesBase, 2, (int)AtributesNameId.messageID, messageID);
            TelemetryUtils.WriteAttributeInt32(attributesBase, 3, (int)AtributesNameId.phaseID, phaseID);

            //Escritura de directivas externas a atributos
            TelemetryUtils.WriteEventHeader(eventPtr, 6, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

            //TODO Envio del evento
            SubmitEvent(eventPtr);
        }

        void Start()
        {

            int numSession = 0;

            if (PlayerPrefs.HasKey("SESSION_NUMBER"))
            {
                numSession = PlayerPrefs.GetInt("SESSION_NUMBER") + 1;
                PlayerPrefs.SetInt("SESSION_NUMBER", numSession);
            }
            else
            {
                PlayerPrefs.SetInt("SESSION_NUMBER", 0);
            }

            string filePath = System.IO.Path.Combine(Application.persistentDataPath, "telemetry_events_" + numSession + "_" + _userID + ".json");

            Debug.Log(filePath);

            try
            {
                _trackerHandle = TelemetryNative.CreateTracker(
                            SerializationJson,
                            PersistenceFile,
                            QueueCircularArray,
                            filePath
                );

                if (_trackerHandle == IntPtr.Zero)
                {
                    Debug.Log("No se pudo abrir el archivo");
                }
            }
            catch (System.EntryPointNotFoundException)
            {
                Debug.LogWarning("Telemetry DLL no disponible o funci�n no encontrada");
            }
            catch (System.DllNotFoundException)
            {
                Debug.LogWarning("Telemetry DLL no encontrada");
            }
        
        }

        void Update()
        {
            tryPersistPeriodically();
        }

        private void Awake()
        {
            if (_instance == null) {

                _instance = this;

                _userID = TelemetryUtils.GetUserID();
                _sessionID = _userID + System.Guid.NewGuid().GetHashCode();
            }
            else
            {
                Destroy(this);
            }
        }

        private void OnDestroy()
        {
            if (_trackerHandle == IntPtr.Zero)
            {
                return;
            }

            TelemetryNative.Flush(_trackerHandle);
            TelemetryNative.CloseTracker(_trackerHandle);
            _trackerHandle = IntPtr.Zero;
        }


        void tryPersistPeriodically()
        {
            //solo persistimos si esta marcado el booleano
            if (!_persistPeriodically)
            {
                return;
            }

            //cuenta de tiempo para persistir
            if (_elapsedTime > _persistSeconds)
            {
                _elapsedTime = 0;

                if (_trackerHandle != IntPtr.Zero)
                {
                    TelemetryNative.Flush(_trackerHandle);
                }
            }
            else
            {
                _elapsedTime += Time.deltaTime;
            }

        }


    }



}